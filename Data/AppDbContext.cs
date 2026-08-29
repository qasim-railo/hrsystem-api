using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Models.Auth;
using Microsoft.EntityFrameworkCore;
using HRSystem.API.Tenancy;
namespace HRSystem.API.Data
{
    public class AppDbContext : DbContext
    {
        private readonly ICurrentTenant? _currentTenant;

        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenant? currentTenant = null)
            : base(options)
        {
            _currentTenant = currentTenant;
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<PlanFeature> PlanFeatures { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<BillingInvoice> BillingInvoices { get; set; }
        public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Position> Positions { get; set; }

        public DbSet<Department> Department { get; set; }
        public DbSet<Employee> Employees { get; set; }

        public DbSet<EmploymentDetail> EmploymentDetails { get; set; }
        public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<EmployeeAsset> EmployeeAssets { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<EmployeeShift> EmployeeShifts { get; set; }
        public DbSet<Attendance> Attendance { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<FinalSettlement> FinalSettlements { get; set; }

        public DbSet<GratuityReport> GratuityReports { get; set; }
        public DbSet<IncrementHistory> IncrementHistories { get; set; }

                public DbSet<EmployeeStatusHistory> EmployeeStatusHistories { get; set; }
                public DbSet<EmployeeEmploymentHistory> EmployeeEmploymentHistories { get; set; }
                        public DbSet<AuditLog> AuditLogs { get; set; }
                        public DbSet<PlatformAuditLog> PlatformAuditLogs { get; set; }
                        public DbSet<TenantSetting> TenantSettings { get; set; }
                        public DbSet<TenantLeaveType> TenantLeaveTypes { get; set; }
                        public DbSet<OnboardingProgress> OnboardingProgress { get; set; }
                        public DbSet<CustomFieldDefinition> CustomFieldDefinitions { get; set; }
                        public DbSet<CustomFieldValue> CustomFieldValues { get; set; }
                        public DbSet<FileRecord> FileRecords { get; set; }
                        public DbSet<NumberingSequence> NumberingSequences { get; set; }
                        public DbSet<ApprovalWorkflow> ApprovalWorkflows { get; set; }
                        public DbSet<ApprovalStep> ApprovalSteps { get; set; }
                        public DbSet<ApprovalRequest> ApprovalRequests { get; set; }
                        public DbSet<ApprovalAction> ApprovalActions { get; set; }
                        public DbSet<PayrollComponent> PayrollComponents { get; set; }
                        public DbSet<PayrollComponentSnapshot> PayrollComponentSnapshots { get; set; }
                        public DbSet<OvertimePolicy> OvertimePolicies { get; set; }
                        public DbSet<AttendanceConfiguration> AttendanceConfigurations { get; set; }
                        public DbSet<AttendanceImportLog> AttendanceImportLogs { get; set; }
                        public DbSet<ImportJob> ImportJobs { get; set; }
                        public DbSet<IntegrationConnection> IntegrationConnections { get; set; }
                        public DbSet<AppUser> Users { get; set; }
                        public DbSet<Role> Roles { get; set; }
                        public DbSet<Permission> Permissions { get; set; }
                        public DbSet<UserRole> UserRoles { get; set; }
                        public DbSet<RolePermission> RolePermissions { get; set; }
                        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
                        public DbSet<Notification> Notifications { get; set; }
                        public DbSet<SupportArticle> SupportArticles { get; set; }
                        public DbSet<SupportTicket> SupportTickets { get; set; }
                        public DbSet<SupportTicketMessage> SupportTicketMessages { get; set; }
                        public DbSet<SupportTicketAttachment> SupportTicketAttachments { get; set; }

                        public override int SaveChanges()
                        {
                            ApplyTenantBoundary();
                            return base.SaveChanges();
                        }

                        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
                        {
                            ApplyTenantBoundary();
                            return base.SaveChangesAsync(cancellationToken);
                        }

                        private void ApplyTenantBoundary()
                        {
                            if (_currentTenant == null || !_currentTenant.HasTenantContext)
                            {
                                return;
                            }

                            if (_currentTenant.IsPlatformAdmin)
                                return;

                            foreach (var entry in ChangeTracker.Entries<ITenantOwned>())
                            {
                                if (_currentTenant.TenantId is not int tenantId)
                                    throw new InvalidOperationException("A resolved tenant is required for tenant-owned data.");

                                if (entry.State == EntityState.Added)
                                    entry.Entity.TenantId = tenantId;
                                else if (entry.Entity.TenantId != tenantId)
                                    throw new InvalidOperationException("Tenant-owned data cannot cross tenant boundaries.");
                            }
                        }

                                //ON MODEL CREATING
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.Entity<Tenant>().HasIndex(t => t.Code).IsUnique();
                    modelBuilder.Entity<Plan>().HasIndex(p => p.Code).IsUnique();
                    modelBuilder.Entity<PlanFeature>().HasIndex(f => new { f.PlanId, f.FeatureCode }).IsUnique();
                    modelBuilder.Entity<Company>().HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
                    modelBuilder.Entity<Employee>().HasIndex(x => new { x.TenantId, x.EmployeeCode }).IsUnique();
                    modelBuilder.Entity<Department>().HasIndex(x => new { x.TenantId, x.CompanyId, x.Name }).IsUnique();
                    modelBuilder.Entity<Asset>().HasIndex(x => new { x.TenantId, x.AssetCode }).IsUnique();
                    modelBuilder.Entity<Section>().HasIndex(x => new { x.TenantId, x.DepartmentId, x.Name }).IsUnique();
                    modelBuilder.Entity<Team>().HasIndex(x => new { x.TenantId, x.SectionId, x.Name }).IsUnique();
                    modelBuilder.Entity<Position>().HasIndex(x => new { x.TenantId, x.TeamId, x.Name }).IsUnique();
                    modelBuilder.Entity<NotificationTemplate>().HasIndex(x => new { x.TenantId, x.EventCode, x.Channel }).IsUnique();
                    modelBuilder.Entity<Plan>().HasMany(p => p.Features).WithOne(f => f.Plan).HasForeignKey(f => f.PlanId);
                    modelBuilder.Entity<Tenant>().HasOne(t => t.Plan).WithMany(p => p.Tenants).HasForeignKey(t => t.PlanId).OnDelete(DeleteBehavior.Restrict);
                    modelBuilder.Entity<Employee>().HasQueryFilter(x => x.ArchivedAt == null);
                    modelBuilder.Entity<Department>().HasQueryFilter(x => x.ArchivedAt == null);
                    modelBuilder.Entity<Asset>().HasQueryFilter(x => x.ArchivedAt == null);
                    modelBuilder.Entity<AppUser>().HasQueryFilter(x => x.ArchivedAt == null && (_currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId));
                    modelBuilder.Entity<Role>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<UserRole>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.User.TenantId);
                    modelBuilder.Entity<RolePermission>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.Role.TenantId);
                    modelBuilder.Entity<Subscription>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<Subscription>().Property(s => s.Status).HasConversion<string>().HasMaxLength(32);
                    modelBuilder.Entity<Subscription>().HasOne(s => s.Tenant).WithMany(t => t.Subscriptions).HasForeignKey(s => s.TenantId).OnDelete(DeleteBehavior.Restrict);
                    modelBuilder.Entity<Subscription>().HasOne(s => s.Plan).WithMany(p => p.Subscriptions).HasForeignKey(s => s.PlanId).OnDelete(DeleteBehavior.Restrict);
                    modelBuilder.Entity<Subscription>().HasIndex(s => new { s.TenantId, s.Status });
                    modelBuilder.Entity<BillingInvoice>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<BillingInvoice>().Property(i => i.Status).HasConversion<string>().HasMaxLength(32);
                    modelBuilder.Entity<BillingInvoice>().HasOne(i => i.Tenant).WithMany(t => t.BillingInvoices).HasForeignKey(i => i.TenantId).OnDelete(DeleteBehavior.Restrict);
                    modelBuilder.Entity<BillingInvoice>().HasOne(i => i.Subscription).WithMany(s => s.Invoices).HasForeignKey(i => i.SubscriptionId).OnDelete(DeleteBehavior.Restrict);
                    modelBuilder.Entity<BillingInvoice>().HasIndex(i => new { i.TenantId, i.Status });
                    modelBuilder.Entity<SubscriptionPayment>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<SubscriptionPayment>().HasOne(p => p.Tenant).WithMany(t => t.SubscriptionPayments).HasForeignKey(p => p.TenantId).OnDelete(DeleteBehavior.Restrict);
                    modelBuilder.Entity<SubscriptionPayment>().HasOne(p => p.BillingInvoice).WithMany(i => i.Payments).HasForeignKey(p => p.BillingInvoiceId).OnDelete(DeleteBehavior.Cascade);
                    modelBuilder.Entity<SupportTicket>().Property(x => x.Priority).HasConversion<string>().HasMaxLength(32);
                    modelBuilder.Entity<SupportTicket>().Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
                    modelBuilder.Entity<SupportTicket>().HasMany(x => x.Messages).WithOne(x => x.Ticket).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
                    modelBuilder.Entity<SupportTicket>().HasMany(x => x.Attachments).WithOne(x => x.Ticket).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Restrict);
                    modelBuilder.Entity<SupportTicketMessage>().HasMany(x => x.Attachments).WithOne(x => x.Message).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.SetNull);
                    modelBuilder.Entity<SupportArticle>().HasIndex(x => new { x.TenantId, x.Category, x.Title });
                    modelBuilder.Entity<SupportTicket>().HasIndex(x => new { x.TenantId, x.Status, x.CreatedAt });
                    modelBuilder.Entity<SupportTicketMessage>().HasIndex(x => new { x.TenantId, x.TicketId, x.CreatedAt });
                    modelBuilder.Entity<SupportTicketAttachment>().HasIndex(x => new { x.TenantId, x.TicketId, x.FileId }).IsUnique();
                    modelBuilder.Entity<Plan>().HasData(
                        new Plan { PlanId = 1, Code = "ESSENTIAL", Name = "PeopleOS Essential", MaxEmployees = 50, MaxUsers = 10, MaxBranches = 1, MaxStorageBytes = 5L * 1024 * 1024 * 1024 },
                        new Plan { PlanId = 2, Code = "PROFESSIONAL", Name = "PeopleOS Professional", MaxEmployees = 250, MaxUsers = 50, MaxBranches = 10, MaxStorageBytes = 25L * 1024 * 1024 * 1024 });
                    var essentialFeatures = new[] { "EMPLOYEE_MANAGEMENT", "DOCUMENTS", "LEAVE", "ATTENDANCE", "SHIFTS", "BASIC_PAYROLL", "PAYSLIPS", "STANDARD_REPORTS", "EMPLOYEE_SELF_SERVICE" };
                    var professionalFeatures = new[] { "EMPLOYEE_MANAGEMENT", "DOCUMENTS", "LEAVE", "ATTENDANCE", "SHIFTS", "BASIC_PAYROLL", "PAYSLIPS", "STANDARD_REPORTS", "EMPLOYEE_SELF_SERVICE", "LOANS", "OVERTIME", "ASSETS", "GRATUITY", "FINAL_SETTLEMENT", "ADVANCED_REPORTS", "CUSTOM_ROLES", "WORKFLOWS", "ORGANIZATION_CHART", "EXPIRY_ALERTS", "ADVANCED_AUDIT" };
                    modelBuilder.Entity<PlanFeature>().HasData(
                        essentialFeatures.Select((code, index) => new PlanFeature { PlanFeatureId = index + 1, PlanId = 1, FeatureCode = code })
                            .Concat(professionalFeatures.Select((code, index) => new PlanFeature { PlanFeatureId = index + 100, PlanId = 2, FeatureCode = code }))
                            .ToArray());
                    modelBuilder.Entity<AppUser>().HasOne<Tenant>().WithMany().HasForeignKey(u => u.TenantId);
                    foreach (var entityType in new[]
                    {
                        typeof(Company), typeof(Department), typeof(Employee), typeof(EmploymentDetail),
                        typeof(EmployeeDocument), typeof(Asset), typeof(EmployeeAsset), typeof(Shift),
                        typeof(EmployeeShift), typeof(Attendance), typeof(Payroll), typeof(LeaveRequest),
                        typeof(FinalSettlement), typeof(GratuityReport), typeof(IncrementHistory),
                        typeof(EmployeeStatusHistory), typeof(AuditLog), typeof(Branch), typeof(Section), typeof(Team), typeof(Position)
                        , typeof(EmployeeEmploymentHistory), typeof(TenantSetting), typeof(TenantLeaveType), typeof(OnboardingProgress),
                        typeof(CustomFieldDefinition), typeof(CustomFieldValue), typeof(FileRecord)
                        ,                                                 typeof(NumberingSequence), typeof(ApprovalWorkflow), typeof(ApprovalRequest), typeof(ApprovalAction), typeof(PayrollComponent), typeof(PayrollComponentSnapshot), typeof(OvertimePolicy), typeof(AttendanceConfiguration), typeof(AttendanceImportLog), typeof(NotificationTemplate), typeof(Notification), typeof(IntegrationConnection), typeof(SupportArticle), typeof(SupportTicket), typeof(SupportTicketMessage), typeof(SupportTicketAttachment)
                    })
                    {
                        modelBuilder.Entity(entityType).Property<int>(nameof(ITenantOwned.TenantId));
                    }
                    modelBuilder.Entity<Company>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<Department>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<Branch>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<Section>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<Team>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<Position>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<Employee>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<EmploymentDetail>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<EmployeeDocument>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<Asset>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<EmployeeAsset>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<Shift>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<EmployeeShift>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<Attendance>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<Payroll>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<LeaveRequest>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<FinalSettlement>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<GratuityReport>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<IncrementHistory>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<EmployeeStatusHistory>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<EmployeeEmploymentHistory>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<AuditLog>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<TenantSetting>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<TenantLeaveType>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<OnboardingProgress>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<CustomFieldDefinition>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<CustomFieldValue>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<CustomFieldDefinition>().HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
                    modelBuilder.Entity<CustomFieldValue>().HasIndex(x => new { x.TenantId, x.EmployeeId, x.CustomFieldDefinitionId }).IsUnique();
                    modelBuilder.Entity<FileRecord>().HasKey(x => x.FileId);
                    modelBuilder.Entity<NumberingSequence>().HasIndex(x => new { x.TenantId, x.SequenceKey, x.Year }).IsUnique();
                    modelBuilder.Entity<FileRecord>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<NumberingSequence>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<ApprovalWorkflow>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<ApprovalRequest>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<ApprovalAction>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<PayrollComponent>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<PayrollComponentSnapshot>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<PayrollComponent>().HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
                    modelBuilder.Entity<PayrollComponentSnapshot>().HasOne(x => x.Payroll).WithMany(x => x.ComponentSnapshots).HasForeignKey(x => x.PayrollId).OnDelete(DeleteBehavior.Restrict);
                    modelBuilder.Entity<OvertimePolicy>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<OvertimePolicy>().HasIndex(x => new { x.TenantId, x.Name, x.EffectiveFrom });
                    modelBuilder.Entity<TenantLeaveType>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<TenantLeaveType>().HasIndex(x => new { x.TenantId, x.Name, x.EffectiveFrom });
                    modelBuilder.Entity<AttendanceConfiguration>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<AttendanceConfiguration>().HasIndex(x => x.TenantId).IsUnique();
                    modelBuilder.Entity<AttendanceImportLog>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<ImportJob>().HasQueryFilter(x => _currentTenant == null || _currentTenant.TenantId == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<IntegrationConnection>().HasQueryFilter(x => _currentTenant == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<IntegrationConnection>().HasIndex(x => new { x.TenantId, x.ProviderKey }).IsUnique();
                    modelBuilder.Entity<NotificationTemplate>().HasQueryFilter(x => _currentTenant == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<Notification>().HasQueryFilter(x => _currentTenant == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<PlatformAuditLog>().HasQueryFilter(x => _currentTenant == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<SupportArticle>().HasQueryFilter(x => _currentTenant == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<SupportTicket>().HasQueryFilter(x => _currentTenant == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<SupportTicketMessage>().HasQueryFilter(x => _currentTenant == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<SupportTicketAttachment>().HasQueryFilter(x => _currentTenant == null || _currentTenant.IsPlatformAdmin || _currentTenant.TenantId == x.TenantId);
                    modelBuilder.Entity<ApprovalWorkflow>().HasIndex(x => new { x.TenantId, x.Module, x.RequestType, x.Name }).IsUnique();
                    modelBuilder.Entity<ApprovalWorkflow>().HasMany(x => x.Steps).WithOne(x => x.Workflow).HasForeignKey(x => x.ApprovalWorkflowId).OnDelete(DeleteBehavior.Cascade);
                    modelBuilder.Entity<ApprovalRequest>().HasOne(x => x.Workflow).WithMany().HasForeignKey(x => x.ApprovalWorkflowId).OnDelete(DeleteBehavior.Restrict);
                    modelBuilder.Entity<ApprovalAction>().HasOne(x => x.Request).WithMany(x => x.Actions).HasForeignKey(x => x.ApprovalRequestId).OnDelete(DeleteBehavior.Restrict);
                    modelBuilder.Entity<FileRecord>().HasIndex(x => x.TenantId);
                    modelBuilder.Entity<FileRecord>().HasIndex(x => x.EntityType);
                    modelBuilder.Entity<FileRecord>().HasIndex(x => x.EntityId);
                    modelBuilder.Entity<FileRecord>().HasIndex(x => x.DocumentType);
                    modelBuilder.Entity<FileRecord>().HasIndex(x => x.UploadedAt);
                    modelBuilder.Entity<FileRecord>().HasIndex(x => x.Status);
                    modelBuilder.Entity<FileRecord>()
                        .HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId, x.DocumentType, x.IsCurrent })
                        .HasFilter("[IsCurrent] = 1")
                        .IsUnique();
                    modelBuilder.Entity<CustomFieldDefinition>().Property(x => x.FieldType).HasConversion<string>().HasMaxLength(32);
                    modelBuilder.Entity<CustomFieldValue>().HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
                    modelBuilder.Entity<CustomFieldValue>().HasOne(x => x.Definition).WithMany(x => x.Values).HasForeignKey(x => x.CustomFieldDefinitionId).OnDelete(DeleteBehavior.Cascade);
                    modelBuilder.Entity<TenantSetting>().HasIndex(x => new { x.TenantId, x.Key }).IsUnique();
                    modelBuilder.Entity<TenantLeaveType>().HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
                    modelBuilder.Entity<AppUser>().HasIndex(u => u.Username).IsUnique();
                    modelBuilder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
                    modelBuilder.Entity<UserRole>().HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId);
                    modelBuilder.Entity<UserRole>().HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.RoleId);
                    modelBuilder.Entity<Role>().HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
                    modelBuilder.Entity<Permission>().HasIndex(x => x.Name).IsUnique();
                    modelBuilder.Entity<RolePermission>().HasKey(x => new { x.RoleId, x.PermissionId });
                    modelBuilder.Entity<RolePermission>().HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId);
                    modelBuilder.Entity<RolePermission>().HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId);
                    modelBuilder.Entity<EmployeeStatusHistory>()
                        .HasKey(h => h.EmployeeStatusHistoryId);

                    modelBuilder.Entity<EmployeeStatusHistory>()
                        .HasOne(h => h.Employee)
                        .WithMany(e => e.StatusHistories)
                        .HasForeignKey(h => h.EmployeeId)
                        .OnDelete(DeleteBehavior.Cascade);
                     modelBuilder.Entity<EmployeeEmploymentHistory>()
                        .HasOne(h => h.Employee)
                        .WithMany()
                        .HasForeignKey(h => h.EmployeeId)
                        .OnDelete(DeleteBehavior.Cascade);
                     modelBuilder.Entity<EmployeeEmploymentHistory>()
                        .HasIndex(h => new { h.TenantId, h.EmployeeId, h.EffectiveFrom });

            modelBuilder.Entity<EmployeeShift>()
       .HasOne(es => es.Employee)
       .WithMany(e => e.EmployeeShifts)
       .HasForeignKey(es => es.EmployeeId);

            modelBuilder.Entity<EmployeeShift>()
                .HasOne(es => es.Shift)
                .WithMany(s => s.EmployeeShifts)
                .HasForeignKey(es => es.ShiftId);


            modelBuilder.Entity<Asset>().HasKey(a => a.Id);
            //Employee Assests
            modelBuilder.Entity<EmployeeAsset>()
    .HasKey(ea => ea.Id);

            modelBuilder.Entity<EmployeeAsset>()
                .HasOne(ea => ea.Employee)
                .WithMany(e => e.EmployeeAssets)
                .HasForeignKey(ea => ea.EmployeeId);

            modelBuilder.Entity<EmployeeAsset>()
                .HasOne(ea => ea.Asset)
                .WithMany(a => a.EmployeeAssets)
                .HasForeignKey(ea => ea.AssetId);


            // EmploymentDetail → Employee (one-to-one)
            modelBuilder.Entity<EmploymentDetail>()
                .HasOne(ed => ed.Employee)
                .WithOne(e => e.EmploymentDetail)
                .HasForeignKey<EmploymentDetail>(ed => ed.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade loop

            // Department → Company (many-to-one)
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Company)
                .WithMany(c => c.Departments)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Branch>().HasOne(b => b.Company).WithMany(c => c.Branches).HasForeignKey(b => b.CompanyId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Branch>().HasIndex(b => new { b.TenantId, b.CompanyId, b.Name }).IsUnique();
            modelBuilder.Entity<Section>().HasOne(s => s.Department).WithMany(d => d.Sections).HasForeignKey(s => s.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Team>().HasOne(t => t.Section).WithMany(s => s.Teams).HasForeignKey(t => t.SectionId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Position>().HasOne(p => p.Team).WithMany(t => t.Positions).HasForeignKey(p => p.TeamId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Employee>().HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Employee>().HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Employee>().HasOne(e => e.Team).WithMany().HasForeignKey(e => e.TeamId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Employee>().HasOne(e => e.Position).WithMany().HasForeignKey(e => e.PositionId).OnDelete(DeleteBehavior.Restrict);

            // Employee → Company (many-to-one)
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Company)
                .WithMany(c => c.Employees)
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Prevents cascade path

            // Employee → Department (many-to-one)
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict); // ✅ Prevents cascade path

            modelBuilder.Entity<EmployeeDocument>()
       .HasOne(ed => ed.Employee)
       .WithMany(e => e.EmployeeDocuments)
       .HasForeignKey(ed => ed.EmployeeId)
       .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LeaveRequest>()
    .HasKey(lr => lr.Id);
            modelBuilder.Entity<FinalSettlement>()
    .HasOne(f => f.Employee)
    .WithMany()
    .HasForeignKey(f => f.EmployeeId);









        }
    }

    
}
