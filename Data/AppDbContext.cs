using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;
namespace HRSystem.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Company> Companies { get; set; }

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


        //ON MODEL CREATING
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                .OnDelete(DeleteBehavior.Cascade); // ✅ OK

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