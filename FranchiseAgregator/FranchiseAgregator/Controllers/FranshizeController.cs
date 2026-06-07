using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FranchiseAggregator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace FranchiseAggregator.Controllers
{
    public class FranchiseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FranchiseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // === ПРОСМОТР ФРАНШИЗЫ (ДОСТУПНО ВСЕМ) ===
        [HttpGet]
        public async Task<IActionResult> Index(int id)
        {
            try
            {
                var franchise = await _context.Franchises
                    .Include(f => f.Category)
                    .Include(f => f.FranType)
                    .Include(f => f.Franchiser)
                    .Include(f => f.FranchiseTags).ThenInclude(ft => ft.Tag)
                    .Include(f => f.PremTypes)
                    .FirstOrDefaultAsync(f => f.FranchiseId == id);

                if (franchise == null) return NotFound();

                ViewBag.CurrentFranchiseId = id;
                ViewBag.Regions = await _context.Regions.ToListAsync();
                ViewBag.ContactMethods = await _context.ContactMethods.ToListAsync();

                string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    var currentUser = await _context.Users.FindAsync(Convert.ToInt32(userIdClaim));
                    if (currentUser != null) ViewBag.UserData = currentUser;
                }

                ViewBag.Reviews = await _context.Reviews
                    .Include(r => r.User).Include(r => r.ModerSt)
                    .Where(r => r.FranchiseId == id && r.ModerStId == 2)
                    .OrderByDescending(r => r.CreatedAt).Take(10).ToListAsync();

                ViewBag.SimilarFranchises = await _context.Franchises
                    .Include(f => f.Category).Include(f => f.Franchiser).Include(f => f.PremTypes)
                    .Where(f => f.CategoryId == franchise.CategoryId && f.FranchiseId != franchise.FranchiseId)
                    .OrderByDescending(f => f.CreatedAt).Take(3).ToListAsync();

                bool hasDiscount = franchise.DiscountPercent.HasValue && franchise.DiscountPercent > 0 && franchise.DiscountPercent < 100;
                decimal oldPrice = 0, savings = 0;
                if (hasDiscount)
                {
                    oldPrice = franchise.FinalPrice / (1 - (franchise.DiscountPercent.Value / 100m));
                    savings = oldPrice - franchise.FinalPrice;
                }
                ViewBag.HasDiscount = hasDiscount;
                ViewBag.OldPrice = oldPrice;
                ViewBag.Savings = savings;

                return View(franchise);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка загрузки страницы.";
                return RedirectToAction("Index", "Catalog");
            }
        }

        // === ДОБАВЛЕНИЕ ОТЗЫВА ===
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview(int franchiseId, int rating, string text)
        {
            try
            {
                string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

                int userId = Convert.ToInt32(userIdClaim);
                var moderateStatus = await _context.ModerSts.FirstOrDefaultAsync(m => m.ModerStId == 1)
                    ?? await _context.ModerSts.FirstOrDefaultAsync();

                if (moderateStatus == null) throw new Exception("Нет статусов модерации!");

                var newReview = new Review
                {
                    FranchiseId = franchiseId,
                    UserId = userId,
                    Rating = rating,
                    Text = text.Trim(),
                    CreatedAt = DateTime.Now,
                    ModerStId = moderateStatus.ModerStId
                };

                _context.Reviews.Add(newReview);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Отзыв отправлен на модерацию!";
                return RedirectToAction("Index", new { id = franchiseId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка при сохранении отзыва.";
                return RedirectToAction("Index", new { id = franchiseId });
            }
        }

        // === ОТПРАВКА ЗАЯВКИ ===
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendRequest(int id, string FullName, string Phone, string Email, string Comment, int? RegionId, int? ContactMethodId)
        {
            try
            {
                string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

                int userId = Convert.ToInt32(userIdClaim);
                ModelState.Clear();

                if (string.IsNullOrWhiteSpace(FullName) || FullName.Length > 100)
                    ModelState.AddModelError(nameof(FullName), "Неверное имя.");
                if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
                    ModelState.AddModelError(nameof(Email), "Неверный Email.");
                if (string.IsNullOrWhiteSpace(Phone) || Phone.Length < 5)
                    ModelState.AddModelError(nameof(Phone), "Неверный телефон.");

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Пожалуйста, проверьте введенные данные.";
                    return RedirectToAction("Index", new { id });
                }

                var regions = await _context.Regions.ToListAsync();
                var contactMethods = await _context.ContactMethods.ToListAsync();
                var statuses = await _context.OrderStatuses.ToListAsync();

                int safeRegionId = regions.Min(r => r.RegionId);
                int safeContactId = contactMethods.Min(c => c.ContactMethodId);
                int safeStatusId = statuses.Any() ? statuses.Min(s => s.OrderStatusId) : 1;

                string orderNumber = $"ORD-{DateTime.Now.Ticks}";
                decimal priceForOrder = _context.Franchises.Find(id)?.FinalPrice ?? 0;

                var newOrder = new Models.Order
                {
                    OrderNumber = orderNumber,
                    UserId = userId,
                    FranchiseId = id,
                    Comments = string.IsNullOrWhiteSpace(Comment) ? "" : Comment,
                    TotalAmount = priceForOrder,
                    OrderStatusId = safeStatusId,
                    RegionId = RegionId ?? safeRegionId,
                    ContactMethodId = ContactMethodId ?? safeContactId,
                    FileId = null,
                    CreatedAt = DateTime.Now
                };

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ваша заявка успешно отправлена!";
                return RedirectToAction("Index", new { id });
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Критическая ошибка сервера.";
                return RedirectToAction("Index", "Home");
            }
        }

        // === ИЗМЕНЕНИЕ СТАТУСА ФРАНШИЗЫ (ТОЛЬКО МЕНЕДЖЕР И ВЫШЕ) ===
        [HttpPost]
        [Authorize(Roles = "Менеджер,Администратор сайта,Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var franchise = await _context.Franchises.FindAsync(id);
            if (franchise == null) return NotFound();

            // Переключаем между статусами:
            // 1 = Активна
            // 2 = На модерации (неактивна)
            if (franchise.FranStatusId == 1)
            {
                // Если активна -> деактивируем (ставим "На модерации")
                franchise.FranStatusId = 2;
                TempData["SuccessMessage"] = "⏸️ Франшиза деактивирована";
            }
            else
            {
                // Если неактивна -> активируем
                franchise.FranStatusId = 1;
                TempData["SuccessMessage"] = "✅ Франшиза активирована";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { id });
        }
    }
}