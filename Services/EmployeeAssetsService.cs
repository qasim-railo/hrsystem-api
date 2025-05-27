using AutoMapper;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services
{
    public class EmployeeAssetsService : IEmployeeAssetsService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public EmployeeAssetsService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<EmployeeAssetDto>> GetAllAsync()
        {
            var list = await _context.EmployeeAssets
                .Include(ea => ea.Employee)
                .Include(ea => ea.Asset)
                .ToListAsync();

            return _mapper.Map<List<EmployeeAssetDto>>(list);
        }

        public async Task<EmployeeAssetDto> GetByIdAsync(int id)
        {
            var item = await _context.EmployeeAssets.FindAsync(id);
            return _mapper.Map<EmployeeAssetDto>(item);
        }

        public async Task<EmployeeAssetDto> AssignAsync(EmployeeAssetDto dto)
        {
            var entity = _mapper.Map<EmployeeAsset>(dto);
            _context.EmployeeAssets.Add(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<EmployeeAssetDto>(entity);
        }

        public async Task<EmployeeAssetDto> UpdateAsync(int id, EmployeeAssetDto dto)
        {
            var entity = await _context.EmployeeAssets.FindAsync(id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<EmployeeAssetDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.EmployeeAssets.FindAsync(id);
            if (entity == null) return false;

            _context.EmployeeAssets.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
