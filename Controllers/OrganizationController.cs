using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/organization")]
public class OrganizationController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrganizationController(AppDbContext db) => _db = db;

    [HttpGet("{type}")]
    public async Task<ActionResult<IEnumerable<OrganizationUnitDto>>> List(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "branches" => Ok(await _db.Branches.Select(x => new OrganizationUnitDto { Id = x.BranchId, Name = x.Name, Code = x.Code, CompanyId = x.CompanyId, IsActive = x.IsActive, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, ArchivedAt = x.ArchivedAt, ChildCount = x.Departments.Count, EmployeeCount = _db.Employees.Count(e => e.BranchId == x.BranchId) }).ToListAsync()),
            "sections" => Ok(await _db.Sections.Select(x => new OrganizationUnitDto { Id = x.SectionId, Name = x.Name, Description = x.Description, DepartmentId = x.DepartmentId, IsActive = x.IsActive, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, ArchivedAt = x.ArchivedAt, ChildCount = x.Teams.Count, EmployeeCount = _db.Employees.Count(e => e.SectionId == x.SectionId) }).ToListAsync()),
            "teams" => Ok(await _db.Teams.Select(x => new OrganizationUnitDto { Id = x.TeamId, Name = x.Name, Description = x.Description, ParentId = x.SectionId, IsActive = x.IsActive, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, ArchivedAt = x.ArchivedAt, ChildCount = x.Positions.Count, EmployeeCount = _db.Employees.Count(e => e.TeamId == x.TeamId) }).ToListAsync()),
            "positions" => Ok(await _db.Positions.Select(x => new OrganizationUnitDto { Id = x.PositionId, Name = x.Name, Code = x.Code, Description = x.Description, ParentId = x.TeamId, IsActive = x.IsActive, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, ArchivedAt = x.ArchivedAt, EmployeeCount = _db.Employees.Count(e => e.PositionId == x.PositionId) }).ToListAsync()),
            _ => BadRequest("type must be branches, sections, teams, or positions")
        };
    }

    [HttpPost("{type}")]
    public async Task<ActionResult<OrganizationUnitDto>> Create(string type, CreateOrganizationUnitDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name is required.");
        OrganizationUnitDto? result = type.ToLowerInvariant() switch
        {
            "branches" => await CreateBranch(dto),
            "sections" => await CreateSection(dto),
            "teams" => await CreateTeam(dto),
            "positions" => await CreatePosition(dto),
            _ => null
        };
        return result == null ? BadRequest("Invalid type or parent.") : Ok(result);
    }

    [HttpPut("{type}/{id:int}")]
    public async Task<ActionResult<OrganizationUnitDto>> Update(string type, int id, UpdateOrganizationUnitDto dto)
    {
        var entity = await Find(type, id);
        if (entity == null) return NotFound();
        if (!await ParentIsValid(type, dto)) return BadRequest("Parent does not exist in this tenant.");
        type = type.ToLowerInvariant();
        entity.Name = dto.Name; entity.Code = dto.Code; entity.Description = dto.Description;
        entity.IsActive = dto.IsActive; entity.EffectiveFrom = dto.EffectiveFrom; entity.EffectiveTo = dto.EffectiveTo;
        entity.ArchivedAt = dto.IsActive ? null : (entity.ArchivedAt ?? DateTime.UtcNow);
        SetParent(type, entity, dto);
        await _db.SaveChangesAsync();
        return Ok(new OrganizationUnitDto
        {
            Id = id, Name = dto.Name, Code = dto.Code, Description = dto.Description,
            ParentId = dto.ParentId, CompanyId = dto.CompanyId, DepartmentId = dto.DepartmentId,
            IsActive = dto.IsActive, EffectiveFrom = dto.EffectiveFrom, EffectiveTo = dto.EffectiveTo,
            ArchivedAt = entity.ArchivedAt
        });
    }

    [HttpPost("{type}/{id:int}/archive")]
    public async Task<IActionResult> Archive(string type, int id)
    {
        var entity = await Find(type, id);
        if (entity == null) return NotFound();
        entity.IsActive = false; entity.ArchivedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{type}/{id:int}/delete-check")]
    public async Task<ActionResult<OrganizationDeleteCheckDto>> DeleteCheck(string type, int id)
    {
        var entity = await Find(type, id);
        if (entity == null) return NotFound();
        var (children, employees) = Counts(type, id);
        return new OrganizationDeleteCheckDto { CanDelete = children == 0 && employees == 0, ChildCount = children, EmployeeCount = employees, Reason = children > 0 ? "Reassign or delete child units first." : employees > 0 ? "Reassign employees before deleting this unit." : null };
    }

    [HttpDelete("{type}/{id:int}")]
    public async Task<IActionResult> Delete(string type, int id)
    {
        var entity = await Find(type, id);
        if (entity == null) return NotFound();
        var (children, employees) = Counts(type, id);
        if (children != 0 || employees != 0) return Conflict(new OrganizationDeleteCheckDto { CanDelete = false, ChildCount = children, EmployeeCount = employees, Reason = "This organizational unit is referenced and cannot be deleted. Reassign references first." });
        Remove(type, entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<OrganizationUnitDto?> CreateBranch(CreateOrganizationUnitDto d)
    {
        if (!d.CompanyId.HasValue || !await _db.Companies.AnyAsync(x => x.CompanyId == d.CompanyId)) return null;
        var x = new Branch { Name = d.Name, Code = d.Code, Address = d.Description, CompanyId = d.CompanyId.Value, IsActive = d.IsActive, EffectiveFrom = d.EffectiveFrom, EffectiveTo = d.EffectiveTo };
        _db.Branches.Add(x); await _db.SaveChangesAsync(); return Map(x.BranchId, x.Name, x.Code, null, x.CompanyId, null, x.IsActive, x.EffectiveFrom, x.EffectiveTo, null, x.Departments.Count, 0);
    }
    private async Task<OrganizationUnitDto?> CreateSection(CreateOrganizationUnitDto d)
    {
        if (!d.DepartmentId.HasValue || !await _db.Department.AnyAsync(x => x.DepartmentId == d.DepartmentId)) return null;
        var x = new Section { Name = d.Name, Description = d.Description, DepartmentId = d.DepartmentId.Value, IsActive = d.IsActive, EffectiveFrom = d.EffectiveFrom, EffectiveTo = d.EffectiveTo };
        _db.Sections.Add(x); await _db.SaveChangesAsync(); return Map(x.SectionId, x.Name, null, null, null, x.DepartmentId, x.IsActive, x.EffectiveFrom, x.EffectiveTo, null, 0, 0);
    }
    private async Task<OrganizationUnitDto?> CreateTeam(CreateOrganizationUnitDto d)
    {
        if (!d.ParentId.HasValue || !await _db.Sections.AnyAsync(x => x.SectionId == d.ParentId)) return null;
        var x = new Team { Name = d.Name, Description = d.Description, SectionId = d.ParentId.Value, IsActive = d.IsActive, EffectiveFrom = d.EffectiveFrom, EffectiveTo = d.EffectiveTo };
        _db.Teams.Add(x); await _db.SaveChangesAsync(); return Map(x.TeamId, x.Name, null, x.SectionId, null, null, x.IsActive, x.EffectiveFrom, x.EffectiveTo, null, 0, 0);
    }
    private async Task<OrganizationUnitDto?> CreatePosition(CreateOrganizationUnitDto d)
    {
        if (d.ParentId.HasValue && !await _db.Teams.AnyAsync(x => x.TeamId == d.ParentId)) return null;
        var x = new Position { Name = d.Name, Code = d.Code, Description = d.Description, TeamId = d.ParentId, IsActive = d.IsActive, EffectiveFrom = d.EffectiveFrom, EffectiveTo = d.EffectiveTo };
        _db.Positions.Add(x); await _db.SaveChangesAsync(); return Map(x.PositionId, x.Name, x.Code, x.TeamId, null, null, x.IsActive, x.EffectiveFrom, x.EffectiveTo, null, 0, 0);
    }
    private static OrganizationUnitDto Map(int id, string name, string? code, int? parent, int? company, int? department, bool active, DateTime? from, DateTime? to, DateTime? archived, int children, int employees) => new() { Id = id, Name = name, Code = code, ParentId = parent, CompanyId = company, DepartmentId = department, IsActive = active, EffectiveFrom = from, EffectiveTo = to, ArchivedAt = archived, ChildCount = children, EmployeeCount = employees };

    private async Task<dynamic?> Find(string type, int id) => type.ToLowerInvariant() switch { "branches" => await _db.Branches.FindAsync(id), "sections" => await _db.Sections.FindAsync(id), "teams" => await _db.Teams.FindAsync(id), "positions" => await _db.Positions.FindAsync(id), _ => null };
    private async Task<bool> ParentIsValid(string type, CreateOrganizationUnitDto d) => type.ToLowerInvariant() switch { "branches" => d.CompanyId.HasValue && await _db.Companies.AnyAsync(x => x.CompanyId == d.CompanyId), "sections" => d.DepartmentId.HasValue && await _db.Department.AnyAsync(x => x.DepartmentId == d.DepartmentId), "teams" => d.ParentId.HasValue && await _db.Sections.AnyAsync(x => x.SectionId == d.ParentId), "positions" => !d.ParentId.HasValue || await _db.Teams.AnyAsync(x => x.TeamId == d.ParentId), _ => false };
    private void SetParent(string type, dynamic e, CreateOrganizationUnitDto d) { if (type == "branches") e.CompanyId = d.CompanyId; if (type == "sections") e.DepartmentId = d.DepartmentId; if (type == "teams") e.SectionId = d.ParentId; if (type == "positions") e.TeamId = d.ParentId; }
    private (int children, int employees) Counts(string type, int id) => type.ToLowerInvariant() switch { "branches" => (_db.Department.Count(x => x.BranchId == id), _db.Employees.Count(x => x.BranchId == id)), "sections" => (_db.Teams.Count(x => x.SectionId == id), _db.Employees.Count(x => x.SectionId == id)), "teams" => (_db.Positions.Count(x => x.TeamId == id), _db.Employees.Count(x => x.TeamId == id)), "positions" => (0, _db.Employees.Count(x => x.PositionId == id)), _ => (0, 0) };
    private void Remove(string type, dynamic e) { if (type == "branches") _db.Branches.Remove(e); if (type == "sections") _db.Sections.Remove(e); if (type == "teams") _db.Teams.Remove(e); if (type == "positions") _db.Positions.Remove(e); }
}
