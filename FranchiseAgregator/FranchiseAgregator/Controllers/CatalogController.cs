using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FranchiseAgregator.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Security.Claims;

namespace FranchiseAgregator.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string search, int? categoryId, int? typeId, string sort, int page = 1)
        {
            try
            {
                // === ВАЛИДАЦИЯ ВХОДНЫХ ДАННЫХ ===
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim();
                    if (search.Length > 100) search = search.Substring(0, 100);
                    search = Regex.Replace(search, @"[^\w\sа-яА-ЯёЁ0-9\-]", "");
                    if (string.IsNullOrWhiteSpace(search)) search = null;
                }

                if (categoryId.HasValue && categoryId.Value <= 0) categoryId = null;
                if (typeId.HasValue && typeId.Value <= 0) typeId = null;

                var validSortOptions = new[] { "price_asc", "price_desc", "name_asc", "name_desc" };
                if (!string.IsNullOrEmpty(sort) && !validSortOptions.Contains(sort)) sort = null;
                if (page < 1) page = 1;

                // === ФОРМИРОВАНИЕ ЗАПРОСА ===
                var query = _context.Franchises
                    .Include(f => f.Category)
                    .Include(f => f.FranType)
                    .Include(f => f.Franchiser)
                    .Include(f => f.FranchiseTags).ThenInclude(ft => ft.Tag)
                    .AsQueryable();

                // 🔥 ГЛАВНОЕ ИЗМЕНЕНИЕ: Фильтр по статусу ТОЛЬКО для клиентов и гостей
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                bool isStaff = User.IsInRole("Менеджер") || User.IsInRole("Администратор сайта") || User.IsInRole("Администратор");

                if (!isStaff)
                {
                    // Клиенты и гости видят ТОЛЬКО активные франшизы
                    query = query.Where(f => f.FranStatusId == 1);
                }
                // Менеджеры и админы видят ВСЕ франшизы (без фильтра)

                // Применение поиска
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(f =>
                        f.Name.ToLower().Contains(searchLower) ||
                        f.Description.ToLower().Contains(searchLower));
                }

                // Фильтр категории
                if (categoryId.HasValue)
                {
                    var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == categoryId.Value);
                    if (categoryExists) query = query.Where(f => f.CategoryId == categoryId.Value);
                }

                // Фильтр типа
                if (typeId.HasValue)
                {
                    var typeExists = await _context.FranTypes.AnyAsync(t => t.FranTypeId == typeId.Value);
                    if (typeExists) query = query.Where(f => f.FranTypeId == typeId.Value);
                }

                // Сортировка
                switch (sort)
                {
                    case "price_asc": query = query.OrderBy(f => f.FinalPrice); break;
                    case "price_desc": query = query.OrderByDescending(f => f.FinalPrice); break;
                    case "name_asc": query = query.OrderBy(f => f.Name); break;
                    case "name_desc": query = query.OrderByDescending(f => f.Name); break;
                    default: query = query.OrderByDescending(f => f.CreatedAt); break;
                }

                // Пагинация
                const int pageSize = 12;
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var franchises = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                // Данные для View
                ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).Select(c => new { c.CategoryId, c.Name }).ToListAsync();
                ViewBag.Types = await _context.FranTypes.OrderBy(t => t.Name).Select(t => new { t.FranTypeId, t.Name }).ToListAsync();
                ViewBag.CurrentSearch = search ?? string.Empty;
                ViewBag.CurrentCategoryId = categoryId;
                ViewBag.CurrentTypeId = typeId;
                ViewBag.CurrentSort = sort ?? string.Empty;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalCount = totalItems;

                return View(franchises);
            }
            catch
            {
                TempData["ErrorMessage"] = "Произошла ошибка при загрузке каталога.";
                ViewBag.Categories = new object[0];
                ViewBag.Types = new object[0];
                ViewBag.TotalPages = 0;
                ViewBag.TotalCount = 0;
                return View(new List<Franchise>());
            }
        }
    }
}