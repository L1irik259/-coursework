namespace RepairSystem.Core.DTOs;

public class StatisticsDto
{
    public int TotalRequests { get; set; }
    public int CompletedRequests { get; set; }
    public int ActiveRequests { get; set; }
    public int PendingRequests { get; set; }
    public double AverageCompletionHours { get; set; }
    public List<MonthlyStatDto> MonthlyStats { get; set; } = new();
    public List<FaultTypeStatDto> FaultTypeStats { get; set; } = new();
}

public class MonthlyStatDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    public int Count { get; set; }
    public double AvgCompletionHours { get; set; }
}

public class FaultTypeStatDto
{
    public string FaultType { get; set; } = null!;
    public int Count { get; set; }
}
