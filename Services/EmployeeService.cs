using AutoMapper;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly CustomFieldService _customFields;
        private readonly IReferenceNumberService _numbering;
        public EmployeeService(AppDbContext context, IMapper mapper, CustomFieldService customFields, IReferenceNumberService numbering)
        {
            _context = context;
            _mapper = mapper;
            _customFields = customFields;
            _numbering = numbering;
        }

        public async Task<List<EmployeeDto>> GetAllAsync()
        {
            var e = await _context.Employees.ToListAsync();
            return _mapper.Map<List<EmployeeDto>>(e);
        }

        public async Task<(List<EmployeeDto> items, int totalCount)> GetFilteredAsync(EmployeeFilterDto filter)
        {
            var query = _context.Employees
                .Include(e => e.Company)
                .Include(e => e.Department)
                .Include(e => e.EmploymentDetail)
                .AsQueryable();

            if (filter.CompanyIds != null && filter.CompanyIds.Any())
                query = query.Where(e => filter.CompanyIds.Contains(e.CompanyId));

            if (filter.DepartmentIds != null && filter.DepartmentIds.Any())
                query = query.Where(e => filter.DepartmentIds.Contains(e.DepartmentId));

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim().ToLower();
                query = query.Where(e => (e.FirstName + " " + e.LastName).ToLower().Contains(s)
                                          || e.Email.ToLower().Contains(s)
                                          || e.PassportNumber.ToLower().Contains(s));
            }

            if (filter.Statuses != null && filter.Statuses.Any())
            {
                query = query.Where(e => filter.Statuses.Contains((int)e.Status));
            }

            if (filter.Category != null)
            {
                query = query.Where(e => e.EmploymentDetail != null && e.EmploymentDetail.Category == filter.Category);
            }

            if (filter.JoiningDateFrom.HasValue)
            {
                query = query.Where(e => e.EmploymentDetail != null && e.EmploymentDetail.JoiningDate >= filter.JoiningDateFrom.Value);
            }
            if (filter.JoiningDateTo.HasValue)
            {
                query = query.Where(e => e.EmploymentDetail != null && e.EmploymentDetail.JoiningDate <= filter.JoiningDateTo.Value);
            }

            var total = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                var dir = (filter.SortDirection ?? "asc").ToLower();
                switch (filter.SortBy.ToLower())
                {
                    case "firstname": query = dir == "asc" ? query.OrderBy(e => e.FirstName) : query.OrderByDescending(e => e.FirstName); break;
                    case "lastname": query = dir == "asc" ? query.OrderBy(e => e.LastName) : query.OrderByDescending(e => e.LastName); break;
                    case "joiningdate": query = dir == "asc" ? query.OrderBy(e => e.EmploymentDetail.JoiningDate) : query.OrderByDescending(e => e.EmploymentDetail.JoiningDate); break;
                    default: query = query.OrderBy(e => e.EmployeeId); break;
                }
            }
            else
            {
                query = query.OrderBy(e => e.EmployeeId);
            }

            var page = Math.Max(filter.PageNumber, 1);
            var size = Math.Max(filter.PageSize, 10);

            var items = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            var dtos = items.Select(e => new EmployeeDto
            {
                EmployeeId = e.EmployeeId,
                CompanyId = e.CompanyId,
                DepartmentId = e.DepartmentId,
                EmployeeCode = e.EmployeeCode,
                FirstName = e.FirstName,
                LastName = e.LastName,
                DateOfBirth = e.DateOfBirth,
                Gender = e.Gender,
                Nationality = e.Nationality,
                MotherName = e.MotherName,
                HomeCountryAddress = e.HomeCountryAddress,
                HomeCountryPhone = e.HomeCountryPhone,
                EmergencyContactName = e.EmergencyContactName,
                EmergencyPhone = e.EmergencyPhone,
                Email = e.Email,
                PassportNumber = e.PassportNumber,
                PassportExpiry = e.PassportExpiry,
                PassportCountry = e.PassportCountry,
                PhotoPath = e.PhotoPath,
                Status = e.Status,
                CompanyName = e.Company?.Name ?? string.Empty,
                DepartmentName = e.Department?.Name ?? string.Empty,
                Category = e.EmploymentDetail?.Category ?? string.Empty,
                Designation = e.EmploymentDetail?.OfferDesignation ?? string.Empty,
                JoiningDate = e.EmploymentDetail?.JoiningDate
            }).ToList();

            return (dtos, total);
        }

        public async Task<EmployeeDto> GetByIdAsync(int id)
        {
            var e = await _context.Employees
                .Include(x => x.Company)
                .Include(x => x.Department)
                .Include(x => x.EmploymentDetail)
                .FirstOrDefaultAsync(x => x.EmployeeId == id);

            if (e == null) return null;
            return _mapper.Map<EmployeeDto>(e);
        }

        public async Task<EmployeeProfileDto?> GetProfileAsync(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Company)
                .Include(e => e.Department)
                .Include(e => e.EmploymentDetail)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (employee == null) return null;

            var documents = await _context.EmployeeDocuments
                .Where(x => x.EmployeeId == id).OrderByDescending(x => x.UploadedAt).ToListAsync();
            var attendance = await _context.Attendance
                .Where(x => x.EmployeeId == id).OrderByDescending(x => x.Date).ToListAsync();
            var leave = await _context.LeaveRequests
                .Include(x => x.Employee)
                .Where(x => x.EmployeeId == id).OrderByDescending(x => x.StartDate).ToListAsync();
            var payroll = await _context.Payrolls
                .Where(x => x.EmployeeId == id).OrderByDescending(x => x.Month).ToListAsync();
            var assets = await _context.EmployeeAssets
                .Where(x => x.EmployeeId == id).OrderByDescending(x => x.AssignedDate).ToListAsync();
            var statusHistory = await _context.EmployeeStatusHistories
                .Where(x => x.EmployeeId == id).OrderByDescending(x => x.EffectiveDate).ToListAsync();
            var employmentHistory = await _context.EmployeeEmploymentHistories
                .Where(x => x.EmployeeId == id).OrderByDescending(x => x.EffectiveFrom).ToListAsync();
            var salaryHistory = await _context.IncrementHistories
                .Include(x => x.Employee).Where(x => x.EmployeeId == id)
                .OrderByDescending(x => x.IncrementDate).ToListAsync();
            var settlements = await _context.FinalSettlements
                .Where(x => x.EmployeeId == id).OrderByDescending(x => x.SettlementDate).ToListAsync();
            var customValues = await _customFields.GetValuesAsync(id);
            var customDefinitions = await _customFields.GetDefinitionsAsync();

            return new EmployeeProfileDto
            {
                Employee = _mapper.Map<EmployeeDto>(employee),
                Employment = employee.EmploymentDetail == null ? null : _mapper.Map<EmploymentDetailDto>(employee.EmploymentDetail),
                Documents = _mapper.Map<List<EmployeeDocumentDto>>(documents),
                Attendance = _mapper.Map<List<AttendanceDto>>(attendance),
                Leave = _mapper.Map<List<LeaveRequestResponseDto>>(leave),
                Payroll = _mapper.Map<List<PayrollDto>>(payroll),
                Assets = _mapper.Map<List<EmployeeAssetDto>>(assets),
                StatusHistory = _mapper.Map<List<EmployeeStatusHistoryDto>>(statusHistory),
                EmploymentHistory = _mapper.Map<List<EmployeeEmploymentHistoryDto>>(employmentHistory),
                SalaryHistory = _mapper.Map<List<IncrementHistoryDto>>(salaryHistory),
                FinalSettlements = _mapper.Map<List<FinalSettlementDto>>(settlements),
                CustomFields = customValues,
                CustomFieldDefinitions = customDefinitions,
                Counts = new EmployeeProfileCountsDto
                {
                    Documents = documents.Count, Attendance = attendance.Count, Leave = leave.Count,
                    Payroll = payroll.Count, Assets = assets.Count, StatusHistory = statusHistory.Count,
                    EmploymentHistory = employmentHistory.Count, SalaryHistory = salaryHistory.Count,
                    FinalSettlements = settlements.Count
                }
            };
        }

        public async Task<EmployeeDto> CreateAsync(EmployeeDto dto)
        {
            var department = await _context.Department
                .FirstOrDefaultAsync(d => d.DepartmentId == dto.DepartmentId);
            if (department == null || department.CompanyId != dto.CompanyId)
                throw new InvalidOperationException("The department does not belong to the selected company.");

            var companyExists = await _context.Companies.AnyAsync(c => c.CompanyId == dto.CompanyId);
            if (!companyExists)
                throw new InvalidOperationException("Invalid CompanyId.");

            var entity = new Employee
            {
                CompanyId = dto.CompanyId,
                DepartmentId = dto.DepartmentId,
                EmployeeCode = string.IsNullOrWhiteSpace(dto.EmployeeCode)
                    ? await _numbering.NextAsync("employee")
                    : dto.EmployeeCode.Trim(),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Nationality = dto.Nationality,
                MotherName = dto.MotherName,
                HomeCountryAddress = dto.HomeCountryAddress,
                HomeCountryPhone = dto.HomeCountryPhone,
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyPhone = dto.EmergencyPhone,
                Email = dto.Email,
                PassportNumber = dto.PassportNumber,
                PassportExpiry = dto.PassportExpiry ?? DateTime.MinValue,
                PassportCountry = dto.PassportCountry,
                PhotoPath = dto.PhotoPath,
                Status = dto.Status
            };

            _context.Employees.Add(entity);
            await _context.SaveChangesAsync();
            await _customFields.ValidateAndSaveAsync(entity.EmployeeId, dto.CustomFields);

            dto.EmployeeId = entity.EmployeeId;
            return dto;
        }

        public async Task<EmployeeDto> UpdateAsync(int id, EmployeeDto dto)
        {
            var e = await _context.Employees.FindAsync(id);
            if (e == null) return null;

            e.CompanyId = dto.CompanyId;
            e.EmployeeCode = dto.EmployeeCode;
            e.FirstName = dto.FirstName;
            e.LastName = dto.LastName;
            e.DateOfBirth = dto.DateOfBirth;
            e.Gender = dto.Gender;
            e.Nationality = dto.Nationality;
            e.MotherName = dto.MotherName;
            e.HomeCountryAddress = dto.HomeCountryAddress;
            e.HomeCountryPhone = dto.HomeCountryPhone;
            e.EmergencyContactName = dto.EmergencyContactName;
            e.EmergencyPhone = dto.EmergencyPhone;
            e.Email = dto.Email;
            e.PassportNumber = dto.PassportNumber;
            e.PassportExpiry = dto.PassportExpiry ?? DateTime.MinValue;
            e.PassportCountry = dto.PassportCountry;
            e.PhotoPath = dto.PhotoPath;

            // Status change should go through ChangeStatusAsync to record history. If DTO contains a different status, call ChangeStatusAsync
            if (dto.Status != e.Status)
            {
                await ChangeStatusAsync(e.EmployeeId, dto.Status, DateTime.UtcNow, null, "Updated via API", null, null);
            }

            await _context.SaveChangesAsync();
            if (dto.CustomFields != null)
                await _customFields.ValidateAndSaveAsync(e.EmployeeId, dto.CustomFields);
            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var e = await _context.Employees.FindAsync(id);
            if (e == null) return false;

            _context.Employees.Remove(e);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DuplicateCheckResultDto> DuplicateCheckAsync(DuplicateCheckDto dto)
        {
            // Normalize
            string normPassport = string.IsNullOrWhiteSpace(dto.PassportNumber) ? null : new string(dto.PassportNumber.Where(char.IsLetterOrDigit).ToArray()).ToLower();
            string normNid = string.IsNullOrWhiteSpace(dto.NationalId) ? null : new string(dto.NationalId.Where(char.IsLetterOrDigit).ToArray()).ToLower();
            string normEmail = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLower();
            string normDob = dto.DateOfBirth?.ToString("yyyy-MM-dd");
            string normFullName = (dto.FirstName + " " + dto.LastName).Trim().ToLower();

            var candidates = new List<(Employee emp, int score, List<string> matched)>();

            var employees = await _context.Employees.Include(e => e.EmploymentDetail).Include(e => e.Company).Include(e => e.Department).ToListAsync();

            foreach (var e in employees)
            {
                var matched = new List<string>();
                int score = 0;

                if (!string.IsNullOrWhiteSpace(normPassport) && !string.IsNullOrWhiteSpace(e.PassportNumber))
                {
                    var ep = new string(e.PassportNumber.Where(char.IsLetterOrDigit).ToArray()).ToLower();
                    if (ep == normPassport) { score += 50; matched.Add("PassportNumber"); }
                }
                if (!string.IsNullOrWhiteSpace(normNid) && !string.IsNullOrWhiteSpace(e.EmploymentDetail?.LaborCardNo))
                {
                    var en = new string(e.EmploymentDetail.LaborCardNo.Where(char.IsLetterOrDigit).ToArray()).ToLower();
                    if (en == normNid) { score += 40; matched.Add("NationalId/LaborCard"); }
                }
                if (!string.IsNullOrWhiteSpace(normEmail) && !string.IsNullOrWhiteSpace(e.Email))
                {
                    if (e.Email.Trim().ToLower() == normEmail) { score += 40; matched.Add("Email"); }
                }
                if (!string.IsNullOrWhiteSpace(normFullName) && dto.DateOfBirth.HasValue)
                {
                    var ename = (e.FirstName + " " + e.LastName).Trim().ToLower();
                    if (ename == normFullName && e.DateOfBirth.ToString("yyyy-MM-dd") == normDob)
                    { score += 30; matched.Add("FullName+DOB"); }
                }

                if (score > 0)
                    candidates.Add((e, score, matched));
            }

            var ordered = candidates.OrderByDescending(c => c.score).ToList();
            var result = new DuplicateCheckResultDto
            {
                HasPotentialDuplicates = ordered.Any(),
                MatchScore = ordered.FirstOrDefault().score,
                MatchedFields = ordered.FirstOrDefault().matched ?? new List<string>(),
                Candidates = ordered.Select(c => new EmployeeDto
                {
                    EmployeeId = c.emp.EmployeeId,
                    CompanyId = c.emp.CompanyId,
                    DepartmentId = c.emp.DepartmentId,
                    EmployeeCode = c.emp.EmployeeCode,
                    FirstName = c.emp.FirstName,
                    LastName = c.emp.LastName,
                    DateOfBirth = c.emp.DateOfBirth,
                    Email = c.emp.Email,
                    PassportNumber = c.emp.PassportNumber,
                    PassportExpiry = c.emp.PassportExpiry,
                    PassportCountry = c.emp.PassportCountry,
                    PhotoPath = c.emp.PhotoPath,
                    Status = c.emp.Status,
                    CompanyName = c.emp.Company?.Name ?? string.Empty,
                    DepartmentName = c.emp.Department?.Name ?? string.Empty,
                    Category = c.emp.EmploymentDetail?.Category ?? string.Empty,
                    Designation = c.emp.EmploymentDetail?.OfferDesignation ?? string.Empty,
                    JoiningDate = c.emp.EmploymentDetail?.JoiningDate,
                    NationalId = c.emp.EmploymentDetail?.LaborCardNo ?? string.Empty,
                    MatchedFields = c.matched
                }).ToList()
            };

            return result;
        }

        public async Task<bool> ChangeStatusAsync(int employeeId, EmployeeStatus newStatus, DateTime effectiveDate, DateTime? lastWorkingDate, string reason, int? changedByUserId, int? supportingDocumentId = null)
        {
                    // Validate required inputs
                    if (effectiveDate == default) throw new ArgumentException("EffectiveDate is required and must be provided.");
                    if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Reason is required for status change.");

                    // Define allowed transitions
                    var allowed = new Dictionary<EmployeeStatus, HashSet<EmployeeStatus>>
                    {
                        [EmployeeStatus.Draft] = new HashSet<EmployeeStatus>{ EmployeeStatus.PreJoining, EmployeeStatus.Active, EmployeeStatus.Archived },
                        [EmployeeStatus.PreJoining] = new HashSet<EmployeeStatus>{ EmployeeStatus.Active, EmployeeStatus.Archived },
                        [EmployeeStatus.Active] = new HashSet<EmployeeStatus>{ EmployeeStatus.Probation, EmployeeStatus.OnLeave, EmployeeStatus.Suspended, EmployeeStatus.NoticePeriod, EmployeeStatus.Resigned, EmployeeStatus.Terminated },
                        [EmployeeStatus.Probation] = new HashSet<EmployeeStatus>{ EmployeeStatus.Active, EmployeeStatus.Suspended, EmployeeStatus.NoticePeriod, EmployeeStatus.Resigned, EmployeeStatus.Terminated },
                        [EmployeeStatus.OnLeave] = new HashSet<EmployeeStatus>{ EmployeeStatus.Active, EmployeeStatus.NoticePeriod, EmployeeStatus.Resigned, EmployeeStatus.Terminated },
                        [EmployeeStatus.Suspended] = new HashSet<EmployeeStatus>{ EmployeeStatus.Active, EmployeeStatus.NoticePeriod, EmployeeStatus.Resigned, EmployeeStatus.Terminated },
                        [EmployeeStatus.NoticePeriod] = new HashSet<EmployeeStatus>{ EmployeeStatus.Active, EmployeeStatus.Resigned, EmployeeStatus.Terminated },
                        [EmployeeStatus.Resigned] = new HashSet<EmployeeStatus>{ EmployeeStatus.Archived },
                        [EmployeeStatus.Terminated] = new HashSet<EmployeeStatus>{ EmployeeStatus.Archived },
                        [EmployeeStatus.ContractCompleted] = new HashSet<EmployeeStatus>{ EmployeeStatus.Archived },
                        [EmployeeStatus.Archived] = new HashSet<EmployeeStatus>()
                    };

                    using var tx = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var e = await _context.Employees.Include(x => x.EmploymentDetail).FirstOrDefaultAsync(x => x.EmployeeId == employeeId);
                        if (e == null) throw new KeyNotFoundException($"Employee {employeeId} not found.");

                        var previous = e.Status;
                        if (!allowed.ContainsKey(previous) || !allowed[previous].Contains(newStatus))
                        {
                            throw new InvalidOperationException($"Invalid status transition from {previous} to {newStatus}.");
                        }

                        // Require last working date when moving to Resigned, Terminated, ContractCompleted
                        if ((newStatus == EmployeeStatus.Resigned || newStatus == EmployeeStatus.Terminated || newStatus == EmployeeStatus.ContractCompleted) && !lastWorkingDate.HasValue)
                        {
                            throw new ArgumentException("LastWorkingDate is required when setting status to Resigned, Terminated or ContractCompleted.");
                        }

                        // Update status and employment IsActive per mapping
                        e.Status = newStatus;

                        if (e.EmploymentDetail != null)
                        {
                            if (newStatus == EmployeeStatus.Active || newStatus == EmployeeStatus.Probation || newStatus == EmployeeStatus.OnLeave || newStatus == EmployeeStatus.Suspended || newStatus == EmployeeStatus.NoticePeriod)
                                e.EmploymentDetail.IsActive = true;
                            else if (newStatus == EmployeeStatus.Resigned || newStatus == EmployeeStatus.Terminated || newStatus == EmployeeStatus.ContractCompleted || newStatus == EmployeeStatus.Archived)
                                e.EmploymentDetail.IsActive = false;
                        }

                        var hist = new EmployeeStatusHistory
                        {
                            EmployeeId = employeeId,
                            PreviousStatus = previous,
                            NewStatus = newStatus,
                            EffectiveDate = effectiveDate,
                            Reason = reason,
                            ChangedByUserId = changedByUserId,
                            ChangedAt = DateTime.UtcNow,
                            SupportingDocumentId = supportingDocumentId
                        };

                        _context.EmployeeStatusHistories.Add(hist);

                        await _context.SaveChangesAsync();
                        await tx.CommitAsync();
                        return true;
                    }
                    catch
                    {
                        await tx.RollbackAsync();
                        throw;
                    }
                }
    }
}