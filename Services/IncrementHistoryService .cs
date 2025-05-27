using AutoMapper;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services
{
    public class IncrementHistoryService : IIncrementHistoryService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public IncrementHistoryService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task AddAsync(IncrementHistoryDto dto)
        {
            var entity = _mapper.Map<IncrementHistory>(dto);
            _context.IncrementHistories.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<IncrementHistoryDto>> GetByEmployeeAsync(int employeeId)
        {
            var entities = await _context.IncrementHistories
                .Where(i => i.EmployeeId == employeeId)
                .ToListAsync();
            return _mapper.Map<List<IncrementHistoryDto>>(entities);
        }

        public async Task<List<IncrementHistoryDto>> GetAllAsync()
        {
            var entities = await _context.IncrementHistories.ToListAsync();
            return _mapper.Map<List<IncrementHistoryDto>>(entities);
        }
    }

}
