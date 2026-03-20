using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "User")]
public class UserDashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}