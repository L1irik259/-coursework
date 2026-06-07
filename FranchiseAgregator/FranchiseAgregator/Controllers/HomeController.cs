using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FranchiseAggregator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FranchiseAggregator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var popularFranchises = await _context.Franchises
                    .Include(f => f.Category)
                    .Include(f => f.Franchiser)
                    .Include(f => f.FranchiseTags).ThenInclude(ft => ft.Tag)
                    .Where(f => f.FranStatusId == 1)  
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(6)
                    .ToListAsync();

                var news = await _context.News
                    .Include(n => n.Status)
                    .Where(n => n.Status.Name == "Опубликовано")
                    .OrderByDescending(n => n.PublishDate)
                    .Take(3)
                    .ToListAsync();

                ViewBag.PopularFranchises = popularFranchises;
                ViewBag.News = news;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка при загрузке главной.";
                ViewBag.PopularFranchises = new List<Franchise>();
                ViewBag.News = new List<News>();
                return View();
            }
        }

        
        [HttpGet]
        public async Task<IActionResult> About()
        {
            try
            {
                ViewBag.CompanyName = "FranchiseHub";
                ViewBag.Mission = "Мы помогаем предпринимателям находить лучшие бизнес-возможности";

                
                int franchiseCount = await _context.Franchises
                    .Where(f => f.FranStatusId == 1)  
                    .CountAsync();

                int userCount = await _context.Users.CountAsync();

                int reviewCount = await _context.Reviews
                    .Where(r => r.ModerStId == 2)  
                    .CountAsync();

               
                var statsList = new List<Dictionary<string, string>>
        {
            new Dictionary<string, string> { { "count", franchiseCount.ToString() }, { "label", "Франшиз в каталоге" } },
            new Dictionary<string, string> { { "count", userCount.ToString() }, { "label", "Активных пользователей" } },
            new Dictionary<string, string> { { "count", reviewCount.ToString() }, { "label", "Отзывов" } }
        };

                ViewBag.Stats = statsList;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Не удалось загрузить страницу.";
                ViewBag.Stats = new List<Dictionary<string, string>>();
                return View();
            }
        }

        
        [HttpGet]
        public IActionResult Contacts()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendFeedback(Feedback feedback)
        {
            
            if (string.IsNullOrWhiteSpace(feedback.Name))
                ModelState.AddModelError("Name", "Имя обязательно.");

            if (string.IsNullOrWhiteSpace(feedback.Phone))
                ModelState.AddModelError("Phone", "Телефон обязателен.");

            if (!string.IsNullOrWhiteSpace(feedback.Email) &&
                !System.Text.RegularExpressions.Regex.IsMatch(feedback.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                ModelState.AddModelError("Email", "Неверный формат Email.");

            if (string.IsNullOrWhiteSpace(feedback.Message) || feedback.Message.Length < 10)
                ModelState.AddModelError("Message", "Сообщение должно содержать не менее 10 символов.");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Пожалуйста, исправьте ошибки в форме.";
                return View("Contacts", feedback);
            }

            try
            {
                
                feedback.FeedbackStatusId = 1;
                feedback.CreatedAt = DateTime.Now;

                
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    feedback.UserId = userId;
                }

                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Сообщение отправлено! Мы свяжемся с вами в ближайшее время.";
                return RedirectToAction("Contacts");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка отправки: {ex.Message}";
                return View("Contacts", feedback);
            }
        }

        
        [HttpGet]
        public async Task<IActionResult> News()
        {
            try
            {
                var newsList = await _context.News
                    .Where(n => n.NewsStatusId == 5)
                    .OrderByDescending(n => n.PublishDate)
                    .ToListAsync();

                return View("News", newsList);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка при загрузке новостей.";
                return RedirectToAction("Error");
            }
        }

        
        [HttpGet]
        public async Task<IActionResult> Promotions()
        {
            try
            {
                var discountedFranchises = await _context.Franchises
                    .Include(f => f.Category)
                    .Include(f => f.Franchiser)
                    .Include(f => f.FranchiseTags)
                        .ThenInclude(ft => ft.Tag)
                    .Where(f => f.DiscountPercent > 0)
                    .OrderByDescending(f => f.DiscountPercent)
                    .ToListAsync();

                return View("Promotions", discountedFranchises);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка при загрузке акций.";
                return View("Error");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }
}