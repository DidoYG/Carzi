using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Carzi.Tests.TestSupport;

internal sealed class TestAuthenticationService : IAuthenticationService
{
    public List<(string Scheme, ClaimsPrincipal Principal, AuthenticationProperties? Properties)> SignIns { get; } = [];
    public List<(string Scheme, AuthenticationProperties? Properties)> SignOuts { get; } = [];

    public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
    {
        var properties = new AuthenticationProperties { IsPersistent = true };
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity()), properties, scheme ?? string.Empty);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        => Task.CompletedTask;

    public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        => Task.CompletedTask;

    public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
    {
        SignIns.Add((scheme ?? string.Empty, principal, properties));
        return Task.CompletedTask;
    }

    public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
    {
        SignOuts.Add((scheme ?? string.Empty, properties));
        return Task.CompletedTask;
    }
}

