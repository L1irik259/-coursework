using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Core.Data;
using RepairSystem.Core.Models;
using System.Security.Claims;

namespace RepairSystem.Web.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            TempData["Error"] = "Введите email и пароль";
            return View();
        }

        var user = await _db.Users
            .Include(u => u.Role)
            .Include(u => u.UserStatus)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null || user.PasswordHash != password)
        {
            TempData["Error"] = "Неверный email или пароль";
            return View();
        }

        if (user.UserStatusId != 1)
        {
            TempData["Error"] = "Ваш аккаунт заблокирован";
            return View();
        }

        user.LastLogin = DateTime.Now;
        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.Name)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string fullName, string email, string phone, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 3)
            ModelState.AddModelError("", "ФИО от 3 символов");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            ModelState.AddModelError("", "Некорректный email");
        if (await _db.Users.AnyAsync(u => u.Email == email))
            ModelState.AddModelError("", "Этот email уже зарегистрирован");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            ModelState.AddModelError("", "Пароль от 6 символов");
        if (password != confirmPassword)
            ModelState.AddModelError("", "Пароли не совпадают");

        if (!ModelState.IsValid) return View();

        var user = new User
        {
            FullName = fullName.Trim(),
            Email = email.ToLower().Trim(),
            Phone = phone.Trim(),
            PasswordHash = password,
            RegisteredAt = DateTime.Now,
            RoleId = 1,
            UserStatusId = 1
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, "Клиент")
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied() => View();
}
