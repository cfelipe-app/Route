using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Route.Backend.Identity;
using Route.Backend.Security;
using System.Security.Claims;

namespace Route.Backend.Data
{
    public static class SeedIdentity
    {
        public static async Task RunAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            // Roles
            string[] roles = { RoleNames.Admin, RoleNames.Planner, RoleNames.ProviderAdmin, RoleNames.Driver };
            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // Admin
            var adminEmail = "admin@route.local";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator"
                };
                await userManager.CreateAsync(admin, "Admin123$");
                await userManager.AddToRoleAsync(admin, RoleNames.Admin);
            }

            // ProviderAdmin (primer proveedor)
            var provider = await context.Providers.OrderBy(p => p.Id).FirstOrDefaultAsync();
            if (provider is not null)
            {
                var email = "prov.admin@route.local";
                var u = await userManager.FindByEmailAsync(email);
                if (u is null)
                {
                    u = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FullName = "Provider Admin",
                        ProviderId = provider.Id
                    };
                    await userManager.CreateAsync(u, "Prov123$");
                    await userManager.AddToRoleAsync(u, RoleNames.ProviderAdmin);
                    await userManager.AddClaimAsync(u, new Claim("provider_id", provider.Id.ToString()));
                }
            }

            // Driver (primer driver)
            var driver = await context.Drivers.OrderBy(d => d.Id).FirstOrDefaultAsync();
            if (driver is not null)
            {
                var email = "driver@route.local";
                var u = await userManager.FindByEmailAsync(email);
                if (u is null)
                {
                    u = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FullName = driver.FullName,
                        DriverId = driver.Id,
                        ProviderId = driver.ProviderId
                    };
                    await userManager.CreateAsync(u, "Driver123$");
                    await userManager.AddToRoleAsync(u, RoleNames.Driver);
                    await userManager.AddClaimAsync(u, new Claim("driver_id", driver.Id.ToString()));
                    await userManager.AddClaimAsync(u, new Claim("provider_id", driver.ProviderId.ToString()));
                }
            }
        }
    }
}