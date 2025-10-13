using MudBlazor.Services;
using Route.Frontend.Components;
using Route.Frontend.Repositories;
using Route.Shared.Services.Api;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri("https://localhost:7241") });
builder.Services.AddScoped<IRepository, Repository>();

//builder.Services.AddScoped<ProvidersClient>();
//builder.Services.AddScoped<VehiclesClient>();

builder.Services.AddHttpClient<ProvidersClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7241"));

builder.Services.AddHttpClient<VehiclesClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7241"));

//builder.Services.AddHttpClient<ProvidersClient>(c =>
//    c.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();