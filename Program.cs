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
            ValidIssuer = jwtSettings?.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings?.Audience,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings?.Key ?? "")),
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
builder.Services.AddScoped<TenantWorkingDayService>();
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
    var platformTenant = db.Tenants.SingleOrDefault(t => t.Code == "PEOPLEOS");
    if (platformTenant == null)
    {
        platformTenant = new HRSystem.API.Models.Tenant
        {
            Name = "PeopleOS Platform",
            Code = "PEOPLEOS",
            Status = "Active",
            LifecycleStatus = "Active",
            Country = "QA",
            Currency = "QAR",
            TimeZone = "Asia/Qatar",
            CountryCode = "QA",
            CurrencyCode = "QAR",
            TimeZoneId = "Asia/Qatar",
            PlanId = 1,
            PlanName = "PeopleOS Essential"
        };
        db.Tenants.Add(platformTenant);
        db.SaveChanges();
    }

    var permissionNames = new[] { "Employees.View", "Employees.Create", "Employees.Edit", "Employees.ChangeStatus", "Employees.OverrideDuplicate", "Employees.Export", "Employees.ViewSensitiveData", "Files.View", "Files.Upload", "Files.Replace", "Files.Delete", "Files.Restore", "Files.Purge", "Users.Manage", "Workflows.Manage" };
    permissionNames = permissionNames.Append("Platform.Tenants").ToArray();
    foreach (var name in permissionNames)
        if (!db.Permissions.Any(p => p.Name == name)) db.Permissions.Add(new Permission { Name = name });
    db.SaveChanges();

    var platformRole = db.Roles.IgnoreQueryFilters()
        .SingleOrDefault(r => r.Name == "PeopleOS Super Admin" && r.TenantId == platformTenant.TenantId)
        ?? new Role { Name = "PeopleOS Super Admin", TenantId = platformTenant.TenantId };
    if (platformRole.Id == 0)
    {
        db.Roles.Add(platformRole);
        db.SaveChanges();
    }

    var platformPermission = db.Permissions.Single(p => p.Name == "Platform.Tenants");
    if (!db.RolePermissions.IgnoreQueryFilters().Any(rp => rp.RoleId == platformRole.Id && rp.PermissionId == platformPermission.Id))
    {
        db.RolePermissions.Add(new RolePermission { RoleId = platformRole.Id, PermissionId = platformPermission.Id });
        db.SaveChanges();
    }

    var superAdmin = db.Users.IgnoreQueryFilters()
        .SingleOrDefault(u => u.Username == "qasim.railo@gmail.com")
        ?? new AppUser
        {
            Username = "qasim.railo@gmail.com",
            PasswordHash = auth.HashPassword(DevelopmentDefaultPassword),
            TenantId = platformTenant.TenantId,
            IsActive = true
        };
    if (superAdmin.Id == 0)
    {
        db.Users.Add(superAdmin);
        db.SaveChanges();
    }
    else
    {
        superAdmin.TenantId = platformTenant.TenantId;
        superAdmin.Username = "qasim.railo@gmail.com";
        superAdmin.PasswordHash = auth.HashPassword(DevelopmentDefaultPassword);
        superAdmin.IsActive = true;
    }

    if (!db.UserRoles.IgnoreQueryFilters().Any(ur => ur.UserId == superAdmin.Id && ur.RoleId == platformRole.Id))
        db.UserRoles.Add(new UserRole { UserId = superAdmin.Id, RoleId = platformRole.Id });
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
