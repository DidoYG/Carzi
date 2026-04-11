using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Carzi.Tests.TestSupport;

internal sealed class TestUrlHelperFactory : IUrlHelperFactory
{
    public IUrlHelper GetUrlHelper(ActionContext context)
        => new TestUrlHelper(context);
}

