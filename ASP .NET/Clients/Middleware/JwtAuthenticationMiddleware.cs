using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Clients.Services;

namespace Clients.Middleware;

/// <summary>
/// Middleware personalizado para validar y enriquecer JWT con información de roles
/// Equivalente a JwtAuthenticationFilter de Java/Spring Security
/// 
/// Flujo:
/// 1. Extrae el JWT del header Authorization o de cookies
/// 2. Valida el token usando JwtProvider
/// 3. Extrae los claims (incluyendo roles)
/// 4. Establece el contexto de autenticación con la información del token
/// 5. Permite que el siguiente middleware use User.IsInRole() y User.FindFirst(ClaimTypes.Role)
/// </summary>
public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;

    public JwtAuthenticationMiddleware(RequestDelegate next, ILogger<JwtAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IJwtProvider jwtProvider, ITokenBlacklistService tokenBlacklist)
    {
        var requestPath = context.Request.Path;
        var isApiRequest = requestPath.StartsWithSegments("/api");

        try
        {
            // 1. Extraer JWT del header Authorization o de cookies
            string? jwt = ExtractJwtFromRequest(context);

            if (jwt != null)
            {
                if (isApiRequest)
                {
                    _logger.LogInformation($"🔹 Middleware processing API request: {context.Request.Method} {requestPath}");
                }

                // ✅ NUEVO: Verificar si el token está en la blacklist (fue invalidado en logout)
                if (tokenBlacklist.IsTokenBlacklisted(jwt))
                {
                    _logger.LogWarning($"🚫 Token está en blacklist (invalidado por logout) para request: {requestPath}");
                }
                else if (!jwtProvider.IsTokenValid(jwt))
                {
                    _logger.LogWarning($"❌ JWT token is invalid for request: {requestPath}");
                }
                else if (jwtProvider.IsTokenExpired(jwt))
                {
                    _logger.LogWarning($"⏰ JWT token has expired for request: {requestPath}");
                }
                else
                {
                    try
                    {
                        // 3. Obtener todos los claims del token
                        var claims = jwtProvider.GetClaimsFromToken(jwt);
                        _logger.LogDebug($"📋 Claims found in token: {claims.Count()}");

                        // Obtener email para logging (optional, don't warn if missing)
                        string? email = null;
                        try
                        {
                            email = jwtProvider.GetEmailFromToken(jwt);
                        }
                        catch
                        {
                            // Email extraction failed, but we still have claims - just continue
                            email = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "unknown";
                        }

                        // 4. Crear una identidad con todos los claims (incluyendo roles)
                        // IMPORTANTE: usar "jwt" como authenticationType para que IsAuthenticated sea true
                        var identity = new ClaimsIdentity(claims, "jwt");
                        var principal = new ClaimsPrincipal(identity);

                        // 5. Establecer el principal en el contexto HTTP
                        // Ahora está disponible en todos los controladores como User
                        context.User = principal;

                        if (isApiRequest)
                        {
                            _logger.LogInformation($"✅ JWT válido - Usuario: {email}, IsAuthenticated: {context.User.Identity?.IsAuthenticated}, Roles: {string.Join(", ", claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");
                        }
                        else
                        {
                            _logger.LogDebug($"✅ JWT válido para página - Usuario: {email}, Path: {requestPath}");
                        }
                    }
                    catch (Exception claimsEx)
                    {
                        _logger.LogError($"❌ Error al procesar claims del JWT: {claimsEx.Message}");
                        // No lanzar excepción, continuar sin autenticación
                    }
                }
            }
            else
            {
                if (isApiRequest)
                {
                    _logger.LogInformation($"ℹ️ No JWT found for request: {requestPath}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error validando JWT: {ex.Message}");
            // Continuar sin autenticación si hay error
        }

        await _next(context);
    }

    /// <summary>
    /// Extrae el JWT del header Authorization (Bearer) o de la cookie auth_token
    /// Equivalente a getJwtFromRequest() en JwtAuthenticationFilter de Java
    /// </summary>
    private string? ExtractJwtFromRequest(HttpContext context)
    {
        // 1. Intentar obtener del header Authorization: "Bearer {token}"
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader))
        {
            _logger.LogDebug($"Authorization header found: {authHeader.Substring(0, Math.Min(50, authHeader.Length))}...");
            if (authHeader.StartsWith("Bearer ") == true)
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                _logger.LogInformation($"✅ JWT extraído del header Authorization (length: {token.Length})");
                return token;
            }
        }

        // 2. Intentar obtener de la cookie auth_token
        if (context.Request.Cookies.TryGetValue("auth_token", out var cookieToken))
        {
            _logger.LogInformation($"✅ JWT extraído de la cookie auth_token (length: {cookieToken.Length})");
            return cookieToken;
        }

        _logger.LogDebug($"ℹ️ No JWT found - Authorization header: {(authHeader == null ? "null" : "present but invalid")}, Cookie: not found");
        return null;
    }
}
