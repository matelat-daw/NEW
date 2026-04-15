using Clients.Dtos;
using Clients.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Clients.Controllers;

/// <summary>
/// Controlador para la gestión del perfil del usuario
/// Maneja todas las operaciones relacionadas con el perfil: actualización, cambio de contraseña, etc.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IImageService _imageService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IUserService userService,
        IImageService imageService,
        ILogger<ProfileController> logger)
    {
        _userService = userService;
        _imageService = imageService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el perfil del usuario autenticado
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            // Log de debug para ver el estado de autenticación
            _logger.LogInformation($"📝 GetProfile - IsAuthenticated: {User.Identity?.IsAuthenticated}, AuthType: {User.Identity?.AuthenticationType}");
            _logger.LogInformation($"📝 Claims: {string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}").Take(5))}");

            // Validar autenticación (similar a Java)
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("⚠️ GetProfile - Usuario no autenticado");
                return Unauthorized(AuthResponse.Error("No autenticado"));
            }

            // Intentar obtener email de múltiples formas (algunas veces ClaimTypes.NameIdentifier no funciona)
            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.FindFirst("email")?.Value;

            _logger.LogInformation($"📧 Email extraído: {email}");

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("⚠️ GetProfile - Email no encontrado en claims");
                _logger.LogWarning($"📋 Tipos de claims: {string.Join(", ", User.Claims.Select(c => c.Type).Distinct())}");
                return Unauthorized(AuthResponse.Error("No autenticado"));
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning($"⚠️ GetProfile - Usuario no encontrado en BD: {email}");
                return NotFound(AuthResponse.Error("Usuario no encontrado"));
            }

            var userDto = UserDto.FromEntity(user);

            _logger.LogInformation($"✅ GetProfile - Perfil obtenido para usuario: {email}, ProfilePicture: {userDto.ProfilePicture}");

            return Ok(AuthResponse.Success("Perfil obtenido exitosamente", null, userDto));
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ GetProfile - Error al obtener perfil: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, AuthResponse.Error(ex.Message));
        }
    }

    /// <summary>
    /// Actualiza los datos del perfil del usuario (nombre, apellidos, teléfono)
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            // Validar autenticación (similar a Java)
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized(AuthResponse.Error("No autenticado"));
            }

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.FindFirst("email")?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(AuthResponse.Error("No autenticado"));
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(AuthResponse.Error("Usuario no encontrado"));
            }

            var updatedUser = await _userService.UpdateUserProfileAsync(
                user.Id,
                request.Name,
                request.Surname1,
                request.Surname2,
                request.Phone
            );

            var userDto = UserDto.FromEntity(updatedUser);

            _logger.LogInformation($"✅ Perfil actualizado para usuario: {email}");

            return Ok(AuthResponse.Success("Perfil actualizado exitosamente", null, userDto));
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al actualizar perfil: {ex.Message}");
            return BadRequest(AuthResponse.Error(ex.Message));
        }
    }

    /// <summary>
    /// Sube una nueva foto de perfil (MultipartFile)
    /// Retorna respuesta compatible con MyIkea-Frontend
    /// </summary>
    [HttpPost("picture")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateProfilePicture(IFormFile? profilePicture)
    {
        try
        {
            _logger.LogInformation($"🔵 UpdateProfilePicture START - IsAuthenticated: {User.Identity?.IsAuthenticated}, AuthType: {User.Identity?.AuthenticationType}");

            // Validar autenticación (similar a Java)
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("❌ UpdateProfilePicture - Usuario no autenticado (IsAuthenticated es false)");
                return Unauthorized(new { success = false, message = "No autenticado" });
            }

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.FindFirst("email")?.Value;
            _logger.LogInformation($"📧 UpdateProfilePicture - Email extraído: {email}");

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("❌ UpdateProfilePicture - Email es nulo o vacío");
                return Unauthorized(new { success = false, message = "No autenticado" });
            }

            if (profilePicture == null || profilePicture.Length == 0)
            {
                _logger.LogWarning("❌ UpdateProfilePicture - No hay imagen o está vacía");
                return BadRequest(new { success = false, message = "Por favor selecciona una imagen" });
            }

            _logger.LogInformation($"📸 UpdateProfilePicture - Archivo: {profilePicture.FileName}, Size: {profilePicture.Length}");

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning($"❌ UpdateProfilePicture - Usuario no encontrado en BD: {email}");
                return NotFound(new { success = false, message = "Usuario no encontrado" });
            }

            _logger.LogInformation($"👤 UpdateProfilePicture - Usuario encontrado: {user.Id} - {user.Email}");

            // Guardar imagen
            string fileName = await _imageService.SaveProfileImageAsync(profilePicture, user.Id);
            _logger.LogInformation($"✅ UpdateProfilePicture - Imagen guardada: {fileName}");

            var updatedUser = await _userService.UpdateProfileImageAsync(user.Id, fileName);
            _logger.LogInformation($"✅ UpdateProfilePicture - BD actualizada con imagen: {fileName}");

            var userDto = UserDto.FromEntity(updatedUser);

            _logger.LogInformation($"🖼️ Foto de perfil actualizada para usuario: {user.Id}, URL: {userDto.ProfilePicture}");

            return Ok(new
            {
                success = true,
                message = "Foto de perfil actualizada correctamente",
                data = userDto,
                statusCode = 200,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al actualizar foto de perfil: {ex.Message}\n{ex.StackTrace}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { success = false, message = $"Error al subir imagen: {ex.Message}" });
        }
    }

    /// <summary>
    /// Actualiza la foto de perfil con URL (legacy)
    /// </summary>
    [HttpPut("image")]
    [HttpPut("pic")]
    public async Task<IActionResult> UpdateProfileImage([FromBody] UpdateProfileImageRequest request)
    {
        try
        {
            // Validar autenticación (similar a Java)
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized(AuthResponse.Error("No autenticado"));
            }

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(AuthResponse.Error("No autenticado"));
            }

            if (string.IsNullOrEmpty(request.ProfileImageUrl))
            {
                return BadRequest(AuthResponse.Error("La URL de la imagen es requerida"));
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(AuthResponse.Error("Usuario no encontrado"));
            }

            var updatedUser = await _userService.UpdateProfileImageAsync(user.Id, request.ProfileImageUrl);
            var userDto = UserDto.FromEntity(updatedUser);

            _logger.LogInformation($"✅ Foto de perfil actualizada para usuario: {email}");

            return Ok(AuthResponse.Success("Foto de perfil actualizada exitosamente", null, userDto));
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al actualizar foto de perfil: {ex.Message}");
            return BadRequest(AuthResponse.Error(ex.Message));
        }
    }

    /// <summary>
    /// Cambia la contraseña del usuario
    /// </summary>
    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
    {
        try
        {
            // Validar autenticación (similar a Java)
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized(AuthResponse.Error("No autenticado"));
            }

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(AuthResponse.Error("No autenticado"));
            }

            // Validar que las nuevas contraseñas coincidan
            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(AuthResponse.Error("Las nuevas contraseñas no coinciden"));
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(AuthResponse.Error("Usuario no encontrado"));
            }

            var updatedUser = await _userService.ChangePasswordAsync(
                user.Id,
                request.CurrentPassword,
                request.NewPassword
            );

            var userDto = UserDto.FromEntity(updatedUser);

            _logger.LogInformation($"✅ Contraseña actualizada para usuario: {email}");

            return Ok(AuthResponse.Success("Contraseña actualizada exitosamente", null, userDto));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"⚠️ Error de contraseña para usuario: {ex.Message}");
            return BadRequest(AuthResponse.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al cambiar contraseña: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, AuthResponse.Error(ex.Message));
        }
    }

    /// <summary>
    /// Valida la contraseña del usuario autenticado
    /// Usado para confirmar acciones sensibles como eliminar la cuenta
    /// </summary>
    [HttpPost("validate-password")]
    public async Task<IActionResult> ValidatePassword([FromBody] Dictionary<string, string> request)
    {
        try
        {
            // Validar autenticación (similar a Java)
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized(new { success = false, message = "No autenticado" });
            }

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(new { success = false, message = "No autenticado" });
            }

            if (!request.TryGetValue("password", out var password) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new { success = false, message = "La contraseña es requerida" });
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Usuario no encontrado" });
            }

            // Validar contraseña
            bool isValid = await _userService.ValidateCredentialsAsync(email, password);

            if (isValid)
            {
                _logger.LogInformation($"✅ Contraseña validada correctamente para usuario: {email}");
                return Ok(new { success = true, message = "Contraseña correcta" });
            }
            else
            {
                _logger.LogWarning($"⚠️ Intento de validación con contraseña incorrecta para usuario: {email}");
                return Unauthorized(new { success = false, message = "Contraseña incorrecta" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al validar contraseña: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { success = false, message = $"Error al validar contraseña: {ex.Message}" });
        }
    }

    /// <summary>
    /// Elimina la cuenta del usuario vía POST
    /// Compatible con MyIkea-Frontend
    /// </summary>
    [HttpPost("delete")]
    public async Task<IActionResult> DeleteAccountPost([FromBody] ProfileDeleteRequest request)
    {
        try
        {
            // Validar autenticación (similar a Java)
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized(new { success = false, message = "No autenticado" });
            }

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(new { success = false, message = "No autenticado" });
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Usuario no encontrado" });
            }

            // Intentar eliminar imagen de perfil si no es protegida
            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                try
                {
                    if (!_imageService.IsProtectedImage(user.ProfilePicture))
                    {
                        await _imageService.DeleteImageAsync(user.ProfilePicture);
                        _logger.LogInformation($"✅ Imagen de perfil eliminada: {user.ProfilePicture}");
                    }
                    else
                    {
                        _logger.LogInformation($"⚠️ No se elimina imagen protegida para usuario: {email}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"⚠️ No se pudo eliminar imagen del usuario: {ex.Message}");
                    // Continuar con la eliminación de la cuenta
                }
            }

            // Eliminar cuenta
            await _userService.DeleteUserAccountAsync(user.Id, request.Password);

            _logger.LogInformation($"✅ Cuenta de usuario eliminada: {email}");

            return Ok(new { success = true, message = "Cuenta eliminada correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"⚠️ Error al eliminar cuenta: {ex.Message}");
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al eliminar cuenta: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { success = false, message = $"Error al eliminar cuenta: {ex.Message}" });
        }
    }

    /// <summary>
    /// Elimina la cuenta del usuario vía DELETE (REST style)
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> DeleteProfile([FromBody] Dictionary<string, string> requestBody)
    {
        try
        {
            // Validar autenticación (similar a Java)
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized(AuthResponse.Error("No autenticado"));
            }

            var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(AuthResponse.Error("No autenticado"));
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(AuthResponse.Error("Usuario no encontrado"));
            }

            if (!requestBody.TryGetValue("password", out var password) || string.IsNullOrEmpty(password))
            {
                return BadRequest(AuthResponse.Error("La contraseña es requerida para eliminar la cuenta"));
            }

            // Intentar eliminar imagen de perfil si no es protegida
            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                try
                {
                    if (!_imageService.IsProtectedImage(user.ProfilePicture))
                    {
                        await _imageService.DeleteImageAsync(user.ProfilePicture);
                        _logger.LogInformation($"✅ Imagen de perfil eliminada: {user.ProfilePicture}");
                    }
                    else
                    {
                        _logger.LogInformation($"⚠️ No se elimina imagen protegida para usuario: {email}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"⚠️ No se pudo eliminar imagen del usuario: {ex.Message}");
                    // Continuar con la eliminación de la cuenta
                }
            }

            // Eliminar cuenta
            await _userService.DeleteUserAccountAsync(user.Id, password);

            _logger.LogInformation($"✅ Cuenta de usuario eliminada: {email}");

            return Ok(AuthResponse.Success("Cuenta eliminada exitosamente", null, null));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"⚠️ Error al eliminar cuenta: {ex.Message}");
            return BadRequest(AuthResponse.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al eliminar cuenta: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, AuthResponse.Error(ex.Message));
        }
    }
}
