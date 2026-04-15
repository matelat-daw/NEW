using Clients.Dtos;
using Clients.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Clients.Controllers
{
    /// <summary>
    /// Controlador optimizado de autenticación
    /// Maneja login, logout, verificación de email y operaciones de auth
    /// Equivalente a AuthController.java en la API JAVA
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtProvider _jwtProvider;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITokenBlacklistService _tokenBlacklist;

        public AuthController(
            IUserService userService, 
            IJwtProvider jwtProvider, 
            ILogger<AuthController> logger,
            IConfiguration configuration,
            ITokenBlacklistService tokenBlacklist)
        {
            _userService = userService;
            _jwtProvider = jwtProvider;
            _logger = logger;
            _configuration = configuration;
            _tokenBlacklist = tokenBlacklist;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// POST /api/auth/login - Autentica al usuario
        /// Retorna token en el body y lo guarda en cookie (como en JAVA)
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<UserDto>>> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("📝 Intento de login para email: {Email}", request.Email);

            try
            {
                // Validar credenciales (lanza excepción si usuario no está verificado)
                bool isValid = await _userService.ValidateCredentialsAsync(request.Email, request.Password);

                if (!isValid)
                {
                    _logger.LogWarning("⚠️ Login fallido: credenciales inválidas para {Email}", request.Email);
                    return Unauthorized(ApiResponse<UserDto>.Unauthorized("Email o contraseña incorrectos"));
                }

                // Obtener usuario
                var user = await _userService.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return Unauthorized(ApiResponse<UserDto>.Unauthorized("Usuario no encontrado"));
                }

                // Generar token JWT
                string token = _jwtProvider.GenerateToken(user);

                // Agregar cookie de autenticación (como en JAVA)
                AddAuthCookie(token);

                _logger.LogInformation("✅ Login exitoso para usuario: {Email}", request.Email);

                // Retornar token en el body + datos del usuario como data (como en JAVA)
                var userDto = UserDto.FromEntity(user);
                var response = new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "Login exitoso",
                    Data = userDto,
                    StatusCode = 200,
                    Timestamp = DateTime.UtcNow
                };

                // Agregar el token al cuerpo de la respuesta (compatible con frontend)
                return Ok(new
                {
                    success = response.Success,
                    message = response.Message,
                    token = token,  // Frontend extrae el token de aquí
                    data = userDto,
                    statusCode = response.StatusCode,
                    timestamp = response.Timestamp
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("⚠️ Error de validación en login: {Message}", ex.Message);
                return Unauthorized(ApiResponse<UserDto>.Unauthorized(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error inesperado en login: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<UserDto>.InternalServerError("Error en el proceso de login"));
            }
        }

        /// <summary>
        /// GET /api/auth/verify/{token} - Verifica el email del usuario
        /// Redirige a la URL configurada tras la verificación (como en JAVA)
        /// </summary>
        [HttpGet("verify/{token}")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            _logger.LogInformation("📧 Intento de verificación de email con token");

            try
            {
                await _userService.VerifyEmailAsync(token);

                // Redirigir a la URL de login del frontend configurada
                var frontendLoginUrl = _configuration["AppUrls:FrontendLoginUrl"] ?? "http://localhost/login";
                return Redirect($"{frontendLoginUrl}?verified=1");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("⚠️ Error en verificación de email: {Message}", ex.Message);
                var frontendRegisterUrl = _configuration["AppUrls:FrontendRegisterUrl"] ?? "http://localhost/register";
                return Redirect($"{frontendRegisterUrl}?verification=failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error inesperado en verificación de email: {Message}", ex.Message);
                var frontendRegisterUrl = _configuration["AppUrls:FrontendRegisterUrl"] ?? "http://localhost/register";
                return Redirect($"{frontendRegisterUrl}?verification=failed");
            }
        }

        /// <summary>
        /// POST /api/auth/refresh - Refresca el JWT token
        /// </summary>
        [Authorize]
        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<object>>> RefreshToken()
        {
            _logger.LogInformation("🔄 Solicitud de refresco de token");

            try
            {
                // Obtener email del usuario autenticado
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(ApiResponse<object>.Unauthorized("No autenticado"));
                }

                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return Unauthorized(ApiResponse<object>.Unauthorized("Usuario no encontrado"));
                }

                // Generar nuevo token
                string newToken = _jwtProvider.GenerateToken(user);
                AddAuthCookie(newToken);

                _logger.LogInformation("✅ Token refrescado para usuario: {Email}", email);

                return Ok(new
                {
                    success = true,
                    message = "Token refrescado exitosamente",
                    data = new { token = newToken },
                    statusCode = 200,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al refrescar token: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<object>.InternalServerError("Error al refrescar token"));
            }
        }

        /// <summary>
        /// GET /api/auth/debug - Endpoint de debug para ver estado de cookies y tokens
        /// SOLO PARA DESARROLLO - Eliminar en producción
        /// </summary>
        [HttpGet("debug")]
        public IActionResult Debug()
        {
            _logger.LogInformation("🔍 DEBUG: Información de autenticación");

            // Obtener cookies
            var cookies = Request.Cookies.ToDictionary(c => c.Key, c => c.Value.Substring(0, Math.Min(50, c.Value.Length)) + "...");

            // Obtener headers
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            var bearerToken = authHeader?.StartsWith("Bearer ") == true 
                ? authHeader.Substring("Bearer ".Length).Substring(0, Math.Min(50, authHeader.Length - 7)) + "..."
                : "None";

            // Obtener info de autenticación
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "None";
            var userId = User.FindFirst("userId")?.Value ?? "None";

            // ✅ NUEVO: Verificar si el token actual está en blacklist
            string? fullToken = ExtractTokenFromRequest();
            bool isTokenBlacklisted = !string.IsNullOrEmpty(fullToken) && _tokenBlacklist.IsTokenBlacklisted(fullToken);

            return Ok(new
            {
                debug = true,
                timestamp = DateTime.UtcNow,
                cookies = cookies,
                authHeader = bearerToken,
                authentication = new
                {
                    isAuthenticated = isAuthenticated,
                    userEmail = userEmail,
                    userId = userId,
                    tokenBlacklisted = isTokenBlacklisted,
                    claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                },
                message = "Este es un endpoint de DEBUG. Úsalo para ver qué está pasando con cookies y tokens. Si tokenBlacklisted=true, el token fue invalidado en logout."
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            _logger.LogInformation("🚪 Solicitud de logout recibida");

            // ✅ NUEVO: Extraer el token actual del request y añadirlo a la blacklist
            string? token = ExtractTokenFromRequest();
            if (!string.IsNullOrEmpty(token))
            {
                // Obtener la expiración del token para saber hasta cuándo invalidarlo
                DateTime tokenExpiration = _jwtProvider.GetTokenExpiration(token);
                _tokenBlacklist.BlacklistToken(token, tokenExpiration);
                _logger.LogInformation($"🚫 Token invalidado y añadido a blacklist hasta: {tokenExpiration:yyyy-MM-dd HH:mm:ss}");
            }
            else
            {
                _logger.LogWarning("⚠️ No se encontró token en el request de logout");
            }

            // Eliminar la cookie del navegador
            RemoveAuthCookie();

            // Agregar headers para indicar al frontend que limpie almacenamientos
            Response.Headers.Add("X-Clear-Auth", "true");
            Response.Headers.Add("Cache-Control", "no-store, no-cache, must-revalidate, proxy-revalidate");
            Response.Headers.Add("Pragma", "no-cache");
            Response.Headers.Add("Expires", "0");

            return Ok(new
            {
                success = true,
                message = "Logout exitoso",
                statusCode = 200,
                timestamp = DateTime.UtcNow,
                cleared = true
            });
        }

        /// <summary>
        /// Agrega cookie de autenticación segura (como en JAVA)
        /// </summary>
        private void AddAuthCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,  // En desarrollo. En producción: true
                SameSite = SameSiteMode.Lax,
                Path = "/",
                MaxAge = TimeSpan.FromSeconds(86400)  // 24 horas
            };

            Response.Cookies.Append("auth_token", token, cookieOptions);
            _logger.LogInformation("🍪 Cookie auth_token configurada (Secure=False, SameSite=Lax)");
        }

        /// <summary>
        /// Elimina la cookie de autenticación (como en JAVA)
        /// </summary>
        private void RemoveAuthCookie()
        {
            // IMPORTANTE: Eliminar con los MISMOS CookieOptions que se usaron al crear
            var cookieOptions = new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = false,  // Debe coincidir con AddAuthCookie
                SameSite = SameSiteMode.Lax,
                IsEssential = false
            };

            Response.Cookies.Delete("auth_token", cookieOptions);

            // Logging de la operación
            _logger.LogInformation("🚪 Cookie auth_token marcada para eliminación (Path=/,HttpOnly=True,Secure=False,SameSite=Lax)");
        }

        /// <summary>
        /// Extrae el token JWT del request actual (del header o de la cookie)
        /// </summary>
        private string? ExtractTokenFromRequest()
        {
            // Intentar obtener del header Authorization
            if (Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var authHeaderValue = authHeader.ToString();
                if (authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return authHeaderValue.Substring("Bearer ".Length).Trim();
                }
            }

            // Intentar obtener de la cookie auth_token
            if (Request.Cookies.TryGetValue("auth_token", out var token))
            {
                return token;
            }

            return null;
        }
    }
}
