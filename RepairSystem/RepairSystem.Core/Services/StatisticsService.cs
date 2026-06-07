using Microsoft.EntityFrameworkCore;
using RepairSystem.Core.Data;
using RepairSystem.Core.DTOs;
using RepairSystem.Core.Interfaces;

namespace RepairSystem.Core.Services;

public class StatisticsService : IStatisticsService
{
    private readonly AppDbContext _db;

    public StatisticsService(AppDbContext db) => _db = db;

    public async Task<StatisticsDto> GetAsync(DateTime from, DateTime to)
    {
        var requests = await _db.Requests
            .Include(r => r.Status)
            .Where(r => r.CreatedAt >= from && r.CreatedAt <= to.AddDays(1))
            .ToListAsync();

        var completed = requests.Where(r => r.StatusId == 3).ToList();

        var avgHours = completed.Any(r => r.CompletedAt.HasValue)
            ? completed.Where(r => r.CompletedAt.HasValue)
                .Average(r => (r.CompletedAt!.Value - r.CreatedAt).TotalHours)
            : 0;

        var monthly = requests
            .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
            .Select(g => new MonthlyStatDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count(),
                AvgCompletionHours = g.Where(r => r.CompletedAt.HasValue)
                    .Select(r => (r.CompletedAt!.Value - r.CreatedAt).TotalHours)
                    .DefaultIfEmpty(0).Average()
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();

        var faultTypes = requests
            .GroupBy(r => r.FaultDescription)
            .Select(g => new FaultTypeStatDto { FaultType = g.Key, Count = g.Count() })
            .OrderByDescending(f => f.Count)
            .Take(10)
            .ToList();

        return new StatisticsDto
        {
            TotalRequests = requests.Count,
            CompletedRequests = completed.Count,
            ActiveRequests = requests.Count(r => r.StatusId == 2),
            PendingRequests = requests.Count(r => r.StatusId == 1),
            AverageCompletionHours = Math.Round(avgHours, 1),
            MonthlyStats = monthly,
            FaultTypeStats = faultTypes
        };
    }
}
