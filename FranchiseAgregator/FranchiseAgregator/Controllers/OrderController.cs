using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FranchiseAgregator.Models;
using FranchiseAgregator.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

namespace FranchiseAgregator.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PdfOrderService _pdfService;

        public OrderController(ApplicationDbContext context, PdfOrderService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        // === СПИСОК ЗАЯВОК ПОЛЬЗОВАТЕЛЯ ===
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim);

            var orders = await _context.Orders
                .Include(o => o.Franchise)
                .Include(o => o.OrderStatus)
                .Include(o => o.Region)
                .Include(o => o.File)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // === УПРАВЛЕНИЕ ЗАЯВКАМИ (ТОЛЬКО ПЕРСОНАЛ) ===
        [HttpGet]
        [Authorize(Roles = "Менеджер,Администратор сайта,Администратор")]
        public async Task<IActionResult> Management()
        {
            var orders = await _context.Orders
                .Include(o => o.Franchise)
                .Include(o => o.OrderStatus)
                .Include(o => o.Region)
                .Include(o => o.File)
                .Include(o => o.Client)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.OrderStatuses = await _context.OrderStatuses.ToListAsync();
            return View(orders);
        }

        // === ДЕТАЛИ ЗАЯВКИ ===
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim);
            bool isStaff = User.IsInRole("Менеджер") || User.IsInRole("Администратор сайта") || User.IsInRole("Администратор");

            var order = await _context.Orders
                .Include(o => o.Franchise)
                .Include(o => o.OrderStatus)
                .Include(o => o.Region)
                .Include(o => o.File)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null || (order.UserId != userId && !isStaff))
            {
                return NotFound();
            }

            if (isStaff)
            {
                ViewBag.OrderStatuses = await _context.OrderStatuses.ToListAsync();
            }

            return View(order);
        }

        // === ИЗМЕНЕНИЕ СТАТУСА ЗАКАЗА (ТОЛЬКО ПЕРСОНАЛ) ===
        [HttpPost]
        [Authorize(Roles = "Менеджер,Администратор сайта,Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int orderId, int newStatusId, string? documentUrl)
        {
            if (orderId <= 0)
            {
                TempData["ErrorMessage"] = "Неверный ID заказа.";
                return RedirectToAction("Index");
            }

            if (newStatusId <= 0)
            {
                TempData["ErrorMessage"] = "Неверный ID статуса.";
                return RedirectToAction("Details", new { id = orderId });
            }

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderStatus)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Заказ не найден.";
                    return RedirectToAction("Index");
                }

                var newStatus = await _context.OrderStatuses.FindAsync(newStatusId);
                if (newStatus == null)
                {
                    TempData["ErrorMessage"] = "Статус не найден.";
                    return RedirectToAction("Details", new { id = orderId });
                }

                bool isCompleted = newStatus.Name.Contains("Выполнен", StringComparison.OrdinalIgnoreCase);

                if (isCompleted)
                {
                    if (string.IsNullOrWhiteSpace(documentUrl))
                    {
                        TempData["ErrorMessage"] = "Для статуса «Выполнена» необходимо указать ссылку на документ — по ней будет сформирован QR-код в чеке.";
                        return RedirectToAction("Details", new { id = orderId });
                    }

                    if (!Regex.IsMatch(documentUrl, @"^https?://.+\..+", RegexOptions.IgnoreCase))
                    {
                        TempData["ErrorMessage"] = "Неверный формат ссылки. Пример: https://disk.yandex.ru/i/...";
                        return RedirectToAction("Details", new { id = orderId });
                    }
                }

                string oldStatusName = order.OrderStatus?.Name ?? "Неизвестно";
                order.OrderStatusId = newStatusId;
                await _context.SaveChangesAsync();

                if (isCompleted)
                {
                    await AttachPdfToOrder(orderId, documentUrl!.Trim());
                }

                TempData["SuccessMessage"] = $"Статус заказа #{orderId} изменён с \"{oldStatusName}\" на \"{newStatus.Name}\"";
                return RedirectToAction("Details", new { id = orderId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при изменении статуса: {ex.Message}";
                return RedirectToAction("Details", new { id = orderId });
            }
        }

        // === ДОБАВЛЕНИЕ ДОКУМЕНТА К ЗАКАЗУ (ТОЛЬКО ПЕРСОНАЛ) ===
        [HttpPost]
        [Authorize(Roles = "Менеджер,Администратор сайта,Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDocument(int orderId, string documentUrl)
        {
            if (orderId <= 0)
            {
                TempData["ErrorMessage"] = "Неверный ID заказа.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(documentUrl))
            {
                TempData["ErrorMessage"] = "URL документа обязателен для заполнения.";
                return RedirectToAction("Details", new { id = orderId });
            }

            if (documentUrl.Length > 500)
            {
                TempData["ErrorMessage"] = "URL документа не должен превышать 500 символов.";
                return RedirectToAction("Details", new { id = orderId });
            }

            if (!Regex.IsMatch(documentUrl, @"^https?://.+\..+", RegexOptions.IgnoreCase))
            {
                TempData["ErrorMessage"] = "Неверный формат URL. Пример: https://example.com/file.pdf";
                return RedirectToAction("Details", new { id = orderId });
            }

            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Заказ не найден.";
                    return RedirectToAction("Index");
                }

                if (order.FileId.HasValue)
                {
                    var existingFile = await _context.Files.FindAsync(order.FileId.Value);
                    if (existingFile != null)
                    {
                        existingFile.FileUri = documentUrl.Trim();
                    }
                }
                else
                {
                    var newFile = new FranchiseAgregator.Models.DbFile
                    {
                        FileUri = documentUrl.Trim()
                    };

                    _context.Files.Add(newFile);
                    await _context.SaveChangesAsync();
                    order.FileId = newFile.FileId;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Документ успешно добавлен к заказу!";
                return RedirectToAction("Details", new { id = orderId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при добавлении документа: {ex.Message}";
                return RedirectToAction("Details", new { id = orderId });
            }
        }

        // === СКАЧИВАНИЕ PDF-ДОКУМЕНТА ===
        [HttpGet]
        [Route("Order/DownloadPdf/{orderId}")]
        [Authorize]
        public async Task<IActionResult> DownloadPdf(int orderId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim);
            bool isStaff = User.IsInRole("Менеджер") || User.IsInRole("Администратор сайта") || User.IsInRole("Администратор");

            var order = await _context.Orders
                .Include(o => o.File)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null || (order.UserId != userId && !isStaff))
                return NotFound();

            if (order.File?.FileContent == null)
                return NotFound();

            return File(order.File.FileContent, "application/pdf",
                order.File.FileName ?? $"order_{orderId}.pdf");
        }

        // === УДАЛЕНИЕ ДОКУМЕНТА ИЗ ЗАКАЗА (ТОЛЬКО ПЕРСОНАЛ) ===
        [HttpPost]
        [Authorize(Roles = "Менеджер,Администратор сайта,Администратор")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDocument(int orderId)
        {
            if (orderId <= 0)
            {
                TempData["ErrorMessage"] = "Неверный ID заказа.";
                return RedirectToAction("Index");
            }

            try
            {
                var order = await _context.Orders
                    .Include(o => o.File)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Заказ не найден.";
                    return RedirectToAction("Index");
                }

                if (order.FileId.HasValue)
                {
                    var file = await _context.Files.FindAsync(order.FileId.Value);
                    if (file != null)
                    {
                        _context.Files.Remove(file);
                        order.FileId = null;
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Документ удалён из заказа.";
                    }
                }

                return RedirectToAction("Details", new { id = orderId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ошибка при удалении документа: {ex.Message}";
                return RedirectToAction("Details", new { id = orderId });
            }
        }
        private async Task AttachPdfToOrder(int orderId, string qrUrl)
        {
            var order = await _context.Orders
                .Include(o => o.Franchise).ThenInclude(f => f.Category)
                .Include(o => o.Franchise).ThenInclude(f => f.Franchiser)
                .Include(o => o.OrderStatus)
                .Include(o => o.Region)
                .Include(o => o.ContactMethod)
                .Include(o => o.Client)
                .Include(o => o.File)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return;

            byte[] pdfBytes = _pdfService.GenerateOrderPdf(order, qrUrl);
            string fileName = $"order_{orderId}.pdf";

            if (order.FileId.HasValue)
            {
                var existingFile = await _context.Files.FindAsync(order.FileId.Value);
                if (existingFile != null)
                {
                    existingFile.FileContent = pdfBytes;
                    existingFile.FileName = fileName;
                    existingFile.FileUri = null;
                }
            }
            else
            {
                var newFile = new DbFile
                {
                    FileContent = pdfBytes,
                    FileName = fileName
                };
                _context.Files.Add(newFile);
                await _context.SaveChangesAsync();
                order.FileId = newFile.FileId;
            }

            await _context.SaveChangesAsync();
        }
    }
}