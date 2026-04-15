using Clients.Dtos;
using Clients.Entities;
using Clients.Enums;
using Microsoft.EntityFrameworkCore;

namespace Clients.Services;

public interface IUserService
{
    Task<User> RegisterUserAsync(RegisterRequest request);
    Task<User?> GetUserByIdAsync(long id);
    Task<User?> GetUserByNickAsync(string nick);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> UpdateProfileImageAsync(long userId, string fileName);
    Task DeleteUserAsync(long id);
    Task<User> ChangeUserRoleAsync(long userId, RoleEnum newRole);
    Task<bool> ValidateCredentialsAsync(string email, string password);
    Task VerifyEmailAsync(string token);
    Task<User> UpdateUserProfileAsync(long userId, string? name, string? surname1, string? surname2, string? phone);
    Task<User> ChangePasswordAsync(long userId, string currentPassword, string newPassword);
    Task DeleteUserAccountAsync(long userId, string password);
    Task<User> VerifyEmailWithTokenAsync(string token);
    bool IsProtectedImage(string imagePath);
    string GetDefaultImageForGender(Gender gender);
    string GetUserRole(long userId);
    bool UserHasRole(long userId, RoleEnum role);
    bool IsAdmin(long userId);
    bool IsPremiumOrAdmin(long userId);
}

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly IImageService _imageService;
    private readonly IEmailService _emailService;

    public UserService(
        ApplicationDbContext context,
        ILogger<UserService> logger,
        IImageService imageService,
        IEmailService? emailService = null)
    {
        _context = context;
        _logger = logger;
        _imageService = imageService;
        _emailService = emailService;
    }

    public async Task<User> RegisterUserAsync(RegisterRequest request)
    {
        _logger.LogInformation($"📝 Iniciando registro de usuario: {request.Email}");

        // Validar que el email no exista
        var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingEmail != null)
            throw new InvalidOperationException("El correo electrónico ya está registrado");

        // Validar que el nick no exista
        var existingNick = await _context.Users.FirstOrDefaultAsync(u => u.Nick == request.Nick);
        if (existingNick != null)
            throw new InvalidOperationException("El nick ya está en uso");

        // Convertir gender string a Gender enum
        Gender gender = GenderExtensions.FromString(request.Gender);

        // Asignar imagen por defecto si no se proporciona
        string profileImg = request.ProfilePicture ?? gender.GetDefaultImagePath();

        // Generar verification token
        string verificationToken = Guid.NewGuid().ToString();
        DateTime verificationTokenExpiry = DateTime.UtcNow.AddHours(24);

        var user = new User
        {
            Nick = request.Nick,
            Name = request.Name,
            Surname1 = request.Surname1,
            Surname2 = request.Surname2,
            Email = request.Email,
            Phone = request.Phone,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Gender = gender.ToString(),
            BirthDate = request.BirthDate,
            ProfilePicture = profileImg,
            Active = 0,
            EmailVerified = 0,
            VerificationToken = verificationToken,
            VerificationTokenExpiry = verificationTokenExpiry,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assign USER role to the new user
        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = (int)RoleEnum.USER,
            AssignedAt = DateTime.UtcNow
        };
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Usuario registrado con ID: {user.Id}");

        // Enviar email de verificación
        try
        {
            if (_emailService != null)
            {
                await _emailService.SendVerificationEmailAsync(
                    user.Email,
                    user.Name,
                    verificationToken
                );
            }
            else
            {
                _logger.LogWarning($"⚠️ EmailService no disponible. Email de verificación NO enviado para {user.Email}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"⚠️ Error al enviar email de verificación a {user.Email}: {ex.Message}");
            // No interrumpir el registro si falla el email
        }

        return user;
    }

    public async Task<User?> GetUserByIdAsync(long id)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetUserByNickAsync(string nick)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Nick == nick);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> UpdateProfileImageAsync(long userId, string fileName)
    {
        var user = await GetUserByIdAsync(userId)
            ?? throw new InvalidOperationException("Usuario no encontrado");

        user.ProfilePicture = fileName;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Imagen de perfil actualizada para usuario {userId}: {fileName}");
        return user;
    }

    public async Task DeleteUserAsync(long id)
    {
        var user = await GetUserByIdAsync(id)
            ?? throw new InvalidOperationException("Usuario no encontrado");

        if (user.IsAdmin)
            throw new InvalidOperationException("No se puede eliminar usuarios con rol ADMIN");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Usuario eliminado: {user.Email} (ID: {id})");
    }

    public async Task<User> ChangeUserRoleAsync(long userId, RoleEnum newRole)
    {
        var user = await GetUserByIdAsync(userId)
            ?? throw new InvalidOperationException("Usuario no encontrado");

        // Remove all existing roles for this user
        var existingRoles = _context.UserRoles.Where(ur => ur.UserId == userId);
        _context.UserRoles.RemoveRange(existingRoles);

        // Assign the new role
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = (int)newRole,
            AssignedAt = DateTime.UtcNow
        };
        _context.UserRoles.Add(userRole);

        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Rol de usuario {userId} actualizado a {newRole}");
        return user;
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password)
    {
        var user = await GetUserByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning($"⚠️ Intento de login con email no registrado: {email}");
            return false;
        }

        // Validar que el usuario esté activo y verificado (como en JAVA)
        if (user.Active == 0 || user.EmailVerified == 0)
        {
            _logger.LogWarning($"⚠️ Usuario no verificado o inactivo: {email}");
            throw new InvalidOperationException("Usuario no verificado o inactivo");
        }

        bool isValid = BCrypt.Net.BCrypt.Verify(password, user.Password);

        if (isValid)
        {
            _logger.LogInformation($"✅ Credenciales válidas para: {email}");
        }
        else
        {
            _logger.LogWarning($"⚠️ Contraseña incorrecta para: {email}");
        }

        return isValid;
    }

    public async Task VerifyEmailAsync(string token)
    {
        // Buscar el usuario por token de verificación (como en JAVA)
        var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);

        if (user == null)
        {
            throw new InvalidOperationException("Token de verificación inválido");
        }

        // Verificar que el token no haya expirado (como en JAVA)
        if (user.VerificationTokenExpiry.HasValue && user.VerificationTokenExpiry.Value < DateTime.UtcNow)
        {
            throw new InvalidOperationException("El token de verificación ha expirado");
        }

        // Marcar como verificado (como en JAVA)
        user.EmailVerified = 1;
        user.Active = 1;
        user.VerificationToken = null;
        user.VerificationTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Email verificado para usuario: {user.Email}");

        // Enviar email de bienvenida (como en JAVA)
        try
        {
            if (_emailService != null)
            {
                await _emailService.SendWelcomeEmailAsync(user.Email, user.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"⚠️ Error al enviar email de bienvenida a {user.Email}: {ex.Message}");
        }
    }

    public async Task<User> UpdateUserProfileAsync(long userId, string? name, string? surname1, string? surname2, string? phone)
    {
        var user = await GetUserByIdAsync(userId)
            ?? throw new InvalidOperationException("Usuario no encontrado");

        if (!string.IsNullOrEmpty(name))
            user.Name = name;
        if (!string.IsNullOrEmpty(surname1))
            user.Surname1 = surname1;
        if (!string.IsNullOrEmpty(surname2))
            user.Surname2 = surname2;
        if (!string.IsNullOrEmpty(phone))
            user.Phone = phone;

        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Perfil del usuario {userId} actualizado");
        return user;
    }

    public async Task<User> ChangePasswordAsync(long userId, string currentPassword, string newPassword)
    {
        var user = await GetUserByIdAsync(userId)
            ?? throw new InvalidOperationException("Usuario no encontrado");

        // Verificar que la contraseña actual sea correcta
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
        {
            _logger.LogWarning($"⚠️ Intento de cambio de contraseña con contraseña incorrecta para usuario: {userId}");
            throw new InvalidOperationException("La contraseña actual es incorrecta");
        }

        // Cambiar a nueva contraseña
        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Contraseña actualizada para usuario: {userId}");
        return user;
    }

    public async Task DeleteUserAccountAsync(long userId, string password)
    {
        var user = await GetUserByIdAsync(userId)
            ?? throw new InvalidOperationException("Usuario no encontrado");

        // Verificar contraseña
        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            _logger.LogWarning($"⚠️ Intento de eliminación de cuenta con contraseña incorrecta para usuario: {userId}");
            throw new InvalidOperationException("Contraseña incorrecta. No se puede eliminar la cuenta.");
        }

        // Intentar eliminar imágenes del usuario
        if (!string.IsNullOrEmpty(user.ProfilePicture) && !IsProtectedImage(user.ProfilePicture))
        {
            try
            {
                await _imageService.DeleteUserProfileImagesAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"⚠️ Error al eliminar imágenes del usuario: {ex.Message}");
                // Continuar con la eliminación de la cuenta
            }
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Cuenta de usuario eliminada: {user.Email} (ID: {userId})");
    }

    /// <summary>
    /// Verifica el email del usuario usando el token de verificación
    /// Marca la cuenta como verificada y activa
    /// </summary>
    public async Task<User> VerifyEmailWithTokenAsync(string token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);

        if (user == null)
            throw new InvalidOperationException("Token de verificación inválido");

        // Verificar que el token no haya expirado
        if (user.VerificationTokenExpiry.HasValue && user.VerificationTokenExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("El token de verificación ha expirado");

        // Marcar como verificado
        user.EmailVerified = 1;
        user.Active = 1;
        user.VerificationToken = null;
        user.VerificationTokenExpiry = null;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Email verificado para usuario: {user.Email}");

        // Enviar email de bienvenida
        try
        {
            if (_emailService != null)
            {
                await _emailService.SendWelcomeEmailAsync(user.Email, user.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"⚠️ Error al enviar email de bienvenida: {ex.Message}");
        }

        return user;
    }

    /// <summary>
    /// Verifica si una imagen es protegida (imagen por defecto)
    /// </summary>
    public bool IsProtectedImage(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return false;

        return GenderExtensions.IsDefaultImagePath(imagePath);
    }

    /// <summary>
    /// Obtiene la imagen por defecto para un género
    /// </summary>
    public string GetDefaultImageForGender(Gender gender)
    {
        return gender.GetDefaultImagePath();
    }

    /// <summary>
    /// Obtiene el rol del usuario como string
    /// </summary>
    public string GetUserRole(long userId)
    {
        var user = _context.Users.Include(u => u.UserRoles).FirstOrDefault(u => u.Id == userId);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado");

        return user.PrimaryRole.ToString();
    }

    /// <summary>
    /// Verifica si un usuario tiene un rol específico
    /// </summary>
    public bool UserHasRole(long userId, RoleEnum role)
    {
        var user = _context.Users.Include(u => u.UserRoles).FirstOrDefault(u => u.Id == userId);
        return user != null && user.HasRole(role);
    }

    /// <summary>
    /// Verifica si un usuario es administrador
    /// </summary>
    public bool IsAdmin(long userId)
    {
        return UserHasRole(userId, RoleEnum.ADMIN);
    }

    /// <summary>
    /// Verifica si un usuario tiene rol premium o administrador
    /// </summary>
    public bool IsPremiumOrAdmin(long userId)
    {
        var user = _context.Users.Include(u => u.UserRoles).FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return false;

        return user.IsPremiumOrAdmin;
    }
}

