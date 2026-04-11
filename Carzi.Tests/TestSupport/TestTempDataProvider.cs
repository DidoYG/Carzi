using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Carzi.Tests.TestSupport;

internal sealed class TestTempDataProvider : ITempDataProvider
{
    private readonly Dictionary<string, object?> _store = new();

    public IDictionary<string, object?> LoadTempData(HttpContext context)
        => new Dictionary<string, object?>(_store);

    public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
    {
        _store.Clear();
        foreach (var (key, value) in values)
        {
            _store[key] = value;
        }
    }
}

