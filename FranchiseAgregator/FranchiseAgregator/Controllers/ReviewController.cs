using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FranchiseAggregator.Models;
using System.Linq;
using System.Threading.Tasks;

namespace FranchiseAggregator.Controllers
{
    [Authorize(Roles = "Администратор сайта,Администратор")]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Franchise)
                .Include(r => r.ModerSt)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.ModerStatuses = await _context.ModerSts.ToListAsync();
            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, int moderStId)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                TempData["ErrorMessage"] = "Отзыв не найден.";
                return RedirectToAction("Manage");
            }

            review.ModerStId = moderStId;
            try
            {
                await _context.SaveChangesAsync();
                var statusName = await _context.ModerSts
                    .Where(s => s.ModerStId == moderStId)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync();
                TempData["SuccessMessage"] = $"Статус отзыва изменён на '{statusName}'.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
            }

            return RedirectToAction("Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                TempData["ErrorMessage"] = "Отзыв не найден.";
                return RedirectToAction("Manage");
            }

            _context.Reviews.Remove(review);
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Отзыв удалён.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка удаления: {ex.Message}";
            }

            return RedirectToAction("Manage");
        }
    }
}