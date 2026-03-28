using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Carzi.Models.ViewModels;
using Carzi.Data;
using Carzi.Models;

namespace Carzi.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Authorize]
    public IActionResult Index(string? tab)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "AdminUsers");
            else if (User.IsInRole("User"))
                return RedirectToAction("Index", "UserDashboard");
        }

        // GUEST VIEW (not logged in)
        var model = new TripCalculatorViewModel
        {
            Fuels = _context.FuelTypes.ToList(),
            Vignettes = _context.VignetteTypes
                .OrderBy(v => v.ValidityDays)
                .ToList()
        };

        ViewBag.ActiveTab = "tripcalc";

        return View(model);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult TripCalculator()
    {
        var model = new TripCalculatorViewModel
        {
            Fuels = _context.FuelTypes.ToList(),
            Vignettes = _context.VignetteTypes
                .OrderBy(v => v.ValidityDays)
                .ToList()
        };

        return View(model);
    }

    // Privacy Policy
    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult About()
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
