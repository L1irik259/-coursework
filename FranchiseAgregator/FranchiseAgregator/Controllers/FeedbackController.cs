using Microsoft.AspNetCore.Mvc;
using FranchiseAggregator.Models;
using System.Threading.Tasks;

namespace FranchiseAggregator.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Submit(Feedback feedback)
        {
            // Если есть ошибки (проверили модель), возвращаем форму обратно
            if (!ModelState.IsValid)
            {
                return View("Contacts", feedback);
            }

            // Сохраняем, если всё ок
            await _context.Feedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Спасибо! Ваше сообщение отправлено.";
            return RedirectToAction("Contacts", "Home");
        }
    }
}