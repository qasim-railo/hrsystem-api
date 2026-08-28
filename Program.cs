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
    var platformRole = db.Roles.Include(r => r.RolePermissions).SingleOrDefault(r => r.Name == "PeopleOS Super Admin") ?? new Role { Name = "PeopleOS Super Admin" };
    if (platformRole.Id == 0) db.Roles.Add(platformRole);
    db.SaveChanges();
    var platformPermission = db.Permissions.Single(p => p.Name == "Platform.Tenants");
    if (!platformRole.RolePermissions.Any(rp => rp.PermissionId == platformPermission.Id))
        platformRole.RolePermissions.Add(new RolePermission { PermissionId = platformPermission.Id });
    var superAdmin = db.Users.Include(u => u.UserRoles).SingleOrDefault(u => u.Username == "superadmin");
    if (superAdmin == null)
    {
        superAdmin = new AppUser { Username = "superadmin", PasswordHash = auth.HashPassword(DevelopmentDefaultPassword), TenantId = tenant.TenantId };
        db.Users.Add(superAdmin);
        db.SaveChanges();
    }
    else
    {
        superAdmin.TenantId = tenant.TenantId;
        superAdmin.PasswordHash = auth.HashPassword(DevelopmentDefaultPassword);
    }
    if (!superAdmin.UserRoles.Any(ur => ur.RoleId == platformRole.Id))
        superAdmin.UserRoles.Add(new UserRole { RoleId = platformRole.Id });
    db.SaveChanges();
    var adminRole = db.Roles.Include(r => r.RolePermissions)
        .FirstOrDefault(r => r.Name == "Admin" && r.TenantId == tenant.TenantId) ?? new Role { Name = "Admin" };
    adminRole.TenantId = tenant.TenantId;
    if (adminRole.Id == 0) db.Roles.Add(adminRole);
    db.SaveChanges();
    foreach (var platformAssignment in adminRole.RolePermissions.Where(rp => rp.PermissionId == platformPermission.Id).ToList())
        db.RolePermissions.Remove(platformAssignment);
    foreach (var permission in db.Permissions.Where(p => permissionNames.Contains(p.Name) && p.Name != "Platform.Tenants"))
        if (!adminRole.RolePermissions.Any(rp => rp.PermissionId == permission.Id)) adminRole.RolePermissions.Add(new RolePermission { PermissionId = permission.Id });
    var companyAdministratorRole = db.Roles.Include(r => r.RolePermissions)
        .SingleOrDefault(r => r.Name == "Company Administrator" && r.TenantId == tenant.TenantId);
    if (companyAdministratorRole == null)
    {
        companyAdministratorRole = new Role { Name = "Company Administrator", TenantId = tenant.TenantId };
        db.Roles.Add(companyAdministratorRole);
        db.SaveChanges();
    }
    foreach (var permission in db.Permissions.Where(p => permissionNames.Contains(p.Name) && p.Name != "Platform.Tenants"))
        if (!companyAdministratorRole.RolePermissions.Any(rp => rp.PermissionId == permission.Id))
            companyAdministratorRole.RolePermissions.Add(new RolePermission { PermissionId = permission.Id });
    db.SaveChanges();
    var admin = db.Users.Include(u => u.UserRoles).SingleOrDefault(u => u.Username == "admin");
    if (admin == null)
    {
        admin = new AppUser { Username = "admin", PasswordHash = auth.HashPassword(DevelopmentDefaultPassword), TenantId = tenant.TenantId };
        db.Users.Add(admin);
        db.SaveChanges();
    }
    else
    {
        admin.TenantId = tenant.TenantId;
        admin.PasswordHash = auth.HashPassword(DevelopmentDefaultPassword);
    }
    if (!admin.UserRoles.Any(ur => ur.RoleId == adminRole.Id)) admin.UserRoles.Add(new UserRole { RoleId = adminRole.Id });
    if (!admin.UserRoles.Any(ur => ur.RoleId == companyAdministratorRole.Id))
        admin.UserRoles.Add(new UserRole { RoleId = companyAdministratorRole.Id });
    db.SaveChanges();
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
