using Carzi.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Carzi.Tests.TestSupport;

internal static class ControllerFactory
{
    public static (T Controller, TestAuthenticationService Auth) Create<T>(
        ApplicationDbContext context,
        int? userId = null,
        string role = "User") where T : Controller
    {
        var services = new ServiceCollection();
        var auth = new TestAuthenticationService();
        services.AddSingleton<IAuthenticationService>(auth);
        services.AddSingleton(auth);
        services.AddSingleton<IUrlHelperFactory, TestUrlHelperFactory>();
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        if (userId is not null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.Value.ToString()),
                new(ClaimTypes.Name, "test"),
                new(ClaimTypes.Role, role)
            };

            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        var controller = (T)Activator.CreateInstance(typeof(T), context)!;
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());

        return (controller, auth);
    }
}
