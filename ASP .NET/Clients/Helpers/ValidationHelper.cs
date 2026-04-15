namespace Clients.Helpers;

/// <summary>
/// Utilidad de validación centralizada
/// Sigue el principio DRY (Don't Repeat Yourself)
/// Equivalente a ValidationHelper.java en la API JAVA
/// </summary>
public static class ValidationHelper
{
    private const long MaxImageSize = 20 * 1024 * 1024; // 20MB

    /// <summary>
    /// Valida que un archivo de imagen sea válido
    /// </summary>
    public static void ValidateImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new InvalidOperationException("Por favor selecciona una imagen");
        }

        if (file.Length > MaxImageSize)
        {
            throw new InvalidOperationException("El archivo excede el tamaño máximo permitido (20MB)");
        }

        if (!IsValidImageType(file.ContentType))
        {
            throw new InvalidOperationException("Tipo de archivo no permitido. Solo se permiten imágenes (jpg, png, gif, webp)");
        }
    }

    /// <summary>
    /// Valida que el tipo de contenido sea una imagen válida
    /// </summary>
    public static bool IsValidImageType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            return false;
        }

        return contentType == "image/jpeg"
            || contentType == "image/png"
            || contentType == "image/gif"
            || contentType == "image/webp";
    }

    /// <summary>
    /// Valida que un string sea válido (no null ni vacío)
    /// </summary>
    public static bool IsValidString(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Valida que una ruta de archivo sea segura (sin path traversal)
    /// </summary>
    public static bool IsValidFilePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        return !path.Contains("..")
            && !path.Contains("//")
            && System.Text.RegularExpressions.Regex.IsMatch(path, @"^[a-zA-Z0-9/_.-]+$");
    }

    /// <summary>
    /// Extrae la extensión de un archivo
    /// </summary>
    public static string GetFileExtension(string? filename)
    {
        if (string.IsNullOrEmpty(filename) || !filename.Contains("."))
        {
            return "jpg";
        }

        return filename.Substring(filename.LastIndexOf(".") + 1).ToLowerInvariant();
    }

    /// <summary>
    /// Valida email básico
    /// </summary>
    public static bool IsValidEmail(string? email)
    {
        if (!IsValidString(email))
        {
            return false;
        }

        return System.Text.RegularExpressions.Regex.IsMatch(
            email!,
            @"^[A-Za-z0-9+_.-]+@[A-Za-z0-9.-]+$");
    }

    /// <summary>
    /// Valida que una contraseña sea segura (mínimo 8 caracteres)
    /// </summary>
    public static bool IsValidPassword(string? password)
    {
        return IsValidString(password) && password!.Length >= 8;
    }
}
