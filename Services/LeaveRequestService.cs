using AutoMapper;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public LeaveRequestService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LeaveRequestResponseDto>> GetAllAsync()
        {
            var leaves = await _context.LeaveRequests.Include(l => l.Employee).ToListAsync();
            return _mapper.Map<IEnumerable<LeaveRequestResponseDto>>(leaves);
        }

        public async Task<LeaveRequestResponseDto> GetByIdAsync(int id)
        {
            var leave = await _context.LeaveRequests.Include(l => l.Employee).FirstOrDefaultAsync(l => l.Id == id);
            return _mapper.Map<LeaveRequestResponseDto>(leave);
        }

        public async Task AddAsync(LeaveRequestDto dto)
        {
            var leave = _mapper.Map<LeaveRequest>(dto);
            leave.Status = "Pending";
            _context.LeaveRequests.Add(leave);
            await _context.SaveChangesAsync();
        }

        public async Task ApproveAsync(int id, int approvedBy)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave != null)
            {
                leave.Status = "Approved";
                leave.ApprovedBy = approvedBy;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RejectAsync(int id)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave != null)
            {
                leave.Status = "Rejected";
                await _context.SaveChangesAsync();
            }
        }
    }

}
