using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Clients.Authentication;

/// <summary>
/// Handler de autenticación "no-op" (no operation)
/// Solo existe para registrar el esquema "jwt" en ASP.NET Core
/// 
/// El verdadero trabajo de validación de JWT lo hace el middleware personalizado JwtAuthenticationMiddleware
/// que configura context.User directamente.
/// 
/// Este handler nunca será realmente invocado porque nuestro middleware personalizado
/// ya ha configurado el contexto de autenticación antes de llegar a este punto.
/// </summary>
public class NoOpAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public NoOpAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // El middleware personalizado ya ha configurado context.User
        // Este handler nunca debería ser invocado
        Logger.LogDebug("NoOpAuthenticationHandler invoked - should not happen with custom JWT middleware");
        return Task.FromResult(AuthenticateResult.NoResult());
    }
}
