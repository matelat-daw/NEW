namespace Clients.Services;

using Clients.Enums;

public interface IImageService
{
    Task<string> SaveProfileImageAsync(IFormFile file, long userId);
    Task DeleteProfileImageAsync(string? fileName);
    string GetUploadDirectory();
    bool IsProtectedImage(string fileName);
    Task DeleteImageAsync(string fileName);
    Task DeleteUserProfileImagesAsync(long userId);
    bool IsValidImagePath(string imagePath);
    bool ImageExists(string imagePath);
    string GetImageFileName(string imagePath);
    string GenerateSecureImagePath(string fileName, long userId);
    bool IsValidImageType(string? contentType);
    Task<string> SaveProfileImageAsync(IFormFile file, long userId, Gender gender);
}

public class ImageService : IImageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ImageService> _logger;

    public ImageService(IConfiguration configuration, ILogger<ImageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Convierte una ruta potencialmente relativa a una ruta absoluta
    /// </summary>
    private string GetAbsolutePath(string? path)
    {
        if (string.IsNullOrEmpty(path))
            path = "uploads";

        // Si ya es una ruta absoluta, devolverla tal cual
        if (Path.IsPathRooted(path))
            return path;

        // Si es relativa, combinarla con el directorio actual
        return Path.Combine(Directory.GetCurrentDirectory(), path);
    }

    /// <summary>
    /// Guarda una imagen de perfil con nombre fijo "profile" para un usuario
    /// </summary>
    public async Task<string> SaveProfileImageAsync(IFormFile file, long userId)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("El archivo de imagen está vacío");

        // Validar tipo de archivo
        if (!IsValidImageType(file.ContentType))
            throw new ArgumentException("Tipo de archivo no permitido. Solo se permiten imágenes (jpg, png, gif, webp)");

        var configuredUploadDir = _configuration["FileUpload:UploadDirectory"];
        var uploadDir = GetAbsolutePath(configuredUploadDir);

        try
        {
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
                _logger.LogInformation($"📁 Carpeta de uploads creada: {uploadDir}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al crear directorio de uploads: {ex.Message}");
            throw new InvalidOperationException($"No se puede crear la carpeta de uploads: {ex.Message}", ex);
        }

        try
        {
            // Obtener extensión original
            string originalFilename = file.FileName;
            string extension = "jpg"; // default
            if (!string.IsNullOrEmpty(originalFilename) && originalFilename.Contains("."))
            {
                extension = originalFilename.Substring(originalFilename.LastIndexOf(".") + 1).ToLower();
            }

            // Crear directorio del usuario si no existe
            string userDir = Path.Combine(uploadDir, userId.ToString());
            if (!Directory.Exists(userDir))
            {
                try
                {
                    Directory.CreateDirectory(userDir);
                    _logger.LogInformation($"📁 Carpeta de usuario creada: {userDir}");
                }
                catch (Exception dirEx)
                {
                    _logger.LogError($"❌ Error al crear carpeta del usuario: {dirEx.Message}");
                    throw new InvalidOperationException($"No se puede crear la carpeta del usuario: {dirEx.Message}", dirEx);
                }
            }

            // El nombre siempre será "profile" con su extensión original
            string fileName = $"profile.{extension}";
            string filePath = Path.Combine(userDir, fileName);

            // Guardar archivo (sobrescribirá si ya existe)
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                _logger.LogInformation($"📸 Imagen guardada exitosamente en: {filePath}");
            }
            catch (Exception fileEx)
            {
                _logger.LogError($"❌ Error al guardar archivo: {fileEx.Message}");
                throw new InvalidOperationException($"No se puede guardar la imagen: {fileEx.Message}", fileEx);
            }

            // La ruta que guardamos en BD será relativa a uploadDir: "ID/profile.ext"
            string dbPath = $"{userId}/{fileName}";
            _logger.LogInformation($"📸 Imagen de perfil guardada para usuario {userId}: {dbPath}");
            return dbPath;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al guardar imagen para usuario {userId}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Guarda una imagen de perfil asignando automáticamente la imagen por defecto si no se proporciona
    /// </summary>
    public async Task<string> SaveProfileImageAsync(IFormFile file, long userId, Gender gender)
    {
        if (file == null || file.Length == 0)
        {
            // Retornar imagen por defecto
            string defaultImage = gender.GetDefaultImagePath();
            _logger.LogInformation($"📸 Asignando imagen por defecto para usuario {userId}: {defaultImage}");
            return defaultImage;
        }

        return await SaveProfileImageAsync(file, userId);
    }

    public async Task DeleteProfileImageAsync(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return;

        var configuredUploadDir = _configuration["FileUpload:UploadDirectory"];
        var uploadDir = GetAbsolutePath(configuredUploadDir);
        var filePath = Path.Combine(uploadDir, fileName);

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation($"✅ Imagen eliminada: {fileName}");
            }
            else
            {
                _logger.LogWarning($"⚠️ Archivo no encontrado para eliminar: {filePath}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al eliminar imagen: {ex.Message}");
        }
    }

    public string GetUploadDirectory()
    {
        var configuredUploadDir = _configuration["FileUpload:UploadDirectory"];
        return GetAbsolutePath(configuredUploadDir);
    }

    public bool IsProtectedImage(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        return GenderExtensions.IsDefaultImagePath(fileName);
    }

    public async Task DeleteImageAsync(string fileName)
    {
        // No eliminar imágenes protegidas
        if (IsProtectedImage(fileName))
        {
            _logger.LogWarning($"⚠️ Intento de eliminar imagen protegida: {fileName}");
            throw new InvalidOperationException("No se pueden eliminar imágenes por defecto");
        }

        var configuredUploadDir = _configuration["FileUpload:UploadDirectory"];
        var uploadDir = GetAbsolutePath(configuredUploadDir);
        var filePath = Path.Combine(uploadDir, fileName);

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation($"✅ Imagen eliminada: {fileName}");
            }
            else
            {
                _logger.LogWarning($"⚠️ Imagen no encontrada: {filePath}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al eliminar imagen: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Elimina todos los archivos de perfil de un usuario (carpeta completa)
    /// Se usa cuando se elimina la cuenta del usuario
    /// </summary>
    public async Task DeleteUserProfileImagesAsync(long userId)
    {
        try
        {
            var configuredUploadDir = _configuration["FileUpload:UploadDirectory"];
            var uploadDir = GetAbsolutePath(configuredUploadDir);
            string userPath = Path.Combine(uploadDir, userId.ToString());

            if (!Directory.Exists(userPath))
            {
                _logger.LogWarning($"📁 Carpeta de usuario no encontrada: {userPath}");
                return;
            }

            // Eliminar recursivamente toda la carpeta
            var files = Directory.GetFiles(userPath);
            foreach (var file in files)
            {
                File.Delete(file);
                _logger.LogInformation($"✅ Archivo eliminado: {file}");
            }

            // Eliminar carpeta
            Directory.Delete(userPath);
            _logger.LogInformation($"✅ Carpeta de imágenes del usuario {userId} eliminada completamente");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al eliminar carpeta de imágenes del usuario {userId}: {ex.Message}");
            // No lanzar excepción para no interrumpir la eliminación del usuario
        }
    }

    public bool IsValidImagePath(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return false;

        // Evitar path traversal
        return !imagePath.Contains("..") && !imagePath.Contains("//") &&
               System.Text.RegularExpressions.Regex.IsMatch(imagePath, @"^[a-zA-Z0-9/_.-]+$");
    }

    public bool ImageExists(string? imagePath)
    {
        try
        {
            if (string.IsNullOrEmpty(imagePath))
                return false;

            var configuredUploadDir = _configuration["FileUpload:UploadDirectory"];
            var uploadDir = GetAbsolutePath(configuredUploadDir);
            string filePath = Path.Combine(uploadDir, imagePath);
            return File.Exists(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al verificar imagen: {ex.Message}");
            return false;
        }
    }

    public string GetImageFileName(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return "";

        return imagePath.Substring(imagePath.LastIndexOf("/") + 1);
    }

    public string GenerateSecureImagePath(string fileName, long userId)
    {
        // Validar nombre de archivo
        if (!System.Text.RegularExpressions.Regex.IsMatch(fileName, @"^[a-zA-Z0-9._-]+$"))
            throw new ArgumentException("Nombre de archivo inválido");

        // Crear ruta con timestamp para evitar colisiones
        string userImageDir = $"users/{userId}/";
        return userImageDir + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "_" + fileName;
    }

    public bool IsValidImageType(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        return contentType switch
        {
            "image/jpeg" => true,
            "image/png" => true,
            "image/gif" => true,
            "image/webp" => true,
            _ => false
        };
    }
}
