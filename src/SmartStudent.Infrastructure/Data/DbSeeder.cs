using Microsoft.AspNetCore.Identity;
using SmartStudent.Core.Entities;
using SmartStudent.Core.Enums;

namespace SmartStudent.Infrastructure.Data;

/// <summary>
/// Seeds the 3 system roles (Admin, Teacher, Student) and a default Admin account on startup.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedRolesAndAdminAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        string[] roleNames =
        [
            UserRole.Admin.ToString(),
            UserRole.Teacher.ToString(),
            UserRole.Student.ToString()
        ];

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Id = Guid.CreateVersion7(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                });
            }
        }

        const string defaultAdminEmail = "admin@smartstudent.edu";
        var existingAdmin = await userManager.FindByEmailAsync(defaultAdminEmail);

        if (existingAdmin is null)
        {
            var adminUser = new ApplicationUser
            {
                FullName = "System Administrator",
                Email = defaultAdminEmail,
                UserName = defaultAdminEmail,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@12345");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToString());
            }
        }
    }
}
