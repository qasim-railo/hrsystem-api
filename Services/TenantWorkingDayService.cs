using HRSystem.API.Data;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services;

public sealed class TenantWorkingDayService
{
    private readonly AppDbContext _db;

    public TenantWorkingDayService(AppDbContext db) => _db = db;

    public async Task<bool> IsWorkingDayAsync(DateTime date)
    {
        var day = await _db.TenantWorkingDays.AsNoTracking()
            .SingleOrDefaultAsync(item => item.DayOfWeek == date.DayOfWeek);
        return day?.IsWorkingDay ?? true;
    }
}
