using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Core.Data;
using RepairSystem.Core.Interfaces;
using System.Security.Claims;

namespace RepairSystem.Web.Controllers;

[Authorize(Roles = "Менеджер")]
public class ManagerController : Controller
{
    private readonly AppDbContext _db;
    private readonly IRequestService _requestService;

    public ManagerController(AppDbContext db, IRequestService requestService)
    {
        _db = db;
        _requestService = requestService;
    }

    // Все заявки
    public async Task<IActionResult> All(string? search, int? statusId)
    {
        var query = _db.Requests
            .Include(r => r.Status)
            .Include(r => r.Equipment)
            .Include(r => r.Client)
            .Include(r => r.Executor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Number.Contains(search) || r.FaultDescription.Contains(search));

        if (statusId.HasValue)
            query = query.Where(r => r.StatusId == statusId);

        ViewBag.Statuses = await _db.RequestStatuses.ToListAsync();
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentStatus = statusId;

        return View(await query.OrderByDescending(r => r.CreatedAt).ToListAsync());
    }

    // Назначить исполнителя
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignExecutor(int requestId, int executorId)
    {
        await _requestService.AssignExecutorAsync(requestId, executorId);
        TempData["Success"] = "Исполнитель назначен";
        return RedirectToAction("Details", "Request", new { id = requestId });
    }

    // Привлечь специалиста-консультанта
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AttractSpecialist(int requestId, int specialistId)
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _requestService.AttractSpecialistAsync(requestId, specialistId, managerId);
        TempData["Success"] = "Специалист привлечён";
        return RedirectToAction("Details", "Request", new { id = requestId });
    }

    // Продлить срок — GET
    [HttpGet]
    public async Task<IActionResult> ExtendDeadline(int requestId)
    {
        var request = await _db.Requests
            .Include(r => r.Equipment)
            .Include(r => r.Client)
            .FirstOrDefaultAsync(r => r.RequestId == requestId);

        if (request == null) return NotFound();
        return View(request);
    }

    // Продлить срок — POST
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ExtendDeadline(int requestId, DateTime newDeadline, string agreementNote)
    {
        if (string.IsNullOrWhiteSpace(agreementNote))
        {
            TempData["Error"] = "Укажите основание для продления";
            return RedirectToAction("ExtendDeadline", new { requestId });
        }

        await _requestService.ExtendDeadlineAsync(requestId, newDeadline, agreementNote);
        TempData["Success"] = "Срок выполнения продлён, клиент уведомлён";
        return RedirectToAction("Details", "Request", new { id = requestId });
    }

    // Список исполнителей для выбора
    [HttpGet]
    public async Task<IActionResult> Executors()
    {
        var executors = await _db.Users
            .Where(u => u.RoleId == 2 && u.UserStatusId == 1)
            .Select(u => new { u.UserId, u.FullName })
            .ToListAsync();

        return Json(executors);
    }
}
