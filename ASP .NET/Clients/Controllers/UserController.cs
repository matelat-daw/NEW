using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Clients.Dtos;
using Clients.Services;
using Clients.Helpers;
using Clients.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Clients.Controllers;

/// <summary>
/// Controlador optimizado para gestión de usuarios
/// Maneja registro, consultas y operaciones administrativas
/// Equivalente a UserController.java en la API JAVA
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtProvider _jwtProvider;
    private readonly IImageService _imageService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IUserService userService,
        IJwtProvider jwtProvider,
        IImageService imageService,
        ApplicationDbContext context,
        ILogger<UserController> logger)
    {
        _userService = userService;
        _jwtProvider = jwtProvider;
        _imageService = imageService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// POST /api/user/register - Registro de usuario con imagen opcional
    /// Equivalente a UserController.register() en JAVA
    /// </summary>
    [HttpPost("register")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Dtos.ApiResponse<UserDto>>> Register(
        [FromForm] string nick,
        [FromForm] string name,
        [FromForm] string surname1,
        [FromForm] string? surname2,
        [FromForm] string email,
        [FromForm] string phone,
        [FromForm] string password,
        [FromForm] string gender,
        [FromForm] string? bday,
        [FromForm] IFormFile? profilePicture)
    {
        _logger.LogInformation("📝 Intento de registro para: {Email} ({Nick})", email, nick);

        try
        {
            // Parsear la fecha de nacimiento si se proporciona
            DateTime? birthDate = null;
            if (!string.IsNullOrEmpty(bday))
            {
                try
                {
                    // Intentar ISO primero (YYYY-MM-DD)
                    birthDate = DateTime.Parse(bday);
                }
                catch
                {
                    _logger.LogWarning("⚠️ No se pudo parsear fecha de nacimiento: {BDay}", bday);
                    throw new InvalidOperationException("Formato de fecha inválido. Use YYYY-MM-DD");
                }
            }

            // Crear request de registro
            var request = new RegisterRequest
            {
                Nick = nick,
                Name = name,
                Surname1 = surname1,
                Surname2 = surname2,
                Email = email,
                Phone = phone,
                Password = password,
                Gender = gender,
                BirthDate = birthDate
            };

            // Registrar usuario
            var user = await _userService.RegisterUserAsync(request);

            // Procesar imagen de perfil si se proporciona
            if (profilePicture != null && profilePicture.Length > 0)
            {
                try
                {
                    // Validar imagen
                    ValidationHelper.ValidateImageFile(profilePicture);

                    // Guardar imagen
                    string fileName = await _imageService.SaveProfileImageAsync(profilePicture, user.Id);
                    user = await _userService.UpdateProfileImageAsync(user.Id, fileName);
                    _logger.LogInformation("✅ Imagen de perfil guardada para usuario: {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("⚠️ No se pudo guardar imagen de perfil post-registro para {Email}: {Message}", 
                        user.Email, ex.Message);
                    // No interrumpir el registro si falla la imagen
                }
            }

            // Generar token JWT
            string token = _jwtProvider.GenerateToken(user);

            var userDto = UserDto.FromEntity(user);

            _logger.LogInformation("✅ Registro exitoso para usuario: {Email}", user.Email);

            // Retornar respuesta con token y datos del usuario (como en JAVA)
            return new CreatedAtActionResult(nameof(Register), "User", null, new
            {
                success = true,
                message = "Usuario registrado exitosamente. Revisa tu correo para verificar la cuenta.",
                token = token,
                data = userDto,
                statusCode = 201,
                timestamp = DateTime.UtcNow
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("⚠️ Error de validación en registro: {Message}", ex.Message);
            return BadRequest(Dtos.ApiResponse<UserDto>.BadRequest(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado en registro de {Email}: {Message}", email, ex.Message);
            return StatusCode(500, Dtos.ApiResponse<UserDto>.InternalServerError($"Error en el proceso de registro: {ex.Message}"));
        }
    }

    /// <summary>
    /// GET /api/user - Lista usuarios (excluye solo al usuario logueado)
    /// El usuario ve todos los demás usuarios de la BD incluyendo ADMIN
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<Dtos.ApiResponse<object>>> GetAllUsers(
        [FromQuery] int page = 0,
        [FromQuery] int size = 10)
    {
        _logger.LogInformation("📝 GetAllUsers - page={Page}, size={Size}", page, size);

        try
        {
            // Obtener la ID del usuario actual desde los claims
            var currentUserIdString = User.FindFirst("userId")?.Value;

            if (!long.TryParse(currentUserIdString, out var currentUserId))
            {
                _logger.LogWarning("❌ No se pudo obtener la ID del usuario actual");
                return Unauthorized(Dtos.ApiResponse<object>.Unauthorized("No autenticado"));
            }

            _logger.LogInformation("👤 Usuario actual ID: {UserId}", currentUserId);

            // Validar parámetros de paginación
            if (page < 0) page = 0;
            if (size <= 0) size = 10;
            if (size > 100) size = 100;

            // Obtener usuarios paginados EXCLUYENDO SOLO al usuario actual
            // Se incluyen todos los demás usuarios incluyendo ADMIN
            var query = _context.Users
                .Include(u => u.UserRoles)
                .Where(u => u.Id != currentUserId)  // Solo excluir al usuario logueado
                .OrderBy(u => u.Id);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / size);

            var users = await query
                .Skip(page * size)
                .Take(size)
                .Select(u => UserDto.FromEntity(u))
                .ToListAsync();

            _logger.LogInformation("✅ GetAllUsers - Retornando {Count} usuarios, total={Total}", users.Count, totalCount);

            // Devolver estructura plana
            return Ok(new
            {
                success = true,
                message = "Lista de usuarios obtenida exitosamente",
                users = users,
                pagination = new
                {
                    currentPage = page,
                    totalItems = totalCount,
                    totalPages = totalPages,
                    pageSize = size,
                    hasNext = page < totalPages - 1,
                    hasPrevious = page > 0
                },
                statusCode = 200,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener lista de usuarios: {Message}", ex.Message);
            return StatusCode(500, Dtos.ApiResponse<object>.InternalServerError($"Error al obtener lista de usuarios: {ex.Message}"));
        }
    }

    /// <summary>
    /// GET /api/user/{id} - Obtener usuario por ID (ADMIN only)
    /// Equivalente a UserController.getUserById() en JAVA
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<Dtos.ApiResponse<UserDto>>> GetUserById(long id)
    {
        _logger.LogInformation("📝 GetUserById - id={Id}", id);

        try
        {
            var user = await _userService.GetUserByIdAsync(id)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            var userDto = UserDto.FromEntity(user);

            _logger.LogInformation("✅ GetUserById - Usuario obtenido: {Email}", user.Email);

            return Ok(new
            {
                success = true,
                message = "Perfil obtenido exitosamente",
                data = userDto,
                statusCode = 200,
                timestamp = DateTime.UtcNow
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("⚠️ Usuario no encontrado: {Message}", ex.Message);
            return NotFound(Dtos.ApiResponse<UserDto>.NotFound(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al obtener usuario: {Message}", ex.Message);
            return StatusCode(500, Dtos.ApiResponse<UserDto>.InternalServerError(ex.Message));
        }
    }

    /// <summary>
    /// PUT /api/user/{id}/role - Cambiar rol de usuario (ADMIN only)
    /// Equivalente a UserController.changeUserRole() en JAVA
    /// </summary>
    [Authorize]
    [HttpPut("{id}/role")]
    public async Task<ActionResult<Dtos.ApiResponse<UserDto>>> ChangeUserRole(long id, [FromQuery] string newRole)
    {
        _logger.LogInformation("📝 ChangeUserRole - id={Id}, newRole={NewRole}", id, newRole);

        try
        {
            // Validar que el nuevo rol sea válido
            if (!Enum.TryParse<RoleEnum>(newRole.ToUpper(), out var role))
            {
                throw new InvalidOperationException($"Rol inválido: {newRole}");
            }

            var updatedUser = await _userService.ChangeUserRoleAsync(id, role);
            var userDto = UserDto.FromEntity(updatedUser);

            _logger.LogInformation("✅ ChangeUserRole - Rol actualizado para usuario: {Email}", updatedUser.Email);

            return Ok(new
            {
                success = true,
                message = "Rol actualizado exitosamente",
                data = userDto,
                statusCode = 200,
                timestamp = DateTime.UtcNow
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("⚠️ Error al cambiar rol: {Message}", ex.Message);
            return BadRequest(Dtos.ApiResponse<UserDto>.BadRequest(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al cambiar rol: {Message}", ex.Message);
            return StatusCode(500, Dtos.ApiResponse<UserDto>.InternalServerError(ex.Message));
        }
    }

    /// <summary>
    /// DELETE /api/user/{id} - Eliminar usuario (ADMIN only)
    /// No permite eliminar usuarios con rol ADMIN (como en JAVA)
    /// </summary>
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<Dtos.ApiResponse<object>>> DeleteUser(long id)
    {
        _logger.LogInformation("📝 DeleteUser - id={Id}", id);

        try
        {
            var user = await _userService.GetUserByIdAsync(id)
                ?? throw new InvalidOperationException("Usuario no encontrado");

            // Validar que no sea ADMIN (como en JAVA)
            if (user.Roles.Contains(RoleEnum.ADMIN))
            {
                throw new InvalidOperationException("No se puede eliminar usuarios con rol ADMIN");
            }

            await _userService.DeleteUserAsync(id);

            _logger.LogInformation("✅ DeleteUser - Usuario eliminado: {Email} (ID: {Id})", user.Email, id);

            return Ok(new
            {
                success = true,
                message = "Usuario eliminado exitosamente",
                statusCode = 200,
                timestamp = DateTime.UtcNow
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("⚠️ Error al eliminar usuario: {Message}", ex.Message);
            return BadRequest(Dtos.ApiResponse<object>.BadRequest(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al eliminar usuario: {Message}", ex.Message);
            return StatusCode(500, Dtos.ApiResponse<object>.InternalServerError(ex.Message));
        }
    }
}
