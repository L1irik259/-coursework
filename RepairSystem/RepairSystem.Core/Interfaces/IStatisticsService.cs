using RepairSystem.Core.DTOs;

namespace RepairSystem.Core.Interfaces;

public interface IStatisticsService
{
    Task<StatisticsDto> GetAsync(DateTime from, DateTime to);
}
