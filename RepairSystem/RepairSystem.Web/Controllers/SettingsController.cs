using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Core.Data;

namespace RepairSystem.Web.Controllers;

[Authorize(Roles = "Менеджер")]
public class SettingsController : Controller
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> QrForm()
    {
        var setting = await _db.SystemSettings.FindAsync("QrCodeFormUrl");
        ViewBag.CurrentUrl = setting?.Value ?? "";
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> QrForm(string url)
    {
        var setting = await _db.SystemSettings.FindAsync("QrCodeFormUrl");
        if (setting != null)
        {
            setting.Value = url?.Trim() ?? "";
            await _db.SaveChangesAsync();
        }

        TempData["Success"] = "Ссылка на форму обновлена";
        return RedirectToAction("QrForm");
    }
}
