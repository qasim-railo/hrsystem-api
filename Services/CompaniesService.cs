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
            };
        }

        public async Task<CompanyDto> CreateAsync(CompanyDto dto)
        {
            var company = new Company
            {
                Name = dto.Name,
                Address = dto.Address
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
    }
}
