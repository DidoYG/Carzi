using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Carzi.Models;

namespace Carzi.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [Authorize]
    public IActionResult Index(string? tab)
    {   
        // Set menu tabs based on user role
        if (User.IsInRole("Guest"))
        {
            ViewBag.ActiveTab = "tripcalc";
        }
        else if (User.IsInRole("Admin"))
        {
            ViewBag.ActiveTab = tab ?? "users";
        }
        else // Registered user
        {
            ViewBag.ActiveTab = tab ?? "dashboard";
        }

        return View();
    }

    // Privacy Policy
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
