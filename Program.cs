using HRSystem.API.Data;
using HRSystem.API.Helpers;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services;
using HRSystem.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;



var builder = WebApplication.CreateBuilder(args);
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

builder.Services.AddAuthorization();



//Mapping Profiles 
builder.Services.AddAutoMapper(typeof(MappingProfile));

//Resistering AppDBContext to Program.CS
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// registering Employee service 

//Custom Services
        builder.Services.AddScoped<IEmployeeService, EmployeeService>();
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



// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAngularApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
