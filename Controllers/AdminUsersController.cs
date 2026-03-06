using Carzi.Data;
using Carzi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

[Authorize(Roles = "Admin")]
public class AdminUsersController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminUsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Show users
    public IActionResult Index()
    {
        var users = _context.Users.ToList();
        return View(users);
    }

    // Create user (GET)
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // Create user (POST)
    [HttpPost]
    public IActionResult Create(string username, string email, string password, string role)
    {
        if (string.IsNullOrWhiteSpace(username))
            ModelState.AddModelError("Username", "Username is required.");

        if (string.IsNullOrWhiteSpace(email))
            ModelState.AddModelError("Email", "Email is required.");

        if (string.IsNullOrWhiteSpace(password))
            ModelState.AddModelError("Password", "Password is required.");
        else if (password.Length < 8)
            ModelState.AddModelError("Password", "Password must be at least 8 characters.");

        if (!string.IsNullOrWhiteSpace(email) && !new EmailAddressAttribute().IsValid(email))
            ModelState.AddModelError("Email", "Invalid email address.");

        if (!string.IsNullOrWhiteSpace(username) && _context.Users.Any(u => u.Username == username))
            ModelState.AddModelError("Username", "Username already exists.");

        if (!string.IsNullOrWhiteSpace(email) && _context.Users.Any(u => u.Email == email))
            ModelState.AddModelError("Email", "Email already registered.");

        if (!ModelState.IsValid)
            return View();

        var user = new User
        {
            Username = username,
            Email = email,
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "User created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Edit user (GET)
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        return View(user);
    }

    // Edit user (POST)
    [HttpPost]
    public IActionResult Edit(int id, string username, string role)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        if (string.IsNullOrWhiteSpace(username))
            ModelState.AddModelError("Username", "Username is required.");

        if (!string.IsNullOrWhiteSpace(username) &&
            _context.Users.Any(u => u.Username == username && u.Id != id))
            ModelState.AddModelError("Username", "Username already exists.");

        if (!ModelState.IsValid)
            return View(user);

        user.Username = username;
        user.Role = role;
        user.CreatedAt = DateTime.UtcNow;

        _context.SaveChanges();

        TempData["SuccessMessage"] = "User updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Reset password (GET)
    [HttpGet]
    public IActionResult ResetPassword(int id)
    {
        return View(id);
    }

    // Reset password (POST)
    [HttpPost]
    public IActionResult ResetPassword(int id, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            ModelState.AddModelError("Password", "Password is required.");
            return View(id);
        }

        if (newPassword.Length < 8)
        {
            ModelState.AddModelError("Password", "Password must be at least 8 characters.");
            return View(id);
        }

        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.CreatedAt = DateTime.UtcNow;

        _context.SaveChanges();

        TempData["SuccessMessage"] = "Password reset successfully.";
        return RedirectToAction(nameof(Index));
    }

    // Delete user
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        if (user.Username == User.Identity?.Name)
            return BadRequest("You cannot delete yourself.");

        _context.Users.Remove(user);
        _context.SaveChanges();
        TempData["SuccessMessage"] = "User deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
