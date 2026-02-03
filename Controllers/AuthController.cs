using Carzi.Models;
using Carzi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Carzi.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Logic for displaying login page
        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.ShowRegister = true;
            return View("Index");
        }

        // Login method POST
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid username or password.";
                return View("Index");
            }

            await SignInUser(user.Id, user.Username, user.Role, true);

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        // Register method POST
        [HttpPost]
        public async Task<IActionResult> Register(
            string username,
            string email,
            string password,
            string confirmPassword)
        {
            ViewBag.ShowRegister = true;

            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View("Index");
            }

            if (password.Length < 8)
            {
                ViewBag.Error = "Password must be at least 8 characters long.";
                return View("Index");
            }

            if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
            {
                ViewBag.Error = "Invalid email address.";
                return View("Index");
            }

            if (_context.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Username already exists.";
                return View("Index");
            }

            if (_context.Users.Any(u => u.Email == email))
            {
                ViewBag.Error = "Email already registered.";
                return View("Index");
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "User"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            await SignInUser(user.Id, user.Username, user.Role, true);

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Home");

        }

        // Continue as Guest POST
        [HttpPost]
        public async Task<IActionResult> Guest()
        {
            await SignInUser(0, "Guest", "Guest", false);
            return RedirectToAction("Index", "Home");
        }

        // LOGOUT
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }

        // Shared logic to sign in user
        private async Task SignInUser(int id, string username, string role, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = isPersistent
                }
            );
        }
    }
}
