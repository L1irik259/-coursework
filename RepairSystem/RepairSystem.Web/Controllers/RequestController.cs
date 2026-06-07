using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Core.Data;
using RepairSystem.Core.DTOs;
using RepairSystem.Core.Interfaces;
using RepairSystem.Core.Models;
using System.Security.Claims;

namespace RepairSystem.Web.Controllers;

[Authorize]
public class RequestController : Controller
{
    private readonly AppDbContext _db;
    private readonly IRequestService _requestService;
    private readonly IQrCodeService _qr;

    public RequestController(AppDbContext db, IRequestService requestService, IQrCodeService qr)
    {
        _db = db;
        _requestService = requestService;
        _qr = qr;
    }

    // Клиент: список своих заявок
    [Authorize(Roles = "Клиент")]
    public async Task<IActionResult> Index(string? search, int? statusId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var query = _db.Requests
            .Include(r => r.Status)
            .Include(r => r.Equipment)
            .Where(r => r.ClientId == userId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Number.Contains(search) || r.FaultDescription.Contains(search));

        if (statusId.HasValue)
            query = query.Where(r => r.StatusId == statusId);

        ViewBag.Statuses = await _db.RequestStatuses.ToListAsync();
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentStatus = statusId;

        return View(await query.OrderByDescending(r => r.CreatedAt).ToListAsync());
    }

    // Исполнитель: список назначенных заявок
    [Authorize(Roles = "Исполнитель")]
    public async Task<IActionResult> Assigned(string? search, int? statusId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var query = _db.Requests
            .Include(r => r.Status)
            .Include(r => r.Equipment)
            .Include(r => r.Client)
            .Where(r => r.ExecutorId == userId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Number.Contains(search) || r.FaultDescription.Contains(search));

        if (statusId.HasValue)
            query = query.Where(r => r.StatusId == statusId);

        ViewBag.Statuses = await _db.RequestStatuses.ToListAsync();
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentStatus = statusId;

        return View(await query.OrderByDescending(r => r.CreatedAt).ToListAsync());
    }

    // Создание заявки — GET
    [Authorize(Roles = "Клиент")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Equipment = await _db.Equipment.ToListAsync();
        return View();
    }

    // Создание заявки — POST
    [Authorize(Roles = "Клиент")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int equipmentId, string faultDescription, string problemDescription, DateTime? deadline)
    {
        if (string.IsNullOrWhiteSpace(faultDescription) || string.IsNullOrWhiteSpace(problemDescription))
        {
            ModelState.AddModelError("", "Заполните все поля");
            ViewBag.Equipment = await _db.Equipment.ToListAsync();
            return View();
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _requestService.CreateAsync(new CreateRequestDto
        {
            EquipmentId = equipmentId,
            FaultDescription = faultDescription,
            ProblemDescription = problemDescription,
            ClientId = userId,
            Deadline = deadline
        });

        TempData["Success"] = "Заявка успешно создана";
        return RedirectToAction("Index");
    }

    // Детали заявки
    public async Task<IActionResult> Details(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);

        var request = await _db.Requests
            .Include(r => r.Status)
            .Include(r => r.Equipment)
            .Include(r => r.Client)
            .Include(r => r.Executor)
            .Include(r => r.Manager)
            .Include(r => r.Comments).ThenInclude(c => c.Author)
            .Include(r => r.Consultants).ThenInclude(rc => rc.Specialist)
            .FirstOrDefaultAsync(r => r.RequestId == id);

        if (request == null) return NotFound();

        // Доступ: клиент видит только свои, исполнитель — только назначенные
        if (role == "Клиент" && request.ClientId != userId) return Forbid();
        if (role == "Исполнитель" && request.ExecutorId != userId) return Forbid();

        // QR только на выполненных
        if (request.StatusId == 3)
            ViewBag.QrFormUrl = _qr.GetFormUrl();

        ViewBag.Statuses = await _db.RequestStatuses.ToListAsync();
        ViewBag.CurrentUserId = userId;
        ViewBag.CurrentRole = role;

        return View(request);
    }

    // Смена статуса
    [Authorize(Roles = "Исполнитель,Менеджер")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int requestId, int statusId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _requestService.ChangeStatusAsync(requestId, statusId, userId);
        return RedirectToAction("Details", new { id = requestId });
    }

    // Запрос помощи — исполнитель
    [Authorize(Roles = "Исполнитель")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestHelp(int requestId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _requestService.RequestHelpAsync(requestId, userId);
        TempData["Success"] = "Запрос помощи отправлен менеджеру";
        return RedirectToAction("Details", new { id = requestId });
    }

    // Добавить комментарий
    [Authorize(Roles = "Исполнитель")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int requestId, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return RedirectToAction("Details", new { id = requestId });

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _db.Comments.Add(new Comment
        {
            RequestId = requestId,
            AuthorId = userId,
            Text = text.Trim(),
            CreatedAt = DateTime.Now
        });
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", new { id = requestId });
    }
}
