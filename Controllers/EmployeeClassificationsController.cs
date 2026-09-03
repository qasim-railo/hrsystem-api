using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController, Authorize(Policy = "Users.Manage")]
[Route("api/employee-classifications")]
public class EmployeeClassificationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _tenant;
    public EmployeeClassificationsController(AppDbContext db, ICurrentTenant tenant) { _db = db; _tenant = tenant; }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<EmployeeCategoryDto>>> Categories() =>
        Ok(await CategoriesQuery().OrderBy(x => x.SortOrder).ThenBy(x => x.Name).Select(x => Map(x)).ToListAsync());

    [HttpPost("categories")]
    public async Task<ActionResult<EmployeeCategoryDto>> CreateCategory(EmployeeCategoryDto dto)
    {
        if (!Valid(dto.Name, dto.Code)) return BadRequest("Category name and code are required.");
        if (await CategoriesQuery().AnyAsync(x => x.Name == dto.Name.Trim() || x.Code == dto.Code.Trim()))
            return Conflict("A category with this name or code already exists.");
        var item = new EmployeeCategory { Name = dto.Name.Trim(), Code = dto.Code.Trim().ToUpperInvariant(), Description = dto.Description?.Trim(), IsActive = dto.IsActive, SortOrder = dto.SortOrder };
        _db.Add(item); await _db.SaveChangesAsync();
        return Ok(Map(item));
    }

    [HttpPut("categories/{id:int}")]
    public async Task<ActionResult<EmployeeCategoryDto>> UpdateCategory(int id, EmployeeCategoryDto dto)
    {
        if (!Valid(dto.Name, dto.Code)) return BadRequest("Category name and code are required.");
        var item = await CategoriesQuery().SingleOrDefaultAsync(x => x.EmployeeCategoryId == id);
        if (item == null) return NotFound();
        if (await CategoriesQuery().AnyAsync(x => x.EmployeeCategoryId != id && (x.Name == dto.Name.Trim() || x.Code == dto.Code.Trim())))
            return Conflict("A category with this name or code already exists.");
        item.Name = dto.Name.Trim(); item.Code = dto.Code.Trim().ToUpperInvariant(); item.Description = dto.Description?.Trim(); item.IsActive = dto.IsActive; item.SortOrder = dto.SortOrder;
        await _db.SaveChangesAsync(); return Ok(Map(item));
    }

    [HttpGet("designations")]
    public async Task<ActionResult<IEnumerable<DesignationDto>>> Designations() =>
        Ok(await PositionsQuery().OrderBy(x => x.Name).Select(x => Map(x)).ToListAsync());

    [HttpPost("designations")]
    public async Task<ActionResult<DesignationDto>> CreateDesignation(DesignationDto dto)
    {
        if (!Valid(dto.Name, dto.Code) || !await ReferencesAreValid(dto)) return BadRequest("A designation name, code, and valid optional references are required.");
        if (await PositionsQuery().AnyAsync(x => x.Name == dto.Name.Trim() || x.Code == dto.Code.Trim())) return Conflict("A designation with this name or code already exists.");
        var item = new Position { Name = dto.Name.Trim(), Code = dto.Code.Trim().ToUpperInvariant(), Description = dto.Description?.Trim(), DepartmentId = dto.DepartmentId, EmployeeCategoryId = dto.EmployeeCategoryId, IsActive = dto.IsActive };
        _db.Add(item); await _db.SaveChangesAsync(); return Ok(Map(item));
    }

    [HttpPut("designations/{id:int}")]
    public async Task<ActionResult<DesignationDto>> UpdateDesignation(int id, DesignationDto dto)
    {
        if (!Valid(dto.Name, dto.Code) || !await ReferencesAreValid(dto)) return BadRequest("A designation name, code, and valid optional references are required.");
        var item = await PositionsQuery().SingleOrDefaultAsync(x => x.PositionId == id);
        if (item == null) return NotFound();
        if (await PositionsQuery().AnyAsync(x => x.PositionId != id && (x.Name == dto.Name.Trim() || x.Code == dto.Code.Trim()))) return Conflict("A designation with this name or code already exists.");
        item.Name = dto.Name.Trim(); item.Code = dto.Code.Trim().ToUpperInvariant(); item.Description = dto.Description?.Trim(); item.DepartmentId = dto.DepartmentId; item.EmployeeCategoryId = dto.EmployeeCategoryId; item.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(); return Ok(Map(item));
    }

    private IQueryable<EmployeeCategory> CategoriesQuery() => _tenant.TenantId is int id ? _db.EmployeeCategories.Where(x => x.TenantId == id) : Enumerable.Empty<EmployeeCategory>().AsQueryable();
    private IQueryable<Position> PositionsQuery() => _tenant.TenantId is int id ? _db.Positions.Where(x => x.TenantId == id) : Enumerable.Empty<Position>().AsQueryable();
    private async Task<bool> ReferencesAreValid(DesignationDto dto) =>
        _tenant.TenantId is int tenantId &&
        (dto.DepartmentId == null || await _db.Department.AnyAsync(x => x.TenantId == tenantId && x.DepartmentId == dto.DepartmentId)) &&
        (dto.EmployeeCategoryId == null || await CategoriesQuery().AnyAsync(x => x.EmployeeCategoryId == dto.EmployeeCategoryId));
    private static bool Valid(string name, string code) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(code);
    private static EmployeeCategoryDto Map(EmployeeCategory x) => new() { EmployeeCategoryId = x.EmployeeCategoryId, Name = x.Name, Code = x.Code, Description = x.Description, IsActive = x.IsActive, SortOrder = x.SortOrder };
    private static DesignationDto Map(Position x) => new() { DesignationId = x.PositionId, Name = x.Name, Code = x.Code ?? string.Empty, Description = x.Description, DepartmentId = x.DepartmentId, EmployeeCategoryId = x.EmployeeCategoryId, IsActive = x.IsActive };
}
