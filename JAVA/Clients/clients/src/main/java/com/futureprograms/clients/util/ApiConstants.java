package com.futureprograms.clients.util;

/**
 * Constantes centralizadas para la API
 * Evita strings duplicados y facilita mantenimiento
 */
public final class ApiConstants {

    // ==================== MENSAJES DE ÉXITO ====================
    public static final String MSG_LOGIN_SUCCESS = "Login exitoso";
    public static final String MSG_REGISTER_SUCCESS = "Usuario registrado exitosamente. Revisa tu correo para verificar la cuenta.";
    public static final String MSG_PROFILE_UPDATED = "Perfil actualizado exitosamente";
    public static final String MSG_PASSWORD_UPDATED = "Contraseña actualizada exitosamente";
    public static final String MSG_PROFILE_PICTURE_UPDATED = "Foto de perfil actualizada exitosamente";
    public static final String MSG_PROFILE_DELETED = "Perfil eliminado exitosamente";
    public static final String MSG_EMAIL_VERIFIED = "Email verificado con éxito";
    public static final String MSG_TOKEN_REFRESHED = "Token refrescado exitosamente";
    public static final String MSG_PROFILE_FETCHED = "Perfil obtenido exitosamente";
    public static final String MSG_USERS_FETCHED = "Lista de usuarios obtenida exitosamente";

    // ==================== MENSAJES DE ERROR ====================
    public static final String ERR_INVALID_CREDENTIALS = "Email o contraseña incorrectos";
    public static final String ERR_UNAUTHORIZED = "No autenticado";
    public static final String ERR_FORBIDDEN = "Acceso denegado";
    public static final String ERR_USER_NOT_FOUND = "Usuario no encontrado";
    public static final String ERR_USER_NOT_ACTIVE = "Usuario no verificado o inactivo";
    public static final String ERR_EMAIL_EXISTS = "El email ya está registrado";
    public static final String ERR_NICK_EXISTS = "El nick ya está en uso";
    public static final String ERR_INVALID_TOKEN = "Token inválido o expirado";
    public static final String ERR_VERIFICATION_FAILED = "Verificación fallida";
    public static final String ERR_NO_IMAGE = "Por favor selecciona una imagen";
    public static final String ERR_IMAGE_TYPE_INVALID = "Tipo de archivo no permitido. Solo se permiten imágenes (jpg, png, gif, webp)";
    public static final String ERR_IMAGE_SAVE_FAILED = "Error al guardar imagen";
    public static final String ERR_IMAGE_DELETE_FAILED = "Error al eliminar imagen";
    public static final String ERR_INTERNAL_ERROR = "Error interno del servidor";
    public static final String ERR_VALIDATION_ERROR = "Error de validación";

    // ==================== RUTAS Y PATHS ====================
    public static final String API_PREFIX = "/api";
    public static final String AUTH_ENDPOINT = API_PREFIX + "/auth";
    public static final String USER_ENDPOINT = API_PREFIX + "/user";
    public static final String PROFILE_ENDPOINT = API_PREFIX + "/profile";
    public static final String IMAGES_ENDPOINT = API_PREFIX + "/images";

    // ==================== CONFIGURACIÓN JWT ====================
    public static final String JWT_COOKIE_NAME = "auth_token";
    public static final String JWT_COOKIE_PATH = "/";
    public static final int JWT_COOKIE_MAX_AGE = 86400; // 24 horas
    public static final String JWT_SAME_SITE = "Lax";

    // ==================== CONFIGURACIÓN ROLES ====================
    public static final String ROLE_PREFIX = "ROLE_";
    public static final String ROLE_ADMIN = "ADMIN";
    public static final String ROLE_USER = "USER";
    public static final String ROLE_PREMIUM = "PREMIUM";

    // ==================== REGEXP Y VALIDACIONES ====================
    public static final String FILENAME_PATTERN = "^[a-zA-Z0-9._-]+$";
    public static final String IMAGE_PATH_PATTERN = "^[a-zA-Z0-9/_.-]+$";

    // ==================== ERRORES HTTP ====================
    public static final String ERR_METHOD_NOT_ALLOWED = "Método HTTP no permitido";
    public static final String ERR_NOT_FOUND = "Recurso no encontrado";

    private ApiConstants() {
        throw new AssertionError("No se puede instanciar esta clase");
    }
}
