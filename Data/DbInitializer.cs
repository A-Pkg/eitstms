using EITSTMS.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace EITSTMS.Data
{
    /// <summary>
    /// 数据库初始化器，用于在应用启动时创建默认角色和管理员用户。
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// 初始化角色数据
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles =
            {
                "Employee",
                "Technician",
                "Admin"
            };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 从配置中获取管理员用户名和密码(目前是用户机密)
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            string? adminUserName = configuration["SeedAdmin:UserName"];
            string? adminPassword = configuration["SeedAdmin:Password"];
            if (string.IsNullOrWhiteSpace(adminUserName) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException("SeedAdmin 配置缺失。");
            }

            var admin = await userManager.FindByNameAsync(adminUserName);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminUserName,
                    EmployeeNumber = "ADMIN001",
                    Name = "System Administrator",
                    IsActive = true,
                    IsSystemAdmin = true,
                };

                var result = await userManager.CreateAsync(admin, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}