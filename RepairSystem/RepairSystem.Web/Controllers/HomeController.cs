using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RepairSystem.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.IsInRole("Клиент"))
            return RedirectToAction("Index", "Request");
        if (User.IsInRole("Исполнитель"))
            return RedirectToAction("Assigned", "Request");
        if (User.IsInRole("Менеджер"))
            return RedirectToAction("All", "Manager");

        return View();
    }

    public IActionResult Error() => View();
}
