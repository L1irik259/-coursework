using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Core.Data;
using System.Security.Claims;

namespace RepairSystem.Web.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly AppDbContext _db;

    public NotificationController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetUnread()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notes = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new { n.NotificationId, n.Message, n.RequestId, n.CreatedAt })
            .ToListAsync();

        return Json(notes);
    }

    [HttpPost]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var note = await _db.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);
        if (note != null)
        {
            note.IsRead = true;
            await _db.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notes = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        notes.ForEach(n => n.IsRead = true);
        await _db.SaveChangesAsync();
        return Ok();
    }
}
