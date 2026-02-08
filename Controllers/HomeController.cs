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
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction("Index", "Admin");
        }

        var model = new TripCalculatorViewModel();

        if (User.IsInRole("Guest"))
        {
            ViewBag.ActiveTab = "tripcalc";

            model.Fuels = _context.FuelTypes.ToList();

            model.Vignettes = _context.VignetteTypes
                .OrderBy(v => v.ValidityDays)
                .ToList();
        }
        else
        {
            ViewBag.ActiveTab = tab ?? "dashboard";
        }

        return View(model);
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
