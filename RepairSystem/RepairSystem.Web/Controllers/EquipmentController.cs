using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Core.Data;
using RepairSystem.Core.Models;

namespace RepairSystem.Web.Controllers;

[Authorize(Roles = "Менеджер")]
public class EquipmentController : Controller
{
    private readonly AppDbContext _db;

    public EquipmentController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index() =>
        View(await _db.Equipment.OrderBy(e => e.Name).ToListAsync());

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name, string serialNumber, string? description)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(serialNumber))
        {
            ModelState.AddModelError("", "Заполните обязательные поля");
            return View();
        }

        _db.Equipment.Add(new Equipment
        {
            Name = name.Trim(),
            SerialNumber = serialNumber.Trim(),
            Description = description?.Trim()
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = "Оборудование добавлено";
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var eq = await _db.Equipment.FindAsync(id);
        if (eq != null)
        {
            _db.Equipment.Remove(eq);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }
}
