using HRSystem.API.Data;
using HRSystem.API.Dtos;
using HRSystem.API.Models;
using HRSystem.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services
{
    public class DepartmentsService : IDepartmentsService
    {
        private readonly AppDbContext _context;

        public DepartmentsService(AppDbContext context)
        {
            _context = context;
        }
         
        public async Task<List<DepartmentDto>> GetAllAsync()
        {
            return await _context.Department
                .Include(d => d.Company)
                .Select(d => new DepartmentDto
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name,
                    Description = d.Description,
                    CompanyId = d.CompanyId,
                    CompanyName = d.Company.Name
                }).ToListAsync();
        }

        public async Task<DepartmentDto?> GetByIdAsync(int id)
        {
            var d = await _context.Department.Include(d => d.Company).FirstOrDefaultAsync(d => d.DepartmentId == id);
            if (d == null) return null;

            return new DepartmentDto
            {
                DepartmentId = d.DepartmentId,
                Name = d.Name,
                Description = d.Description,
                CompanyId = d.CompanyId,
                CompanyName = d.Company.Name
            };
        }

        public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto)
        {
            var dept = new Department
            {
                Name = dto.Name,
                Description = dto.Description,
                CompanyId = dto.CompanyId
            };
            _context.Department.Add(dept);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(dept.DepartmentId);
        }

        public async Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto)
        {
            var dept = await _context.Department.FindAsync(id);
            if (dept == null) return null;

            dept.Name = dto.Name;
            dept.Description = dto.Description;
            dept.CompanyId = dto.CompanyId;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(dept.DepartmentId);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var dept = await _context.Department.FindAsync(id);
            if (dept == null) return false;

            _context.Department.Remove(dept);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<DepartmentDto>> GetByCompanyAsync(int companyId)
        {
            var departments = await _context.Department
                .Where(d => d.CompanyId == companyId)
                .ToListAsync();

            return departments.Select(d => new DepartmentDto
            {
                DepartmentId = d.DepartmentId,
                Name = d.Name,
                Description = d.Description,
                CompanyId = d.CompanyId
            });
        }

    }
}
