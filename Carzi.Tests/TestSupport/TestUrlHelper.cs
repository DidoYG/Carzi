using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Carzi.Tests.TestSupport;

internal sealed class TestUrlHelper : IUrlHelper
{
    public TestUrlHelper(ActionContext actionContext)
    {
        ActionContext = actionContext;
    }

    public ActionContext ActionContext { get; }

    public string? Action(UrlActionContext actionContext)
    {
        var controller = actionContext.Controller ?? string.Empty;
        var action = actionContext.Action ?? string.Empty;
        return $"/{controller}/{action}";
    }

    public string? Content(string? contentPath) => contentPath;

    public bool IsLocalUrl(string? url) => true;

    public string? Link(string? routeName, object? values) => $"/{routeName}";

    public string? RouteUrl(UrlRouteContext routeContext) => $"/{routeContext.RouteName}";
}
