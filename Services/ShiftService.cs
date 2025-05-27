using AutoMapper;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services
{
    public class ShiftService : IShiftService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ShiftService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ShiftDto>> GetAllAsync()
        {
            var shifts = await _context.Shifts.ToListAsync();
            return _mapper.Map<List<ShiftDto>>(shifts);
        }

        public async Task<ShiftDto> GetByIdAsync(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            return _mapper.Map<ShiftDto>(shift);
        }

        public async Task<ShiftDto> CreateAsync(ShiftDto dto)
        {
            var shift = _mapper.Map<Shift>(dto);
            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();
            return _mapper.Map<ShiftDto>(shift);
        }

        public async Task UpdateAsync(int id, ShiftDto dto)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null) throw new Exception("Shift not found");

            _mapper.Map(dto, shift);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null) throw new Exception("Shift not found");

            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();
        }
    }
}
