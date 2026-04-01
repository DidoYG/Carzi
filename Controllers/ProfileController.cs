using Carzi.Data;
using Carzi.Models;
using Carzi.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Carzi.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Auth");
            }

            ViewBag.Username = user.Username;
            ViewBag.Email = user.Email;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ChangeUsername()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Auth");
            }

            return View(new ChangeUsernameViewModel { CurrentUsername = user.Username });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUsername(ChangeUsernameViewModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Auth");
            }

            model.CurrentUsername = user.Username;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Invalid password.");
                return View(model);
            }

            var newUsername = model.NewUsername.Trim();
            if (string.Equals(newUsername, user.Username, StringComparison.Ordinal))
            {
                ModelState.AddModelError(nameof(model.NewUsername), "New username must be different.");
                return View(model);
            }

            var exists = await _context.Users.AnyAsync(u => u.Username == newUsername && u.Id != user.Id);
            if (exists)
            {
                ModelState.AddModelError(nameof(model.NewUsername), "Username already exists.");
                return View(model);
            }

            user.Username = newUsername;
            await _context.SaveChangesAsync();

            await RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Username updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Auth");
            }

            return View(new ResetPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Index", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Invalid password.");
                return View(model);
            }

            if (model.NewPassword.Length < 8)
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Password must be at least 8 characters long.");
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Passwords do not match.");
                return View(model);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idValue, out var userId) || userId <= 0)
            {
                return null;
            }

            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        private async Task RefreshSignInAsync(User user)
        {
            var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var isPersistent = authResult?.Properties?.IsPersistent ?? true;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = isPersistent }
            );
        }
    }
}

