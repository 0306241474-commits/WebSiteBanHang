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
            EnsureSucceeded(roleResult, "Tạo vai trò Admin thất bại.");
        }

        var username = configuration["AdminBootstrap:Username"];
        var password = configuration["AdminBootstrap:Password"];
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "Chưa tạo tài khoản Admin. Hãy cấu hình AdminBootstrap:Username và AdminBootstrap:Password bằng User Secrets hoặc biến môi trường.");
            return;
        }

        var admin = await userManager.FindByNameAsync(username);
        if (admin is null)
        {
            admin = new IdentityUser { UserName = username, Email = username };
            var userResult = await userManager.CreateAsync(admin, password);
            EnsureSucceeded(userResult, "Tạo tài khoản Admin thất bại.");
        }

        if (!await userManager.IsInRoleAsync(admin, AdminRole))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, AdminRole);
            EnsureSucceeded(roleResult, "Gán vai trò Admin thất bại.");
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
