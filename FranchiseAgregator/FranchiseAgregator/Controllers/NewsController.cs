using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FranchiseAgregator.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FranchiseAgregator.Controllers
{
    [Authorize(Roles = "Администратор сайта,Администратор")]
    public class NewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var newsList = await _context.News
                .Include(n => n.Status)
                .OrderByDescending(n => n.PublishDate)
                .ToListAsync();
            return View(newsList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.NewsStatuses = _context.NewsStatuses.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(News news)
        {
            // ✅ ИСПРАВЛЕНО: ID=4 (Черновик)
            if (news.NewsStatusId == 0)
                news.NewsStatusId = 4;

            // 🔍 СЕРВЕРНАЯ ВАЛИДАЦИЯ
            if (string.IsNullOrWhiteSpace(news.Title))
                ModelState.AddModelError("Title", "Заголовок обязателен.");
            else if (news.Title.Length > 200)
                ModelState.AddModelError("Title", "Заголовок не должен превышать 200 символов.");

            if (string.IsNullOrWhiteSpace(news.Text))
                ModelState.AddModelError("Text", "Текст новости обязателен.");
            else if (news.Text.Length > 500)
                ModelState.AddModelError("Text", "Текст не должен превышать 500 символов.");

            if (!string.IsNullOrWhiteSpace(news.ImageUrl))
            {
                if (news.ImageUrl.Length > 500)
                    ModelState.AddModelError("ImageUrl", "Ссылка не должна превышать 500 символов.");
                else if (!Regex.IsMatch(news.ImageUrl, @"^https?://.+\..+", RegexOptions.IgnoreCase))
                    ModelState.AddModelError("ImageUrl", "Неверный формат URL.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.NewsStatuses = _context.NewsStatuses.ToList();
                return View(news);
            }

            try
            {
                news.PublishDate = DateTime.Now;
                _context.News.Add(news);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Новость успешно создана!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var realError = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", $"❌ Ошибка сохранения: {realError}");
                ViewBag.NewsStatuses = _context.NewsStatuses.ToList();
                return View(news);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null) return NotFound();

            ViewBag.NewsStatuses = await _context.NewsStatuses.ToListAsync();
            return View(news);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, News news)
        {
            if (id != news.NewsId) return NotFound();

            // ✅ Если статус не передан, оставляем текущий
            if (news.NewsStatusId == 0)
            {
                var existingNews = await _context.News.FindAsync(id);
                if (existingNews != null)
                    news.NewsStatusId = existingNews.NewsStatusId;
            }

            // 🔍 СЕРВЕРНАЯ ВАЛИДАЦИЯ
            if (string.IsNullOrWhiteSpace(news.Title))
                ModelState.AddModelError("Title", "Заголовок обязателен.");
            else if (news.Title.Length > 200)
                ModelState.AddModelError("Title", "Заголовок не должен превышать 200 символов.");

            if (string.IsNullOrWhiteSpace(news.Text))
                ModelState.AddModelError("Text", "Текст новости обязателен.");
            else if (news.Text.Length > 500)
                ModelState.AddModelError("Text", "Текст не должен превышать 500 символов.");

            if (!string.IsNullOrWhiteSpace(news.ImageUrl))
            {
                if (news.ImageUrl.Length > 500)
                    ModelState.AddModelError("ImageUrl", "Ссылка не должна превышать 500 символов.");
                else if (!Regex.IsMatch(news.ImageUrl, @"^https?://.+\..+", RegexOptions.IgnoreCase))
                    ModelState.AddModelError("ImageUrl", "Неверный формат URL.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.NewsStatuses = await _context.NewsStatuses.ToListAsync();
                return View(news);
            }

            try
            {
                _context.Entry(news).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Новость успешно обновлена!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NewsExists(news.NewsId)) return NotFound();
                else throw;
            }
            catch (Exception ex)
            {
                var realError = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", $"❌ Ошибка обновления: {realError}");
                ViewBag.NewsStatuses = await _context.NewsStatuses.ToListAsync();
                return View(news);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id, int targetStatusId)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null) return NotFound();

            var targetStatus = await _context.NewsStatuses.FindAsync(targetStatusId);
            if (targetStatus == null)
            {
                TempData["ErrorMessage"] = "Неверный целевой статус.";
                return RedirectToAction("Index");
            }

            try
            {
                news.NewsStatusId = targetStatusId;
                if (targetStatus.Name == "Опубликовано")
                    news.PublishDate = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Статус изменён на \"{targetStatus.Name}\"";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка смены статуса: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.NewsId == id);
        }
    }
}