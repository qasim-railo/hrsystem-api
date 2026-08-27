using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;
namespace HRSystem.API.Services
{
    public class CompaniesService : ICompaniesService
    {
        private readonly AppDbContext _context;

        public CompaniesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CompanyDto>> GetAllAsync()
        {
            return await _context.Companies
                .Select(c => new CompanyDto
                {
                    CompanyId = c.CompanyId,
                    Name = c.Name,
                    Address = c.Address
                    , IsActive = c.IsActive, EffectiveFrom = c.EffectiveFrom, EffectiveTo = c.EffectiveTo
                })
                .ToListAsync();
        }

        public async Task<CompanyDto> GetByIdAsync(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return null;

            return new CompanyDto
            {
                CompanyId = company.CompanyId,
                Name = company.Name,
                Address = company.Address
                , IsActive = company.IsActive, EffectiveFrom = company.EffectiveFrom, EffectiveTo = company.EffectiveTo
            };
        }

        public async Task<CompanyDto> CreateAsync(CompanyDto dto)
        {
            var company = new Company
            {
                Name = dto.Name,
                Address = dto.Address
                , IsActive = dto.IsActive, EffectiveFrom = dto.EffectiveFrom, EffectiveTo = dto.EffectiveTo
            };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            dto.CompanyId = company.CompanyId;
            return dto;
        }

        public async Task<CompanyDto> UpdateAsync(int id, CompanyDto dto)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return null;

            company.Name = dto.Name;
            company.Address = dto.Address;
            company.IsActive = dto.IsActive;
            company.EffectiveFrom = dto.EffectiveFrom;
            company.EffectiveTo = dto.EffectiveTo;
            company.ArchivedAt = dto.IsActive ? null : (company.ArchivedAt ?? DateTime.UtcNow);
            await _context.SaveChangesAsync();

            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return false;

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CanDeleteAsync(int id) =>
            !await _context.Department.AnyAsync(d => d.CompanyId == id) &&
            !await _context.Employees.AnyAsync(e => e.CompanyId == id);
    }
}
