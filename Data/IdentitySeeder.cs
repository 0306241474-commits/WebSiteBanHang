using Microsoft.AspNetCore.Identity;

namespace WebBanHang.Data;

public static class IdentitySeeder
{
    public const string AdminRole = "Admin";

    public static async Task SeedAdminAsync(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(AdminRole));
            EnsureSucceeded(roleResult, "Failed to create the Admin role.");
        }

        var username = configuration["AdminBootstrap:Username"];
        var password = configuration["AdminBootstrap:Password"];
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "The Admin account was not created. Configure AdminBootstrap:Username and AdminBootstrap:Password using User Secrets or environment variables.");
            return;
        }

        var admin = await userManager.FindByNameAsync(username);
        if (admin is null)
        {
            admin = new IdentityUser { UserName = username, Email = username };
            var userResult = await userManager.CreateAsync(admin, password);
            EnsureSucceeded(userResult, "Failed to create the Admin account.");
        }

        if (!await userManager.IsInRoleAsync(admin, AdminRole))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, AdminRole);
            EnsureSucceeded(roleResult, "Failed to assign the Admin role.");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"{message} {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }
}
