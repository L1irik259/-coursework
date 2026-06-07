using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Core.Interfaces;

namespace RepairSystem.Web.Controllers;

[Authorize(Roles = "Менеджер")]
public class StatisticsController : Controller
{
    private readonly IStatisticsService _stats;

    public StatisticsController(IStatisticsService stats) => _stats = stats;

    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var dateFrom = from ?? DateTime.Now.AddMonths(-6);
        var dateTo = to ?? DateTime.Now;

        var stats = await _stats.GetAsync(dateFrom, dateTo);

        ViewBag.From = dateFrom.ToString("yyyy-MM-dd");
        ViewBag.To = dateTo.ToString("yyyy-MM-dd");

        return View(stats);
    }
}
