using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FranchiseAgregator.Models;
using System.Security.Claims;

namespace FranchiseAgregator.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // === ВХОД: GET ===
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // === ВХОД: POST ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorMessage"] = "Введите email и пароль";
                return View();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserStatus)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                TempData["ErrorMessage"] = "Неверный email или пароль";
                return View();
            }

            // Проверка статуса
            if (user.UserStatusId != 1)
            {
                TempData["AccountStatusError"] = "Ваш аккаунт неактивен или заблокирован.";
                return View();
            }

            // Проверка пароля
            if (user.Password != password)
            {
                TempData["ErrorMessage"] = "Неверный email или пароль";
                return View();
            }

            // 🔹 СОЗДАНИЕ CLAIMS С РОЛЬЮ
            var roleName = user.Role?.Name ?? "Пользователь";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName)  // ✅ КРИТИЧНО для ролей
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = rememberMe });

            user.LastLogin = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        // === РЕГИСТРАЦИЯ: GET ===
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // === РЕГИСТРАЦИЯ: POST ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email, string phone,
            string password, string confirmPassword, string acceptTerms)
        {
            // === ВАЛИДАЦИЯ ФИО ===
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 3 || fullName.Length > 100)
                ModelState.AddModelError("FullName", "ФИО от 3 до 100 символов");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(fullName, @"^[а-яА-ЯёЁ\s\-\.]+$"))
                ModelState.AddModelError("FullName", "Только русские буквы");

            // === ВАЛИДАЦИЯ EMAIL ===
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                ModelState.AddModelError("Email", "Введите корректный Email");
            else if (email.Length > 254)
                ModelState.AddModelError("Email", "Email слишком длинный");
            else if (await _context.Users.AnyAsync(u => u.Email == email))
                ModelState.AddModelError("Email", "Такой Email уже занят");

            // === ВАЛИДАЦИЯ ТЕЛЕФОНА ===
            if (string.IsNullOrWhiteSpace(phone) || phone.Length < 5 || phone.Length > 50)
                ModelState.AddModelError("Phone", "Телефон от 5 до 50 символов");

            // === ВАЛИДАЦИЯ ПАРОЛЯ ===
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6 || password.Length > 100)
                ModelState.AddModelError("Password", "Пароль от 6 до 100 символов");

            if (password != confirmPassword)
                ModelState.AddModelError("ConfirmPassword", "Пароли не совпадают");

            if (string.IsNullOrWhiteSpace(acceptTerms))
                ModelState.AddModelError("", "Вы должны согласиться с условиями соглашения");

            if (!ModelState.IsValid)
                return View();

            // Получаем roleId и statusId
            var roleRecord = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Пользователь");
            int roleId = roleRecord?.RoleId ?? 1;

            var statusRecord = await _context.UserStatuses.FirstOrDefaultAsync(s => s.Name == "Активен");
            int statusId = statusRecord?.UserStatusId ?? 1;

            var newUser = new Users
            {
                FullName = fullName.Trim(),
                Email = email.ToLower().Trim(),
                Phone = phone.Trim(),
                Password = password,  // Без хеширования
                RegisteredAt = DateTime.Now,
                LastLogin = null,
                RoleId = roleId,
                UserStatusId = statusId
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Автоматический вход после регистрации
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, newUser.UserId.ToString()),
                new Claim(ClaimTypes.Name, newUser.FullName),
                new Claim(ClaimTypes.Email, newUser.Email),
                new Claim(ClaimTypes.Role, "Пользователь")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["SuccessMessage"] = "Регистрация успешна! Добро пожаловать.";
            return RedirectToAction("Index", "Home");
        }

        // === ПРОФИЛЬ: GET ===
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login");

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userIdClaim);

            if (user == null) return RedirectToAction("Login");

            return View(user);
        }

        // === ПРОФИЛЬ: POST ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(
            string fullName,
            string email,
            string phone,
            string oldPassword,
            string newPassword,
            string confirmPassword)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login");

            var existingUser = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userIdClaim);

            if (existingUser == null) return RedirectToAction("Login");

            // === ВАЛИДАЦИЯ ===
            bool hasError = false;

            // Имя
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Length < 3)
            {
                ModelState.AddModelError("FullName", "Имя: от 3 до 100 символов");
                hasError = true;
            }
            else if (fullName.Length > 100)
            {
                ModelState.AddModelError("FullName", "Имя не должно превышать 100 символов");
                hasError = true;
            }

            // Email
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
            {
                ModelState.AddModelError("Email", "Введите корректный Email");
                hasError = true;
            }
            else if (email.Length > 254)
            {
                ModelState.AddModelError("Email", "Email слишком длинный");
                hasError = true;
            }
            else if (await _context.Users.AnyAsync(u => u.Email == email && u.UserId != existingUser.UserId))
            {
                ModelState.AddModelError("Email", "Этот Email уже занят");
                hasError = true;
            }

            // Телефон
            if (string.IsNullOrWhiteSpace(phone) || phone.Length < 5)
            {
                ModelState.AddModelError("Phone", "Телефон: минимум 5 символов");
                hasError = true;
            }
            else if (phone.Length > 50)
            {
                ModelState.AddModelError("Phone", "Телефон: максимум 50 символов");
                hasError = true;
            }

            // Пароль
            bool passwordAttempt = !string.IsNullOrWhiteSpace(oldPassword) ||
                                   !string.IsNullOrWhiteSpace(newPassword) ||
                                   !string.IsNullOrWhiteSpace(confirmPassword);

            if (passwordAttempt)
            {
                if (string.IsNullOrWhiteSpace(oldPassword))
                    ViewBag.PasswordError = "Введите текущий пароль для подтверждения";
                else if (oldPassword != existingUser.Password)
                    ViewBag.PasswordError = "Неверный текущий пароль";
                else if (string.IsNullOrWhiteSpace(newPassword))
                    ViewBag.PasswordError = "Введите новый пароль";
                else if (newPassword.Length < 6)
                    ViewBag.PasswordError = "Новый пароль: минимум 6 символов";
                else if (newPassword.Length > 100)
                    ViewBag.PasswordError = "Пароль: максимум 100 символов";
                else if (!newPassword.Any(char.IsDigit))
                    ViewBag.PasswordError = "Пароль должен содержать цифру";
                else if (!newPassword.Any(char.IsLetter))
                    ViewBag.PasswordError = "Пароль должен содержать букву";
                else if (newPassword != confirmPassword)
                    ViewBag.PasswordError = "Пароли не совпадают";
                else
                    existingUser.Password = newPassword;

                if (ViewBag.PasswordError != null) hasError = true;
            }

            if (hasError)
            {
                existingUser.FullName = fullName;
                existingUser.Email = email;
                existingUser.Phone = phone;
                return View(existingUser);
            }

            // === СОХРАНЕНИЕ ===
            try
            {
                existingUser.FullName = fullName.Trim();
                existingUser.Email = email.ToLower().Trim();
                existingUser.Phone = phone.Trim();

                _context.Update(existingUser);
                await _context.SaveChangesAsync();

                // Обновляем куки с ролью
                var roleName = existingUser.Role?.Name ?? "Пользователь";
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, existingUser.UserId.ToString()),
                    new Claim(ClaimTypes.Name, existingUser.FullName),
                    new Claim(ClaimTypes.Email, existingUser.Email),
                    new Claim(ClaimTypes.Role, roleName)
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                ViewBag.SuccessMessage = passwordAttempt
                    ? "Профиль и пароль успешно обновлены!"
                    : "Профиль успешно обновлен!";

                return View(existingUser);
            }
            catch (Exception ex)
            {
                ViewBag.PasswordError = $"Ошибка сохранения: {ex.Message}";
                existingUser.FullName = fullName;
                existingUser.Email = email;
                existingUser.Phone = phone;
                return View(existingUser);
            }
        }

        // === ВЫХОД - ПОДДЕРЖКА ОБЕИХ МЕТОДОВ ===
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}