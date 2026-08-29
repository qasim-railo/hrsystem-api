using HRSystem.API.Models.Auth;
using HRSystem.API.Services;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Data;

public static class DevelopmentSeedData
{
    public const string DefaultPassword = "12345678";

    public static async Task SeedAsync(AppDbContext db, AuthService auth)
    {
        var defaultTenant = await db.Tenants.SingleOrDefaultAsync(t => t.Code == "DEFAULT");
        if (defaultTenant == null)
        {
            return;
        }

        var permissionNames = new[]
        {
            "Employees.View",
            "Employees.Create",
            "Employees.Edit",
            "Employees.ChangeStatus",
            "Employees.OverrideDuplicate",
            "Employees.Export",
            "Employees.ViewSensitiveData",
            "Files.View",
            "Files.Upload",
            "Files.Replace",
            "Files.Delete",
            "Files.Restore",
            "Files.Purge",
            "Users.Manage",
            "Workflows.Manage",
            "Platform.Tenants"
        };

        foreach (var name in permissionNames)
        {
            if (!await db.Permissions.AnyAsync(p => p.Name == name))
            {
                db.Permissions.Add(new Permission { Name = name });
            }
        }

        await db.SaveChangesAsync();

        var roleDefinitions = new[]
        {
            new { RoleName = "PeopleOS Super Admin", Permissions = new[] { "Platform.Tenants" } },
            new { RoleName = "Admin", Permissions = new[] { "Employees.View", "Employees.Create", "Employees.Edit", "Employees.ChangeStatus", "Employees.OverrideDuplicate", "Employees.Export", "Employees.ViewSensitiveData", "Files.View", "Files.Upload", "Files.Replace", "Files.Delete", "Files.Restore", "Files.Purge", "Users.Manage", "Workflows.Manage" } },
            new { RoleName = "Company Administrator", Permissions = new[] { "Employees.View", "Employees.Create", "Employees.Edit", "Employees.ChangeStatus", "Employees.OverrideDuplicate", "Employees.Export", "Employees.ViewSensitiveData", "Files.View", "Files.Upload", "Files.Replace", "Files.Delete", "Files.Restore", "Files.Purge", "Users.Manage", "Workflows.Manage" } },
            new { RoleName = "Manager", Permissions = new[] { "Employees.View", "Employees.Create", "Employees.Edit" } },
            new { RoleName = "HR", Permissions = new[] { "Employees.View", "Employees.Create", "Employees.Edit", "Employees.Export", "Files.View", "Files.Upload" } },
            new { RoleName = "Employee", Permissions = new[] { "Employees.View" } }
        };

        foreach (var definition in roleDefinitions)
        {
            var role = await db.Roles.IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Name == definition.RoleName && r.TenantId == defaultTenant.TenantId)
                ?? await db.Roles.IgnoreQueryFilters()
                    .OrderByDescending(r => r.Id)
                    .FirstOrDefaultAsync(r => r.Name == definition.RoleName);

            if (role == null)
            {
                role = new Role { Name = definition.RoleName, TenantId = defaultTenant.TenantId };
                db.Roles.Add(role);
                await db.SaveChangesAsync();
            }
            else if (role.TenantId != defaultTenant.TenantId)
            {
                role.TenantId = defaultTenant.TenantId;
            }

            var permissionIds = await db.Permissions
                .Where(p => definition.Permissions.Contains(p.Name))
                .Select(p => p.Id)
                .ToListAsync();

            foreach (var permissionId in permissionIds)
            {
                var relationshipExists = await db.RolePermissions.IgnoreQueryFilters()
                    .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permissionId);

                if (!relationshipExists)
                {
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId,
                        DataScope = "TenantWide",
                        ScopeIdsJson = "[]"
                    });
                }
            }
        }

        await db.SaveChangesAsync();

        var seededUsers = new[]
        {
            new { Username = "superadmin", RoleName = "PeopleOS Super Admin" },
            new { Username = "admin", RoleName = "Admin" },
            new { Username = "tenantadmin", RoleName = "Company Administrator" },
            new { Username = "manager", RoleName = "Manager" },
            new { Username = "hr", RoleName = "HR" },
            new { Username = "employee", RoleName = "Employee" },
            new { Username = "user1", RoleName = "Employee" }
        };

        foreach (var seededUser in seededUsers)
        {
            var existingUsers = await db.Users.IgnoreQueryFilters()
                .Where(u => u.Username == seededUser.Username)
                .OrderByDescending(u => u.Id)
                .ToListAsync();

            var user = existingUsers.FirstOrDefault(u => u.TenantId == defaultTenant.TenantId)
                ?? existingUsers.FirstOrDefault()
                ?? new AppUser
                {
                    Username = seededUser.Username,
                    TenantId = defaultTenant.TenantId,
                    PasswordHash = auth.HashPassword(DefaultPassword),
                    IsActive = true
                };

            if (user.Id == 0)
            {
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }

            user.TenantId = defaultTenant.TenantId;
            user.Username = seededUser.Username;
            user.PasswordHash = auth.HashPassword(DefaultPassword);
            user.IsActive = true;

            foreach (var duplicate in existingUsers.Where(existing => existing.Id != user.Id).ToList())
            {
                db.Users.Remove(duplicate);
            }

            var role = await db.Roles.IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Name == seededUser.RoleName && r.TenantId == defaultTenant.TenantId)
                ?? await db.Roles.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(r => r.Name == seededUser.RoleName);

            if (role != null)
            {
                var roleExists = await db.UserRoles.IgnoreQueryFilters()
                    .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);

                if (!roleExists)
                {
                    db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
                }
            }
        }

        await db.SaveChangesAsync();
    }
}
