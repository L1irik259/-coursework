using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FranchiseAgregator.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FranchiseAgregator.Controllers
{
    [Authorize(Roles = "Администратор сайта,Администратор")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // === УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ ===
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.UserStatus)
                .Include(u => u.Role)
                .OrderByDescending(u => u.RegisteredAt)
                .ToListAsync();

            ViewBag.UserStatuses = await _context.UserStatuses.ToListAsync();
            ViewBag.Roles = await _context.Roles.ToListAsync();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserStatus(string userId, int userStatusId)
        {
            if (!int.TryParse(userId, out int userIdInt))
            {
                TempData["ErrorMessage"] = "Неверный идентификатор пользователя.";
                return RedirectToAction("Users");
            }

            var user = await _context.Users.FindAsync(userIdInt);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Пользователь не найден.";
                return RedirectToAction("Users");
            }

            user.UserStatusId = userStatusId;

            try
            {
                await _context.SaveChangesAsync();

                // 🔹 ПОЛУЧАЕМ ИМЯ СТАТУСА ИЗ БАЗЫ
                var statusName = await _context.UserStatuses
                    .Where(s => s.UserStatusId == userStatusId)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync();

                TempData["SuccessMessage"] = $"Статус пользователя изменён на '{statusName}'.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
            }

            return RedirectToAction("Users");
        }

       

        // === УПРАВЛЕНИЕ ОБРАТНОЙ СВЯЗЬЮ ===
        [HttpGet]
        public async Task<IActionResult> Feedbacks()
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Status)
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            ViewBag.FeedbackStatuses = await _context.FeedbackStatuses.ToListAsync();

            return View(feedbacks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFeedbackStatus(int id, int feedbackStatusId)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                TempData["ErrorMessage"] = "Обращение не найдено.";
                return RedirectToAction("Feedbacks");
            }

            feedback.FeedbackStatusId = feedbackStatusId;

            try
            {
                await _context.SaveChangesAsync();
                var statusName = await _context.FeedbackStatuses
                    .Where(s => s.FeedbackStatusId == feedbackStatusId)
                    .Select(s => s.Name)
                    .FirstOrDefaultAsync();
                TempData["SuccessMessage"] = $"Статус обращения изменён на '{statusName}'.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
            }

            return RedirectToAction("Feedbacks");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseFeedback(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                TempData["ErrorMessage"] = "Обращение не найдено.";
                return RedirectToAction("Feedbacks");
            }

            // Статус "Закрыто" = ID 4
            feedback.FeedbackStatusId = 4;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Обращение закрыто.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка: {ex.Message}";
            }

            return RedirectToAction("Feedbacks");
        }
    }
}