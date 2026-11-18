using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using LernPlaner.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LernPlaner.Api;
public sealed class SessionAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, LernPlanerDbContext db) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public static string Digest(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.Ordinal))
            return AuthenticateResult.NoResult();
        var token = header[7..];
        if (token.Length != 64)
            return AuthenticateResult.Fail("Invalid session.");
        var hash = Digest(token);
        var session = await db.ApiSessions.AsNoTracking().SingleOrDefaultAsync(x => x.TokenHash == hash && x.ExpiresAt > DateTime.UtcNow, Context.RequestAborted);
        if (session == null)
            return AuthenticateResult.Fail("Expired session.");
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == session.UserId && x.IsActive, Context.RequestAborted);
        if (user == null || session.CredentialStamp != Digest(user.PasswordHash))
            return AuthenticateResult.Fail("Revoked session.");
        Context.Items["actor"] = user;
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), new Claim(ClaimTypes.Role, user.Role) }, Scheme.Name)), Scheme.Name));
    }
}
