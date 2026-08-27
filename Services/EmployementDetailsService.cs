using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services
{
    public class EmploymentDetailsService : IEmploymentDetailsService
    {
        private readonly AppDbContext _context;

        public EmploymentDetailsService(AppDbContext context)
        {
            _context = context;
        }

        // Private helper: Map model to DTO
        private static EmploymentDetailDto MapToDto(EmploymentDetail ed) => new EmploymentDetailDto
        {
            EmploymentDetailId = ed.EmploymentDetailId,
            EmployeeId = ed.EmployeeId,
            JoiningDate = ed.JoiningDate,
            Category = ed.Category,
            OfferDesignation = ed.OfferDesignation,
            MOLDesignation = ed.MOLDesignation,
            BasicSalary = ed.BasicSalary,
            AccommodationAllowance = ed.AccommodationAllowance,
            TravelAllowance = ed.TravelAllowance,
            OtherAllowance = ed.OtherAllowance,
            MOLBasicSalary = ed.MOLBasicSalary,
            MOLGrossSalary = ed.MOLGrossSalary,
            CurrentGrossSalary = ed.CurrentGrossSalary,
            OT_Eligible = ed.OT_Eligible,
            SalaryMode = ed.SalaryMode,
            BankDetails = ed.BankDetails,
            BankAccountNo = ed.BankAccountNo,
            IBAN = ed.IBAN,
            WorkLocation = ed.WorkLocation,
            ContractType = ed.ContractType,
            OtherSalaryDetails = ed.OtherSalaryDetails,
            VisaNo = ed.VisaNo,
            VisaIssueDate = ed.VisaIssueDate,
            VisaExpiry = ed.VisaExpiry,
            EmiratesId = ed.EmiratesId,
            EmiratesIssueDate = ed.EmiratesIssueDate,
            EmiratesExpiry = ed.EmiratesExpiry,
            LaborCardNo = ed.LaborCardNo,
            LaborCardIssueDate = ed.LaborCardIssueDate,
            LaborCardExpiry = ed.LaborCardExpiry,
            Remarks = ed.Remarks,
            IsActive = ed.IsActive
        };

        // Private helper: Update model from DTO
        private static void UpdateModelFromDto(EmploymentDetail ed, UpdateEmploymentDetailDto dto)
        {
            ed.EmployeeId = dto.EmployeeId;
            ed.JoiningDate = dto.JoiningDate;
            ed.Category = dto.Category;
            ed.OfferDesignation = dto.OfferDesignation;
            ed.MOLDesignation = dto.MOLDesignation;
            ed.BasicSalary = dto.BasicSalary;
            ed.AccommodationAllowance = dto.AccommodationAllowance;
            ed.TravelAllowance = dto.TravelAllowance;
            ed.OtherAllowance = dto.OtherAllowance;
            ed.MOLBasicSalary = dto.MOLBasicSalary;
            ed.MOLGrossSalary = dto.MOLGrossSalary;
            ed.CurrentGrossSalary = dto.CurrentGrossSalary;
            ed.OT_Eligible = dto.OT_Eligible;
            ed.SalaryMode = dto.SalaryMode;
            ed.BankDetails = dto.BankDetails;
            ed.BankAccountNo = dto.BankAccountNo;
            ed.IBAN = dto.IBAN;
            ed.WorkLocation = dto.WorkLocation;
            ed.ContractType = dto.ContractType;
            ed.OtherSalaryDetails = dto.OtherSalaryDetails;
            ed.VisaNo = dto.VisaNo;
            ed.VisaIssueDate = dto.VisaIssueDate;
            ed.VisaExpiry = dto.VisaExpiry;
            ed.EmiratesId = dto.EmiratesId;
            ed.EmiratesIssueDate = dto.EmiratesIssueDate;
            ed.EmiratesExpiry = dto.EmiratesExpiry;
            ed.LaborCardNo = dto.LaborCardNo;
            ed.LaborCardIssueDate = dto.LaborCardIssueDate;
            ed.LaborCardExpiry = dto.LaborCardExpiry;
            ed.Remarks = dto.Remarks;
            ed.IsActive = dto.IsActive;
        }

        public async Task<List<EmploymentDetailDto>> GetAllAsync()
        {
            return await _context.EmploymentDetails
                .Select(ed => MapToDto(ed))
                .ToListAsync();
        }

        public async Task<EmploymentDetailDto?> GetByIdAsync(int id)
        {
            var ed = await _context.EmploymentDetails.FindAsync(id);
            if (ed == null) return null;

            return MapToDto(ed);
        }

        public async Task<EmploymentDetailDto?> GetByEmployeeIdAsync(int employeeId)
        {
            var ed = await _context.EmploymentDetails
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (ed == null) return null;

            return MapToDto(ed);
        }

        public async Task<EmploymentDetailDto> CreateAsync(CreateEmploymentDetailDto dto)
        {
            var employeeExists = await _context.Employees.AnyAsync(e => e.EmployeeId == dto.EmployeeId);
            if (!employeeExists) throw new InvalidOperationException("Invalid EmployeeId.");

            var ed = new EmploymentDetail
            {
                EmployeeId = dto.EmployeeId,
                JoiningDate = dto.JoiningDate,
                Category = dto.Category,
                OfferDesignation = dto.OfferDesignation,
                MOLDesignation = dto.MOLDesignation,
                BasicSalary = dto.BasicSalary,
                AccommodationAllowance = dto.AccommodationAllowance,
                TravelAllowance = dto.TravelAllowance,
                OtherAllowance = dto.OtherAllowance,
                MOLBasicSalary = dto.MOLBasicSalary,
                MOLGrossSalary = dto.MOLGrossSalary,
                CurrentGrossSalary = dto.CurrentGrossSalary,
                OT_Eligible = dto.OT_Eligible,
                SalaryMode = dto.SalaryMode,
                BankDetails = dto.BankDetails,
                BankAccountNo = dto.BankAccountNo,
                IBAN = dto.IBAN,
                WorkLocation = dto.WorkLocation,
                ContractType = dto.ContractType,
                OtherSalaryDetails = dto.OtherSalaryDetails,
                VisaNo = dto.VisaNo,
                VisaIssueDate = dto.VisaIssueDate,
                VisaExpiry = dto.VisaExpiry,
                EmiratesId = dto.EmiratesId,
                EmiratesIssueDate = dto.EmiratesIssueDate,
                EmiratesExpiry = dto.EmiratesExpiry,
                LaborCardNo = dto.LaborCardNo,
                LaborCardIssueDate = dto.LaborCardIssueDate,
                LaborCardExpiry = dto.LaborCardExpiry,
                Remarks = dto.Remarks,
                IsActive = dto.IsActive
            };

            _context.EmploymentDetails.Add(ed);
            await _context.SaveChangesAsync();

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == ed.EmployeeId);
            if (employee != null)
            {
                _context.EmployeeEmploymentHistories.Add(CreateHistory(employee, ed, ed.JoiningDate, "Initial employment record"));
                await _context.SaveChangesAsync();
            }

            return MapToDto(ed);
        }

        public async Task<EmploymentDetailDto?> UpdateAsync(int id, UpdateEmploymentDetailDto dto)
        {
            var ed = await _context.EmploymentDetails.FindAsync(id);
            if (ed == null) return null;
            if (dto.EmployeeId != ed.EmployeeId)
                throw new InvalidOperationException("Employment details cannot be reassigned to another employee.");

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == ed.EmployeeId);
            if (employee != null)
            {
                var history = CreateHistory(employee, ed, ed.JoiningDate, "Employment record superseded");
                history.EffectiveTo = DateTime.UtcNow;
                _context.EmployeeEmploymentHistories.Add(history);
            }
            UpdateModelFromDto(ed, dto);
            await _context.SaveChangesAsync();

            return MapToDto(ed);
        }

        private static EmployeeEmploymentHistory CreateHistory(Employee employee, EmploymentDetail detail, DateTime effectiveFrom, string reason)
        {
            return new EmployeeEmploymentHistory
            {
                EmployeeId = employee.EmployeeId,
                CompanyId = employee.CompanyId,
                DepartmentId = employee.DepartmentId,
                BranchId = employee.BranchId,
                SectionId = employee.SectionId,
                TeamId = employee.TeamId,
                PositionId = employee.PositionId,
                EffectiveFrom = effectiveFrom,
                Category = detail.Category ?? string.Empty,
                Designation = detail.OfferDesignation ?? string.Empty,
                ContractType = detail.ContractType ?? string.Empty,
                BasicSalary = detail.BasicSalary,
                GrossSalary = detail.CurrentGrossSalary,
                ChangeReason = reason,
                RecordedAt = DateTime.UtcNow
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ed = await _context.EmploymentDetails.FindAsync(id);
            if (ed == null) return false;

            _context.EmploymentDetails.Remove(ed);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
