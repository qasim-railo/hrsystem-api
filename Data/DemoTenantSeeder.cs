using HRSystem.API.Models;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Data;

/// <summary>
/// Seeds two fully populated demo tenants/companies with realistic fictional data
/// across every major PeopleOS module, for demo/showcase purposes only.
/// Safe to re-run: skips a tenant if it already exists (checked by Tenant.Code).
/// </summary>
public static class DemoTenantSeeder
{
    public const string DefaultPassword = "12345678";

    private static readonly string[] PermissionNames =
    {
        "Employees.View", "Employees.Create", "Employees.Edit", "Employees.ChangeStatus",
        "Employees.OverrideDuplicate", "Employees.Export", "Employees.ViewSensitiveData",
        "Files.View", "Files.Upload", "Files.Replace", "Files.Delete", "Files.Restore", "Files.Purge",
        "Users.Manage", "Workflows.Manage"
    };

    private class DemoCompanyDefinition
    {
        public string TenantCode = "";
        public string TenantName = "";
        public string CompanyName = "";
        public string Country = "";
        public string Currency = "";
        public string TimeZone = "";
        public int PlanId;
        public string[] Branches = Array.Empty<string>();
        public string[] Departments = Array.Empty<string>();
        public string UserPrefix = "";
        public string EmployeeCodePrefix = "";
    }

    public static async Task SeedAsync(AppDbContext db, AuthService auth)
    {
        var definitions = new[]
        {
            new DemoCompanyDefinition
            {
                TenantCode = "ACME",
                TenantName = "Acme Facilities Group",
                CompanyName = "Acme Facilities Group LLC",
                Country = "QA",
                Currency = "QAR",
                TimeZone = "Asia/Qatar",
                PlanId = 2, // Professional
                Branches = new[] { "Doha Head Office", "Al Khor Site" },
                Departments = new[] { "Human Resources", "Finance", "Operations", "Engineering", "Sales" },
                UserPrefix = "acme",
                EmployeeCodePrefix = "ACM"
            },
            new DemoCompanyDefinition
            {
                TenantCode = "GLOBEX",
                TenantName = "Globex Retail Holdings",
                CompanyName = "Globex Retail Holdings W.L.L.",
                Country = "AE",
                Currency = "AED",
                TimeZone = "Asia/Dubai",
                PlanId = 1, // Essential
                Branches = new[] { "Dubai Main Branch", "Abu Dhabi Branch" },
                Departments = new[] { "Human Resources", "Retail Operations", "Warehouse", "Marketing" },
                UserPrefix = "globex",
                EmployeeCodePrefix = "GLX"
            }
        };

        foreach (var name in PermissionNames)
        {
            if (!await db.Permissions.AnyAsync(p => p.Name == name))
            {
                db.Permissions.Add(new Permission { Name = name });
            }
        }
        await db.SaveChangesAsync();

        foreach (var def in definitions)
        {
            var existing = await db.Tenants.SingleOrDefaultAsync(t => t.Code == def.TenantCode);
            if (existing != null)
            {
                continue; // already seeded, skip to keep this idempotent
            }

            await SeedCompanyAsync(db, auth, def);
        }
    }

    private static async Task SeedCompanyAsync(AppDbContext db, AuthService auth, DemoCompanyDefinition def)
    {
        var now = DateTime.UtcNow;

        var tenant = new Tenant
        {
            Name = def.TenantName,
            Code = def.TenantCode,
            Status = "Active",
            LifecycleStatus = "Active",
            Country = def.Country,
            Currency = def.Currency,
            TimeZone = def.TimeZone,
            CountryCode = def.Country,
            CurrencyCode = def.Currency,
            TimeZoneId = def.TimeZone,
            DisplayName = def.TenantName,
            PlanId = def.PlanId,
            PlanName = def.PlanId == 2 ? "PeopleOS Professional" : "PeopleOS Essential",
            CreatedAt = now
        };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        db.Subscriptions.Add(new Subscription
        {
            TenantId = tenant.TenantId,
            PlanId = def.PlanId,
            Status = SubscriptionStatus.Active,
            StartDate = now.AddMonths(-3),
            RenewalDate = now.AddMonths(9),
            BillingCycle = "Monthly",
            Notes = "Demo subscription seeded for showcase purposes."
        });

        var company = new Company
        {
            TenantId = tenant.TenantId,
            Name = def.CompanyName,
            Address = $"{def.Branches[0]}, {def.Country}",
            LegalName = def.CompanyName,
            TradeName = def.TenantName,
            CommercialRegistrationNumber = $"CR-{def.TenantCode}-10245",
            Industry = def.TenantCode == "ACME" ? "Facilities Management" : "Retail",
            EmployeeCount = 0,
            Country = def.Country,
            Phone = "+974 4400 1234",
            Email = $"info@{def.UserPrefix}.demo",
            Website = $"https://www.{def.UserPrefix}.demo",
            ContactPerson = "Operations Office",
            ContactPhone = "+974 4400 5678",
            IsActive = true,
            EffectiveFrom = now.AddYears(-2)
        };
        db.Companies.Add(company);
        await db.SaveChangesAsync();

        var branches = new List<Branch>();
        foreach (var branchName in def.Branches)
        {
            var branch = new Branch
            {
                TenantId = tenant.TenantId,
                CompanyId = company.CompanyId,
                Name = branchName,
                Code = branchName.Substring(0, Math.Min(3, branchName.Length)).ToUpperInvariant(),
                Address = $"{branchName}, {def.Country}",
                IsActive = true,
                EffectiveFrom = now.AddYears(-2)
            };
            db.Branches.Add(branch);
            branches.Add(branch);
        }
        await db.SaveChangesAsync();

        var departments = new List<Department>();
        foreach (var deptName in def.Departments)
        {
            var dept = new Department
            {
                TenantId = tenant.TenantId,
                CompanyId = company.CompanyId,
                BranchId = branches[0].BranchId,
                Name = deptName,
                Description = $"{deptName} department",
                IsActive = true,
                EffectiveFrom = now.AddYears(-2)
            };
            db.Department.Add(dept);
            departments.Add(dept);
        }
        await db.SaveChangesAsync();

        // Leave types
        var leaveTypeNames = new[] { "Annual", "Sick", "Emergency", "Unpaid" };
        var leaveTypeDays = new[] { 21, 14, 5, 0 };
        for (int i = 0; i < leaveTypeNames.Length; i++)
        {
            db.TenantLeaveTypes.Add(new TenantLeaveType
            {
                TenantId = tenant.TenantId,
                Name = leaveTypeNames[i],
                DefaultDays = leaveTypeDays[i],
                AccrualMethod = "Annual",
                CarryForwardLimit = 5,
                AllowEncashment = leaveTypeNames[i] == "Annual",
                MinimumServiceDays = 90,
                DocumentRequired = leaveTypeNames[i] == "Sick",
                ApprovalRequired = true,
                EmployeeCategory = "*",
                EffectiveFrom = now.AddYears(-2),
                IsActive = true
            });
        }

        // Shifts
        var morningShift = new Shift { TenantId = tenant.TenantId, Name = "Morning Shift", StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(17, 0, 0), Type = "Staff" };
        var nightShift = new Shift { TenantId = tenant.TenantId, Name = "Night Shift", StartTime = new TimeSpan(20, 0, 0), EndTime = new TimeSpan(5, 0, 0), Type = "Labor" };
        db.Shifts.AddRange(morningShift, nightShift);
        await db.SaveChangesAsync();

        // Assets
        var assetCatalog = new (string Name, string Category)[]
        {
            ("Dell Latitude Laptop", "IT Equipment"),
            ("iPhone 14", "Mobile Device"),
            ("Toyota Hilux", "Vehicle"),
            ("Office Desk Set", "Furniture"),
            ("Safety Helmet", "PPE")
        };
        var assets = new List<Asset>();
        int assetSeq = 1;
        foreach (var (assetName, category) in assetCatalog)
        {
            var asset = new Asset
            {
                TenantId = tenant.TenantId,
                Name = assetName,
                Category = category,
                AssetCode = $"{def.EmployeeCodePrefix}-AST-{assetSeq:000}",
                PurchaseDate = now.AddMonths(-Random.Shared.Next(3, 24))
            };
            db.Assets.Add(asset);
            assets.Add(asset);
            assetSeq++;
        }
        await db.SaveChangesAsync();

        // Roles per tenant
        var roleDefinitions = new (string RoleName, string[] Permissions)[]
        {
            ("Company Administrator", PermissionNames),
            ("HR", new[] { "Employees.View", "Employees.Create", "Employees.Edit", "Employees.Export", "Files.View", "Files.Upload" }),
            ("Payroll", new[] { "Employees.View", "Employees.Export" }),
            ("Manager", new[] { "Employees.View", "Employees.Create", "Employees.Edit" }),
            ("Employee", new[] { "Employees.View" })
        };

        var roles = new Dictionary<string, Role>();
        var permissionsByName = await db.Permissions.ToDictionaryAsync(p => p.Name, p => p.Id);
        foreach (var (roleName, perms) in roleDefinitions)
        {
            var role = new Role { Name = roleName, TenantId = tenant.TenantId };
            db.Roles.Add(role);
            await db.SaveChangesAsync();

            foreach (var permName in perms)
            {
                if (permissionsByName.TryGetValue(permName, out var permId))
                {
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permId,
                        DataScope = "TenantWide",
                        ScopeIdsJson = "[]"
                    });
                }
            }
            roles[roleName] = role;
        }
        await db.SaveChangesAsync();

        // Employees + linked users
        var firstNames = new[] { "Ahmed", "Fatima", "Omar", "Layla", "Yusuf", "Noor", "Khalid", "Amina", "Hassan", "Mariam", "Ali", "Sara", "Bilal", "Huda", "Tariq", "Rania", "Salem", "Dana", "Waleed", "Nadia" };
        var lastNames = new[] { "Al-Sayed", "Hassan", "Al-Farsi", "Nasser", "Al-Mansoori", "Qureshi", "Al-Katheeri", "Rahman", "Al-Suwaidi", "Malik", "Al-Zaabi", "Iqbal", "Al-Hashimi", "Chaudhry", "Al-Marzooqi" };

        var random = new Random(def.TenantCode.GetHashCode());
        var employees = new List<Employee>();
        int employeeCount = 18;

        // Role assignment plan: index 0 = Company Admin, 1 = HR, 2 = Payroll, 3-4 = Managers, rest = Employees
        for (int i = 0; i < employeeCount; i++)
        {
            var first = firstNames[random.Next(firstNames.Length)];
            var last = lastNames[random.Next(lastNames.Length)];
            var dept = departments[i % departments.Count];
            var branch = branches[i % branches.Count];

            EmployeeStatus status = i switch
            {
                _ when i == employeeCount - 1 => EmployeeStatus.Terminated,
                _ when i == employeeCount - 2 => EmployeeStatus.OnLeave,
                _ when i == employeeCount - 3 => EmployeeStatus.Probation,
                _ when i == employeeCount - 4 => EmployeeStatus.NoticePeriod,
                _ => EmployeeStatus.Active
            };

            var employee = new Employee
            {
                TenantId = tenant.TenantId,
                CompanyId = company.CompanyId,
                DepartmentId = dept.DepartmentId,
                BranchId = branch.BranchId,
                EmployeeCode = $"{def.EmployeeCodePrefix}-{(i + 1):0000}",
                FirstName = first,
                LastName = last,
                DateOfBirth = now.AddYears(-random.Next(22, 55)).AddDays(-random.Next(0, 365)),
                Gender = i % 2 == 0 ? "Male" : "Female",
                Nationality = def.Country == "QA" ? "Qatari" : "Emirati",
                MotherName = $"{lastNames[random.Next(lastNames.Length)]} Family",
                HomeCountryAddress = $"Street {random.Next(1, 200)}, {def.Country}",
                HomeCountryPhone = $"+97{random.Next(0, 9)} {random.Next(1000000, 9999999)}",
                EmergencyContactName = $"{firstNames[random.Next(firstNames.Length)]} {last}",
                EmergencyPhone = $"+97{random.Next(0, 9)} {random.Next(1000000, 9999999)}",
                Email = $"{first.ToLowerInvariant()}.{last.ToLowerInvariant().Replace("-", "")}.{def.UserPrefix}{i}@{def.UserPrefix}.demo",
                PassportNumber = $"P{random.Next(10000000, 99999999)}",
                PassportExpiry = now.AddYears(random.Next(1, 5)),
                PassportCountry = def.Country,
                PhotoPath = "",
                Status = status,
                EmploymentDetail = new EmploymentDetail
                {
                    TenantId = tenant.TenantId,
                    JoiningDate = now.AddYears(-random.Next(1, 4)).AddDays(-random.Next(0, 365)),
                    Category = i % 4 == 0 ? "Labor" : "Staff",
                    OfferDesignation = dept.Name + " Officer",
                    MOLDesignation = dept.Name + " Officer",
                    BasicSalary = 3000 + random.Next(0, 8) * 500,
                    AccommodationAllowance = 800,
                    TravelAllowance = 300,
                    OtherAllowance = 200,
                    MOLBasicSalary = 3000,
                    MOLGrossSalary = 4300,
                    CurrentGrossSalary = 4300 + random.Next(0, 8) * 500,
                    OT_Eligible = i % 3 == 0,
                    SalaryMode = "Bank",
                    BankDetails = $"{def.TenantCode} Bank",
                    BankAccountNo = $"{random.Next(100000000, 999999999)}",
                    IBAN = $"{def.Country}{random.Next(10, 99)}DEMO{random.Next(100000000, 999999999)}",
                    WorkLocation = branch.Name,
                    ContractType = "Permanent",
                    OtherSalaryDetails = "",
                    Remarks = "",
                    VisaNo = $"V{random.Next(100000, 999999)}",
                    VisaIssueDate = now.AddYears(-2),
                    VisaExpiry = now.AddYears(1),
                    EmiratesId = $"E{random.Next(100000000, 999999999)}",
                    EmiratesIssueDate = now.AddYears(-2),
                    EmiratesExpiry = now.AddYears(2),
                    LaborCardNo = $"L{random.Next(100000, 999999)}",
                    LaborCardIssueDate = now.AddYears(-2),
                    LaborCardExpiry = now.AddYears(1),
                    IsActive = status == EmployeeStatus.Active
                }
            };
            db.Employees.Add(employee);
            employees.Add(employee);
        }
        await db.SaveChangesAsync();

        company.EmployeeCount = employees.Count;

        // Status history for each employee (draft -> active)
        foreach (var employee in employees)
        {
            db.EmployeeStatusHistories.Add(new EmployeeStatusHistory
            {
                TenantId = tenant.TenantId,
                EmployeeId = employee.EmployeeId,
                PreviousStatus = EmployeeStatus.Draft,
                NewStatus = employee.Status,
                EffectiveDate = employee.EmploymentDetail?.JoiningDate ?? now.AddYears(-1),
                Reason = "Initial onboarding",
                ChangedAt = employee.EmploymentDetail?.JoiningDate ?? now.AddYears(-1)
            });
        }

        // Attendance and shifts: last 20 working days
        var attendanceStatusChoices = new[] { "Present", "Present", "Present", "Late", "Absent" };
        for (int d = 20; d >= 1; d--)
        {
            var date = now.Date.AddDays(-d);
            if (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday) continue;

            foreach (var employee in employees.Where(e => e.Status == EmployeeStatus.Active || e.Status == EmployeeStatus.Probation))
            {
                var shift = employee.EmployeeId % 5 == 0 ? nightShift : morningShift;
                db.EmployeeShifts.Add(new EmployeeShift
                {
                    TenantId = tenant.TenantId,
                    EmployeeId = employee.EmployeeId,
                    ShiftId = shift.Id,
                    Date = date
                });

                var attendanceState = attendanceStatusChoices[random.Next(attendanceStatusChoices.Length)];
                if (attendanceState == "Absent")
                {
                    continue; // no attendance record logged for absentees
                }

                var checkIn = attendanceState == "Late" ? shift.StartTime.Add(TimeSpan.FromMinutes(random.Next(15, 60))) : shift.StartTime;
                var checkOut = shift.EndTime;
                var ot1 = employee.EmploymentDetail?.OT_Eligible == true ? random.Next(0, 90) : 0;

                db.Attendance.Add(new Attendance
                {
                    TenantId = tenant.TenantId,
                    EmployeeId = employee.EmployeeId,
                    Date = date,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    OT1 = ot1,
                    OT2 = 0
                });
            }
        }
        await db.SaveChangesAsync();

        // Leave requests with varied statuses
        var leaveStatuses = new[] { "Pending", "Approved", "Rejected" };
        var leaveTypes = new[] { "Annual", "Sick", "Emergency" };
        for (int i = 0; i < 10 && i < employees.Count; i++)
        {
            var employee = employees[i];
            var start = now.Date.AddDays(random.Next(-30, 20));
            db.LeaveRequests.Add(new LeaveRequest
            {
                TenantId = tenant.TenantId,
                EmployeeId = employee.EmployeeId,
                StartDate = start,
                EndDate = start.AddDays(random.Next(1, 5)),
                Reason = "Personal reasons",
                Status = leaveStatuses[i % leaveStatuses.Length],
                LeaveType = leaveTypes[i % leaveTypes.Length]
            });
        }
        await db.SaveChangesAsync();

        // Payroll for last 2 months, varied approval status
        foreach (var monthOffset in new[] { 2, 1 })
        {
            var month = new DateTime(now.AddMonths(-monthOffset).Year, now.AddMonths(-monthOffset).Month, 1);
            foreach (var employee in employees)
            {
                var basic = (double)(employee.EmploymentDetail?.BasicSalary ?? 3000);
                var otHours = random.Next(0, 10);
                var otEarnings = otHours * 25;
                var deductions = random.Next(0, 150);
                db.Payrolls.Add(new Payroll
                {
                    TenantId = tenant.TenantId,
                    EmployeeId = employee.EmployeeId,
                    Month = month,
                    BasicSalary = basic,
                    OT1Hours = otHours,
                    OT2Hours = 0,
                    OTEarnings = otEarnings,
                    Deductions = deductions,
                    NetSalary = basic + otEarnings - deductions,
                    IsApproved = monthOffset == 2 // older month approved/paid, latest month still draft
                });
            }
        }
        await db.SaveChangesAsync();

        // Asset assignments
        for (int i = 0; i < assets.Count; i++)
        {
            var employee = employees[i % employees.Count];
            db.EmployeeAssets.Add(new EmployeeAsset
            {
                TenantId = tenant.TenantId,
                EmployeeId = employee.EmployeeId,
                AssetId = assets[i].Id,
                AssignedDate = now.AddMonths(-random.Next(1, 12)),
                Status = i == assets.Count - 1 ? "Returned" : "Assigned",
                ReturnedDate = i == assets.Count - 1 ? now.AddDays(-10) : null
            });
        }

        // Employee documents (metadata only, no physical file required)
        var docTypes = new[] { "Passport Copy", "Visa Copy", "Emirates ID", "Employment Contract" };
        foreach (var employee in employees.Take(8))
        {
            var docType = docTypes[random.Next(docTypes.Length)];
            db.EmployeeDocuments.Add(new EmployeeDocument
            {
                TenantId = tenant.TenantId,
                EmployeeId = employee.EmployeeId,
                FileName = $"{docType.Replace(" ", "_")}_{employee.EmployeeCode}.pdf",
                FilePath = $"/uploads/demo/{def.TenantCode.ToLowerInvariant()}/{employee.EmployeeCode}.pdf",
                CloudinaryPublicId = "",
                UploadedAt = now.AddMonths(-random.Next(1, 10)),
                FileType = "application/pdf"
            });
        }

        // Notifications, mixed read/unread
        var notificationSamples = new[]
        {
            ("LEAVE_REQUEST_SUBMITTED", "New leave request submitted", "A leave request is awaiting your approval."),
            ("PAYROLL_PROCESSED", "Payroll processed", "This month's payroll has been processed."),
            ("DOCUMENT_EXPIRING", "Document expiring soon", "An employee document is expiring within 30 days."),
            ("ASSET_ASSIGNED", "Asset assigned", "A company asset has been assigned to an employee.")
        };
        for (int i = 0; i < 12; i++)
        {
            var (code, subject, body) = notificationSamples[i % notificationSamples.Length];
            db.Notifications.Add(new Notification
            {
                TenantId = tenant.TenantId,
                RecipientEmail = $"admin.{def.UserPrefix}@peopleos.demo",
                EventCode = code,
                Channel = "InApp",
                Subject = subject,
                Body = body,
                IsRead = i % 3 == 0,
                CreatedAt = now.AddDays(-i),
                ReadAt = i % 3 == 0 ? now.AddDays(-i).AddHours(2) : null
            });
        }
        await db.SaveChangesAsync();

        // Users linked to specific employees, one per role
        var userPlan = new (string Suffix, string RoleName, int? EmployeeIndex)[]
        {
            ("admin", "Company Administrator", 0),
            ("hr", "HR", 1),
            ("payroll", "Payroll", 2),
            ("manager", "Manager", 3),
            ("employee", "Employee", 5)
        };

        foreach (var (suffix, roleName, _) in userPlan)
        {
            var username = $"{suffix}.{def.UserPrefix}@peopleos.demo";
            var user = new AppUser
            {
                Username = username,
                TenantId = tenant.TenantId,
                PasswordHash = auth.HashPassword(DefaultPassword),
                IsActive = true
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roles[roleName].Id });
        }
        await db.SaveChangesAsync();
    }
}
