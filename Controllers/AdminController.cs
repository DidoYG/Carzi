using Carzi.Data;
using Carzi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(string tab = "Users")
    {
        ViewBag.ActiveTab = tab;

        if (tab == "Users")
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        return View(new List<User>());
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public IActionResult Create(string username, string email, string password, string role)
    {
        if (_context.Users.Any(u => u.Username == username))
        {
            ModelState.AddModelError("", "Username already exists.");
            return View();
        }

        var user = new User
        {
            Username = username,
            Email = email,
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        return RedirectToAction("Index", new { tab = "Users" });
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    public IActionResult Edit(int id, string username, string role)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        user.Username = username;
        user.Role = role;

        _context.SaveChanges();
        return RedirectToAction("Index", new { tab = "Users" });
    }

    [HttpGet]
    public IActionResult ResetPassword(int id) => View(id);

    [HttpPost]
    public IActionResult ResetPassword(int id, string newPassword)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _context.SaveChanges();

        return RedirectToAction("Index", new { tab = "Users" });
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        if (user.Username == User.Identity?.Name)
            return BadRequest("Cannot delete yourself.");

        _context.Users.Remove(user);
        _context.SaveChanges();

        return RedirectToAction("Index", new { tab = "Users" });
    }
}