using AutoMapper;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services
{
    public class EmployeeShiftService : IEmployeeShiftService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public EmployeeShiftService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<EmployeeShiftDto>> GetAllAsync()
        {
            var empShifts = await _context.EmployeeShifts.ToListAsync();
             return _mapper.Map<List<EmployeeShiftDto>>(empShifts);
        }

        //public async Task<EmployeeShiftDto> GetByIdAsync(int id)
        //{
        //    var empShift = await _context.EmployeeShifts.FindAsync(id);
        //    Console.WriteLine("THIs is HEre=====>");

        //    return _mapper.Map<EmployeeShiftDto>(empShift);
        //}
        public async Task<List<object>> GetByIdAsync(int id)
        {
            var shifts = await _context.EmployeeShifts
                .Include(es => es.Shift)
                .Where(es => es.EmployeeId == id)
                .Select(es => new
                {
                    es.Id,
                    es.EmployeeId,
                    es.ShiftId,
                    es.Date,
                    ShiftName = es.Shift.Name,
                    StartTime = es.Shift.StartTime,
                    EndTime = es.Shift.EndTime
                })
                .ToListAsync();

            return shifts.Cast<object>().ToList();
        }

        //public async Task<List<EmployeeShiftDto>> GetByIdAsync(int id)
        //{
        //    var shifts = await _context.EmployeeShifts
        //         .Include(es => es.Shift) // Optional: to include shift details like name, start/end time
        //        .Where(es => es.EmployeeId == id)
        //        .ToListAsync();

        //    return _mapper.Map<List<EmployeeShiftDto>>(shifts);
        //}

        public async Task<EmployeeShiftDto> CreateAsync(EmployeeShiftDto dto)
        {
            var empShift = _mapper.Map<EmployeeShift>(dto);
            _context.EmployeeShifts.Add(empShift);
            await _context.SaveChangesAsync();
            return _mapper.Map<EmployeeShiftDto>(empShift);
        }

        public async Task UpdateAsync(int id, EmployeeShiftDto dto)
        {
            var empShift = await _context.EmployeeShifts.FindAsync(id);
            if (empShift == null) throw new Exception("Employee Shift not found");

            _mapper.Map(dto, empShift);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var empShift = await _context.EmployeeShifts.FindAsync(id);
            if (empShift == null) throw new Exception("Employee Shift not found");

            _context.EmployeeShifts.Remove(empShift);
            await _context.SaveChangesAsync();
        }
    }

}
