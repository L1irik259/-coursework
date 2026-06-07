using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FranchiseAgregator.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace FranchiseAgregator.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FavoritesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // === ДОБАВИТЬ В ИЗБРАННОЕ ===
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Add(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim);

            var exists = await _context.Favorites.AnyAsync(f => f.FranchiseId == id && f.UserId == userId);

            if (!exists)
            {
                var favorite = new Favorite
                {
                    FranchiseId = id,
                    UserId = userId,
                };

                _context.Favorites.Add(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Catalog");
        }

        // === УДАЛИТЬ ИЗ ИЗБРАННОГО ===
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim);

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.FavoriteId == id && f.UserId == userId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // === СПИСОК ИЗБРАННОГО ===
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim);

            var favorites = await _context.Favorites
                .Include(f => f.Franchise)
                .Include(f => f.Franchise).ThenInclude(fr => fr.Franchiser)
                .Where(f => f.UserId == userId)
                .ToListAsync();

            return View(favorites);
        }
    }
}