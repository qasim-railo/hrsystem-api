using AutoMapper;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace HRSystem.API.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AttendanceService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task AddAsync(AttendanceDto dto)
        {
            Console.WriteLine("DTOS Val=>", dto);
            var (ot1, ot2) = await CalculateOvertimeAsync(dto.EmployeeId, dto.Date, dto.CheckOut);

            var attendance = _mapper.Map<Attendance>(dto);
            attendance.OT1 = ot1;
            attendance.OT2 = ot2;

            _context.Attendance.Add(attendance);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(int id, AttendanceDto dto)
        {
            var existing = await _context.Attendance.FindAsync(id);
            if (existing == null)
                throw new Exception("Attendance record not found");

            // Recalculate overtime
            var (ot1, ot2) = await CalculateOvertimeAsync(dto.EmployeeId, dto.Date, dto.CheckOut);

            // Update properties
            _mapper.Map(dto, existing);
            existing.OT1 = ot1;
            existing.OT2 = ot2;

            _context.Attendance.Update(existing);
            await _context.SaveChangesAsync();
        }


        public async Task<List<AttendanceDto>> GetByEmployeeAsync(int employeeId)
        {
            var records = await _context.Attendance
                .Where(a => a.EmployeeId == employeeId)
                .ToListAsync();
            return _mapper.Map<List<AttendanceDto>>(records);
        }
        public async Task ImportFromExcelAsync(IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            var rowCount = worksheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                var dto = new AttendanceDto
                {
                    EmployeeId = int.Parse(worksheet.Cells[row, 1].Text),
                    Date = DateTime.Parse(worksheet.Cells[row, 2].Text),
                    CheckIn = TimeSpan.Parse(worksheet.Cells[row, 3].Text),
                    CheckOut = TimeSpan.Parse(worksheet.Cells[row, 4].Text)
                    // OT1 and OT2 will be calculated dynamically
                };

                // 🔁 Recalculate OT based on CheckIn/CheckOut
                var (ot1, ot2) = await CalculateOvertimeAsync(dto.EmployeeId, dto.Date, dto.CheckOut);

                var entity = _mapper.Map<Attendance>(dto);
                entity.OT1 = ot1;
                entity.OT2 = ot2;

                _context.Attendance.Add(entity);
            }

            await _context.SaveChangesAsync();
        }
        public async Task RecalculateAllOvertimeAsync()
        {
            var records = await _context.Attendance.ToListAsync();

            foreach (var record in records)
            {
                var (ot1, ot2) = await CalculateOvertimeAsync(record.EmployeeId, record.Date, record.CheckOut);
                record.OT1 = ot1;
                record.OT2 = ot2;
            }

            await _context.SaveChangesAsync();
        }


        public async Task<(int ot1, int ot2)> CalculateOvertimeAsync(int employeeId, DateTime date, TimeSpan checkOut)
        {
            var shift = await _context.EmployeeShifts
                .Include(es => es.Shift)
                .Where(es => es.EmployeeId == employeeId && es.Date == date)
                .Select(es => es.Shift)
                .FirstOrDefaultAsync();

            if (shift == null)
                return (0, 0);

            var shiftEnd = shift.EndTime;

            if (checkOut <= shiftEnd)
                return (0, 0);

            var overtimeMinutes = (int)(checkOut - shiftEnd).TotalMinutes;

            var ot1 = Math.Min(overtimeMinutes, 120); // OT1: up to 2 hours
            var ot2 = Math.Max(overtimeMinutes - 120, 0); // OT2: beyond 2 hours

            return (ot1, ot2);
        }

    }
}
