using AutoMapper;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services
{
    public class FinalSettlementService:IFinalSettlementService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public FinalSettlementService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<FinalSettlementDto>> GetAllAsync()
        {
            var settlements = await _context.FinalSettlements.Include(f => f.Employee).ToListAsync();
            return _mapper.Map<List<FinalSettlementDto>>(settlements);
        }

        public async Task<FinalSettlementDto> GetByIdAsync(int id)
        {
            var settlement = await _context.FinalSettlements.Include(f => f.Employee).FirstOrDefaultAsync(f => f.Id == id);
            return _mapper.Map<FinalSettlementDto>(settlement);
        }

        public async Task GenerateSettlementAsync(int employeeId)
        {
            var attendanceDays = await _context.Attendance.Where(a => a.EmployeeId == employeeId).CountAsync();
            var gratuity = attendanceDays * 20; // placeholder logic

            var leaveEncashment = 500.0; // calculate from LeaveRequests
            var deductions = 100.0; // placeholder

            var entity = new FinalSettlement
            {
                EmployeeId = employeeId,
                SettlementDate = DateTime.Now,
                LeaveEncashment = leaveEncashment,
                GratuityAmount = gratuity,
                Deductions = deductions,
                NetPayable = (leaveEncashment + gratuity) - deductions,
                EmployeeSigned = false,
                HRSigned = false
            };

            _context.FinalSettlements.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task SignOffAsync(int settlementId, bool isEmployeeSign, bool isHRSign)
        {
            var settlement = await _context.FinalSettlements.FindAsync(settlementId);
            if (settlement == null) return;

            if (isEmployeeSign)
                settlement.EmployeeSigned = true;
            if (isHRSign)
                settlement.HRSigned = true;

            await _context.SaveChangesAsync();
        }
    }
}
