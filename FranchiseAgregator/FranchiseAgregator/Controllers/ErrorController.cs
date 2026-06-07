using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FranchiseAggregator.Controllers
{
    public class ErrorController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            // Можно добавить логику логирования ошибки здесь
            return View();
        }
    }
}