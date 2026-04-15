namespace Clients.Constants;

/// <summary>
/// Constantes centralizadas para la API
/// Evita strings duplicados y facilita mantenimiento
/// Equivalente a ApiConstants.java en la API JAVA
/// </summary>
public static class ApiConstants
{
    // ==================== MENSAJES DE ÉXITO ====================
    public const string MessageLoginSuccess = "Login exitoso";
    public const string MessageRegisterSuccess = "Usuario registrado exitosamente. Revisa tu correo para verificar la cuenta.";
    public const string MessageProfileUpdated = "Perfil actualizado exitosamente";
    public const string MessagePasswordUpdated = "Contraseña actualizada exitosamente";
    public const string MessageProfilePictureUpdated = "Foto de perfil actualizada exitosamente";
    public const string MessageProfileDeleted = "Perfil eliminado exitosamente";
    public const string MessageEmailVerified = "Email verificado con éxito";
    public const string MessageTokenRefreshed = "Token refrescado exitosamente";
    public const string MessageProfileFetched = "Perfil obtenido exitosamente";
    public const string MessageUsersFetched = "Lista de usuarios obtenida exitosamente";

    // ==================== MENSAJES DE ERROR ====================
    public const string ErrorInvalidCredentials = "Email o contraseña incorrectos";
    public const string ErrorUnauthorized = "No autenticado";
    public const string ErrorForbidden = "Acceso denegado";
    public const string ErrorUserNotFound = "Usuario no encontrado";
    public const string ErrorUserNotActive = "Usuario no verificado o inactivo";
    public const string ErrorEmailExists = "El email ya está registrado";
    public const string ErrorNickExists = "El nick ya está en uso";
    public const string ErrorInvalidToken = "Token inválido o expirado";
    public const string ErrorVerificationFailed = "Verificación fallida";
    public const string ErrorNoImage = "Por favor selecciona una imagen";
    public const string ErrorImageTypeInvalid = "Tipo de archivo no permitido. Solo se permiten imágenes (jpg, png, gif, webp)";
    public const string ErrorImageSaveFailed = "Error al guardar imagen";
    public const string ErrorImageDeleteFailed = "Error al eliminar imagen";
    public const string ErrorInternalError = "Error interno del servidor";
    public const string ErrorValidationError = "Error de validación";

    // ==================== RUTAS Y PATHS ====================
    public const string ApiPrefix = "/api";
    public const string AuthEndpoint = ApiPrefix + "/auth";
    public const string UserEndpoint = ApiPrefix + "/user";
    public const string ProfileEndpoint = ApiPrefix + "/profile";
    public const string ImagesEndpoint = ApiPrefix + "/images";

    // ==================== CONFIGURACIÓN JWT ====================
    public const string JwtCookieName = "auth_token";
    public const string JwtCookiePath = "/";
    public const int JwtCookieMaxAge = 86400; // 24 horas
    public const string JwtSameSite = "Lax";

    // ==================== CONFIGURACIÓN ROLES ====================
    public const string RolePrefix = "ROLE_";
    public const string RoleAdmin = "ADMIN";
    public const string RoleUser = "USER";
    public const string RolePremium = "PREMIUM";

    // ==================== REGEXP Y VALIDACIONES ====================
    public const string FilenamePattern = @"^[a-zA-Z0-9._-]+$";
    public const string ImagePathPattern = @"^[a-zA-Z0-9/_.-]+$";

    // ==================== ERRORES HTTP ====================
    public const string ErrorMethodNotAllowed = "Método HTTP no permitido";
    public const string ErrorNotFound = "Recurso no encontrado";
}
