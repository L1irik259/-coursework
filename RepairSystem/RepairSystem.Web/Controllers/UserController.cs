using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Core.Data;

namespace RepairSystem.Web.Controllers;

[Authorize(Roles = "Менеджер")]
public class UserController : Controller
{
    private readonly AppDbContext _db;

    public UserController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Users
            .Include(u => u.Role)
            .Include(u => u.UserStatus)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

        ViewBag.CurrentSearch = search;
        return View(await query.OrderBy(u => u.FullName).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole(int userId, int roleId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user != null)
        {
            user.RoleId = roleId;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user != null)
        {
            user.UserStatusId = user.UserStatusId == 1 ? 2 : 1;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
}
