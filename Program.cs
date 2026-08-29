using HRSystem.API.Data;
using HRSystem.API.Helpers;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services;
using HRSystem.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using HRSystem.API.Tenancy;

ExcelPackage.License.SetNonCommercialOrganization("PeopleOS Development");


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<CurrentTenant>();
builder.Services.AddScoped<ICurrentTenant>(sp => sp.GetRequiredService<CurrentTenant>());
// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
//JWT Settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in new[] { "Employees.View", "Employees.Create", "Employees.Edit", "Employees.ChangeStatus", "Employees.OverrideDuplicate", "Employees.Export", "Employees.ViewSensitiveData", "Files.View", "Files.Upload", "Files.Replace", "Files.Delete", "Files.Restore", "Files.Purge" })
        options.AddPolicy(permission, policy => policy.RequireClaim("permission", permission));
    options.AddPolicy("Users.Manage", policy => policy.RequireClaim("permission", "Users.Manage"));
    options.AddPolicy("Workflows.Manage", policy => policy.RequireClaim("permission", "Workflows.Manage"));
    options.AddPolicy("Platform.Tenants", policy => policy.RequireClaim("permission", "Platform.Tenants"));
});

// register audit service
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ISubscriptionCheckService, SubscriptionCheckService>();
builder.Services.AddScoped<BillingService>();



//Mapping Profiles 
builder.Services.AddAutoMapper(typeof(MappingProfile));

//Resistering AppDBContext to Program.CS
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// registering Employee service 

//Custom Services
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<CustomFieldService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICompaniesService, CompaniesService>();
builder.Services.AddScoped<IDepartmentsService, DepartmentsService>();
builder.Services.AddScoped<IEmploymentDetailsService, EmploymentDetailsService>();
builder.Services.AddScoped<IEmployeeDocumentsService, EmployeeDocumentsService>();
builder.Services.AddScoped<IAssetsService, AssetsService>();
builder.Services.AddScoped<IEmployeeAssetsService, EmployeeAssetsService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IEmployeeShiftService, EmployeeShiftService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<ILeaveRequestService, LeaveRequestService>();
builder.Services.AddScoped<IFinalSettlementService, FinalSettlementService>();
builder.Services.AddScoped<IGratuityReportService, GratuityReportService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IIncrementHistoryService, IncrementHistoryService>();
builder.Services.AddScoped<CloudinaryService>();
builder.Services.Configure<FileStorageOptions>(
    builder.Configuration.GetSection(FileStorageOptions.SectionName));
builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IReferenceNumberService, ReferenceNumberService>();
builder.Services.AddScoped<OvertimePolicyService>();
builder.Services.AddSingleton<IMalwareScanner, NoOpMalwareScanner>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<INotificationService, NotificationService>();



// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
const string DevelopmentDefaultPassword = "12345678";
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var auth = scope.ServiceProvider.GetRequiredService<AuthService>();
    var tenant = db.Tenants.SingleOrDefault(t => t.Code == "DEFAULT");
    if (tenant == null)
    {
        tenant = new HRSystem.API.Models.Tenant
        {
            Name = "Default Tenant",
            Code = "DEFAULT",
            Country = "QA",
            Currency = "QAR",
            TimeZone = "Asia/Qatar",
            CountryCode = "QA",
            CurrencyCode = "QAR",
            TimeZoneId = "Asia/Qatar"
        };
        db.Tenants.Add(tenant);
        db.SaveChanges();
    }
    foreach (var existingTenant in db.Tenants.ToList())
    {
        if (!db.Subscriptions.Any(s => s.TenantId == existingTenant.TenantId))
        {
            var now = DateTime.UtcNow;
            db.Subscriptions.Add(new HRSystem.API.Models.Subscription
            {
                TenantId = existingTenant.TenantId,
                PlanId = existingTenant.PlanId,
                Status = Enum.TryParse<HRSystem.API.Models.SubscriptionStatus>(existingTenant.Status, true, out var status)
                    ? status : HRSystem.API.Models.SubscriptionStatus.Trial,
                StartDate = existingTenant.CreatedAt,
                RenewalDate = existingTenant.TrialEndDate ?? now.AddMonths(1),
                TrialStartDate = existingTenant.TrialStartDate,
                TrialEndDate = existingTenant.TrialEndDate,
                BillingCycle = "Monthly",
                Notes = "Created during subscription lifecycle initialization."
            });
        }
    }
    db.SaveChanges();
    var permissionNames = new[] { "Employees.View", "Employees.Create", "Employees.Edit", "Employees.ChangeStatus", "Employees.OverrideDuplicate", "Employees.Export", "Employees.ViewSensitiveData", "Files.View", "Files.Upload", "Files.Replace", "Files.Delete", "Files.Restore", "Files.Purge", "Users.Manage", "Workflows.Manage" };
    permissionNames = permissionNames.Append("Platform.Tenants").ToArray();
    foreach (var name in permissionNames)
        if (!db.Permissions.Any(p => p.Name == name)) db.Permissions.Add(new Permission { Name = name });
    db.SaveChanges();

    var allRolePermissions = db.RolePermissions.IgnoreQueryFilters().ToList();
    var allUserRoles = db.UserRoles.IgnoreQueryFilters().ToList();

    void RemoveDuplicateRolePermissions(Role role)
    {
        var duplicatePermissions = allRolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .GroupBy(rp => rp.PermissionId)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.Skip(1))
            .ToList();

        foreach (var duplicate in duplicatePermissions)
        {
            db.RolePermissions.Remove(duplicate);
            allRolePermissions.Remove(duplicate);
        }
    }

    void EnsureRolePermission(Role role, int permissionId)
    {
        RemoveDuplicateRolePermissions(role);

        var existingPermissionIds = allRolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .Select(rp => rp.PermissionId)
            .ToHashSet();

        if (!existingPermissionIds.Contains(permissionId))
        {
            var newRolePermission = new RolePermission { RoleId = role.Id, PermissionId = permissionId };
            db.RolePermissions.Add(newRolePermission);
            allRolePermissions.Add(newRolePermission);
        }
    }

    void EnsureUserRole(AppUser user, int roleId)
    {
        var duplicateUserRoles = allUserRoles
            .Where(ur => ur.UserId == user.Id)
            .GroupBy(ur => ur.RoleId)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.Skip(1))
            .ToList();

        foreach (var duplicate in duplicateUserRoles)
        {
            db.UserRoles.Remove(duplicate);
            allUserRoles.Remove(duplicate);
        }

        var existingRoleIds = allUserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToHashSet();

        if (!existingRoleIds.Contains(roleId))
        {
            var newUserRole = new UserRole { UserId = user.Id, RoleId = roleId };
            db.UserRoles.Add(newUserRole);
            allUserRoles.Add(newUserRole);
        }
    }

    var existingPlatformRoles = db.Roles.IgnoreQueryFilters()
        .Where(r => r.Name == "PeopleOS Super Admin")
        .OrderByDescending(r => r.Id)
        .ToList();
    var platformRole = existingPlatformRoles.FirstOrDefault(r => r.TenantId == tenant.TenantId)
        ?? existingPlatformRoles.FirstOrDefault()
        ?? new Role { Name = "PeopleOS Super Admin", TenantId = tenant.TenantId };
    if (platformRole.Id == 0)
    {
        db.Roles.Add(platformRole);
    }
    else if (platformRole.TenantId != tenant.TenantId)
    {
        platformRole.TenantId = tenant.TenantId;
    }

    foreach (var duplicate in existingPlatformRoles.Where(r => r.Id != platformRole.Id).ToList())
        db.Roles.Remove(duplicate);

    db.SaveChanges();

    var platformPermission = db.Permissions.Single(p => p.Name == "Platform.Tenants");
    EnsureRolePermission(platformRole, platformPermission.Id);
    var existingSuperAdmins = db.Users.IgnoreQueryFilters()
        .Where(u => u.Username == "superadmin")
        .OrderByDescending(u => u.Id)
        .ToList();
    var superAdmin = existingSuperAdmins.FirstOrDefault(u => u.TenantId == tenant.TenantId)
        ?? existingSuperAdmins.FirstOrDefault()
        ?? new AppUser { Username = "superadmin", PasswordHash = auth.HashPassword(DevelopmentDefaultPassword), TenantId = tenant.TenantId };
    if (superAdmin.Id == 0)
    {
        db.Users.Add(superAdmin);
        db.SaveChanges();
    }
    else
    {
        superAdmin.TenantId = tenant.TenantId;
        superAdmin.PasswordHash = auth.HashPassword(DevelopmentDefaultPassword);
    }
    foreach (var duplicate in existingSuperAdmins.Where(u => u.Id != superAdmin.Id).ToList())
        db.Users.Remove(duplicate);
    EnsureUserRole(superAdmin, platformRole.Id);
    db.SaveChanges();
    var adminRole = db.Roles.IgnoreQueryFilters().Include(r => r.RolePermissions)
        .FirstOrDefault(r => r.Name == "Admin" && r.TenantId == tenant.TenantId) ?? new Role { Name = "Admin", TenantId = tenant.TenantId };
    if (adminRole.Id == 0) db.Roles.Add(adminRole);
    adminRole.TenantId = tenant.TenantId;
    db.SaveChanges();
    RemoveDuplicateRolePermissions(adminRole);
    foreach (var platformAssignment in db.RolePermissions.IgnoreQueryFilters().Where(rp => rp.RoleId == adminRole.Id && rp.PermissionId == platformPermission.Id).ToList())
        db.RolePermissions.Remove(platformAssignment);
    foreach (var permission in db.Permissions.Where(p => permissionNames.Contains(p.Name) && p.Name != "Platform.Tenants"))
        EnsureRolePermission(adminRole, permission.Id);
    var companyAdministratorRole = db.Roles.IgnoreQueryFilters().Include(r => r.RolePermissions)
        .SingleOrDefault(r => r.Name == "Company Administrator" && r.TenantId == tenant.TenantId) ?? new Role { Name = "Company Administrator", TenantId = tenant.TenantId };
    if (companyAdministratorRole.Id == 0)
    {
        db.Roles.Add(companyAdministratorRole);
        db.SaveChanges();
    }
    companyAdministratorRole.TenantId = tenant.TenantId;
    RemoveDuplicateRolePermissions(companyAdministratorRole);
    foreach (var permission in db.Permissions.Where(p => permissionNames.Contains(p.Name) && p.Name != "Platform.Tenants"))
        EnsureRolePermission(companyAdministratorRole, permission.Id);
    db.SaveChanges();
    var existingAdmins = db.Users.IgnoreQueryFilters()
        .Where(u => u.Username == "admin")
        .OrderByDescending(u => u.Id)
        .ToList();
    var admin = existingAdmins.FirstOrDefault(u => u.TenantId == tenant.TenantId)
        ?? existingAdmins.FirstOrDefault()
        ?? new AppUser { Username = "admin", PasswordHash = auth.HashPassword(DevelopmentDefaultPassword), TenantId = tenant.TenantId };
    if (admin.Id == 0)
    {
        db.Users.Add(admin);
        db.SaveChanges();
    }
    else
    {
        admin.TenantId = tenant.TenantId;
        admin.PasswordHash = auth.HashPassword(DevelopmentDefaultPassword);
    }
    foreach (var duplicate in existingAdmins.Where(u => u.Id != admin.Id).ToList())
        db.Users.Remove(duplicate);
    EnsureUserRole(admin, adminRole.Id);
    EnsureUserRole(admin, companyAdministratorRole.Id);
    db.SaveChanges();

    await DevelopmentSeedData.SeedAsync(db, auth);
}
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable CORS
app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();

app.MapControllers().RequireAuthorization();

app.Run();
