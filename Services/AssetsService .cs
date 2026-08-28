using AutoMapper;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services
{
    public class AssetsService : IAssetsService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IReferenceNumberService _numbering;

        public AssetsService(AppDbContext context, IMapper mapper, IReferenceNumberService numbering)
        {
            _context = context;
            _mapper = mapper;
            _numbering = numbering;
        }

        public async Task<List<AssetDto>> GetAllAsync()
        {
            var assets = await _context.Assets.ToListAsync();
            return _mapper.Map<List<AssetDto>>(assets);
        }

        public async Task<AssetDto> GetByIdAsync(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            return _mapper.Map<AssetDto>(asset);
        }

        public async Task<AssetDto> CreateAsync(AssetDto dto)
        {
            var asset = _mapper.Map<Asset>(dto);
            if (string.IsNullOrWhiteSpace(asset.AssetCode))
                asset.AssetCode = await _numbering.NextAsync("asset");
            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();
            return _mapper.Map<AssetDto>(asset);
        }

        public async Task<AssetDto> UpdateAsync(int id, AssetDto dto)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return null;

            _mapper.Map(dto, asset);
            await _context.SaveChangesAsync();
            return _mapper.Map<AssetDto>(asset);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return false;

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
