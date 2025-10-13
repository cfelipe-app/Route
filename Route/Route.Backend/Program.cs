using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Route.Backend.Data;
using Route.Backend.Identity;
using Route.Backend.Repositories.Implementations;
using Route.Backend.Repositories.Interfaces;
using Route.Backend.Security;
using Route.Backend.UnitsOfWork.Implementations;
using Route.Backend.UnitsOfWork.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== Controllers + JSON (evita ciclos y oculta nulls) =====
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ===== DbContext =====
// RECUERDA: DataContext debe heredar de IdentityDbContext<ApplicationUser> para que Identity funcione.
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocalConnection")));

// ===== Repos / UoW =====
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericUnitOfWork<>), typeof(GenericUnitOfWork<>));

// ===== Seeder de negocio =====
builder.Services.AddTransient<SeedDb>();

// ===== Identity + Roles =====
builder.Services
    .AddIdentityCore<ApplicationUser>(opt =>
    {
        opt.User.RequireUniqueEmail = true;
        opt.Password.RequiredLength = 6;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

// ===== JWT =====
// Valida que exista la clave para evitar el error “Value cannot be null (Parameter 's')”
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Falta Jwt:Key en appsettings.json (o en User Secrets/env).");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    });

// ===== Authorization Policies =====
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole(RoleNames.Admin));
    options.AddPolicy("AdminOrPlanner", p => p.RequireRole(RoleNames.Admin, RoleNames.Planner));
    options.AddPolicy("ProviderOrAdmin", p =>
        p.RequireAssertion(ctx =>
        {
            var isAdmin = ctx.User.IsInRole(RoleNames.Admin);
            var isProvider = ctx.User.IsInRole(RoleNames.ProviderAdmin);
            var hasProviderId = ctx.User.HasClaim(c => c.Type == "provider_id");
            return isAdmin || (isProvider && hasProviderId);
        }));
    options.AddPolicy("DriverOnly", p =>
        p.RequireRole(RoleNames.Driver).RequireClaim("driver_id"));
});

// ===== CORS (desde appsettings o “open” en dev) =====
var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Default", p =>
    {
        if (origins.Length == 0 || Array.Exists(origins, o => o == "*"))
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); // DEV
        else
            p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    });
});

// ===== Swagger + JWT (Swashbuckle) =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Route API", Version = "v1" });

    // Evita conflictos cuando tienes controllers genéricos/repetidos
    c.ResolveConflictingActions(apiDescs => apiDescs.First());
    c.CustomSchemaIds(t => t.FullName);

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Use: Bearer {token}",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(jwtScheme.Reference.Id, jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});

var app = builder.Build();

// ===== Migración + Seed (una sola vez al arrancar) =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dataContext = services.GetRequiredService<DataContext>();
    await dataContext.Database.MigrateAsync();

    // Seed de roles/usuarios (usa Admin.Email/Password del appsettings)
    await SeedIdentity.RunAsync(services);

    // Seed de datos de negocio
    var configuration = services.GetRequiredService<IConfiguration>();
    var seedDb = services.GetRequiredService<SeedDb>();
    var resetDatabase = configuration.GetValue("Seed:Reset", false);
    await seedDb.SeedAsync(resetDatabase);
}

// ===== Pipeline =====
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Route API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors("Default");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();