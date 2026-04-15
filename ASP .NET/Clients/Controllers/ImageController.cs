using Microsoft.AspNetCore.Mvc;
using System.Net;
using Clients.Services;

namespace Clients.Controllers;

/// <summary>
/// Controlador para servir imágenes subidas por usuarios
/// Maneja la descarga de imágenes de perfil y otros archivos
/// 
/// Equivalente al ImageController de Java
/// GET /api/images/{path} - Obtiene imagen por ruta relativa (ej: 1/profile.jpg)
/// </summary>
[ApiController]
[Route("api/images")]
public class ImageController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly ILogger<ImageController> _logger;
    private readonly IConfiguration _configuration;

    public ImageController(
        IImageService imageService,
        ILogger<ImageController> logger,
        IConfiguration configuration)
    {
        _imageService = imageService;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Test endpoint para verificar que los archivos estáticos funcionan
    /// GET /api/images/test
    /// </summary>
    [HttpGet("test")]
    public IActionResult TestStaticFiles()
    {
        var uploadDir = _imageService.GetUploadDirectory();
        var user3Dir = Path.Combine(uploadDir, "3");
        var testFile = Path.Combine(user3Dir, "profile.png");

        _logger.LogInformation($"📋 TEST - Upload dir: {uploadDir}");
        _logger.LogInformation($"📋 TEST - User 3 dir: {user3Dir}");
        _logger.LogInformation($"📋 TEST - Test file: {testFile}");
        _logger.LogInformation($"📋 TEST - File exists: {System.IO.File.Exists(testFile)}");

        if (System.IO.File.Exists(testFile))
        {
            var fileInfo = new System.IO.FileInfo(testFile);
            return Ok(new {
                status = "OK",
                file = testFile,
                size = fileInfo.Length,
                exists = true,
                uploadDir = uploadDir,
                accessUrl = "/uploads/3/profile.png"
            });
        }

        return Ok(new {
            status = "File not found",
            file = testFile,
            exists = false,
            uploadDir = uploadDir,
            files = Directory.Exists(user3Dir) ? Directory.GetFiles(user3Dir) : new string[] { }
        });
    }

    /// <summary>
    /// Maneja preflight CORS OPTIONS requests
    /// </summary>
    [HttpOptions("{*path}")]
    public IActionResult OptionsImage(string? path)
    {
        Response.Headers.AccessControlAllowOrigin = "*";
        Response.Headers.AccessControlAllowMethods = "GET, OPTIONS";
        Response.Headers.AccessControlAllowHeaders = "Content-Type";
        Response.Headers.AccessControlMaxAge = "3600";
        return Ok();
    }

    /// <summary>
    /// Obtiene una imagen por ruta relativa
    /// GET /api/images/1/profile.jpg
    /// GET /api/images/default/male.png
    /// GET /api/images/health para verificar el estado del servicio
    /// </summary>
    [HttpGet("{*path}")]
    public async Task<IActionResult> GetImage(string? path)
    {
        try
        {
            // Health check
            if (string.IsNullOrEmpty(path) || path == "health")
            {
                var uploadDirectory = _imageService.GetUploadDirectory();
                _logger.LogInformation($"🏥 Health check - Upload dir: {uploadDirectory}");
                return Ok(new
                {
                    status = "Image service is running",
                    upload_dir = uploadDirectory,
                    timestamp = DateTime.UtcNow
                });
            }

            // List directory
            if (path == "list/3")
            {
                var listDir = _imageService.GetUploadDirectory();
                var userDir = Path.Combine(listDir, "3");
                _logger.LogInformation($"📋 Listando archivos en: {userDir}");
                if (Directory.Exists(userDir))
                {
                    var files = Directory.GetFiles(userDir);
                    return Ok(new { directory = userDir, files = files.Select(f => Path.GetFileName(f)).ToList() });
                }
                return NotFound(new { error = $"Directorio no encontrado: {userDir}" });
            }

            _logger.LogInformation($"🖼️ Solicitud de imagen procesada. Archivo buscado: '{path}'");

            // Limpiar la ruta
            path = path?.Replace("//", "/").TrimStart('/') ?? "";

            if (string.IsNullOrEmpty(path))
            {
                _logger.LogWarning("⚠️ Ruta de imagen vacía");
                return NotFound(new { error = "Ruta de imagen vacía" });
            }

            // Validar que la ruta es segura (no path traversal)
            if (!_imageService.IsValidImagePath(path))
            {
                _logger.LogWarning($"❌ Ruta de imagen inválida: {path}");
                return BadRequest(new { error = "Ruta de imagen inválida" });
            }

            // Obtener el directorio de uploads
            var uploadDir = _imageService.GetUploadDirectory();
            var uploadPath = Path.IsPathRooted(uploadDir)
                ? uploadDir
                : Path.Combine(Directory.GetCurrentDirectory(), uploadDir);

            // Resolver la ruta completa
            var filePath = Path.Combine(uploadPath, path);
            var normalizedPath = Path.GetFullPath(filePath);

            _logger.LogInformation($"📂 Buscando imagen en: {normalizedPath}");
            _logger.LogInformation($"📂 Upload dir: {uploadPath}");
            _logger.LogInformation($"📂 Path original (antes de combine): {path}");

            // Verificar que no es path traversal
            var baseDir = Path.GetFullPath(uploadPath);
            if (!normalizedPath.StartsWith(baseDir + Path.DirectorySeparatorChar) && normalizedPath != baseDir)
            {
                _logger.LogWarning($"❌ Intento de path traversal detectado: {path} | Base: {baseDir} | Normalized: {normalizedPath}");
                return BadRequest(new { error = "Ruta inválida" });
            }

            // Verificar que el archivo existe y es legible
            if (!System.IO.File.Exists(normalizedPath))
            {
                _logger.LogWarning($"❌ Imagen no encontrada: {path} | Full path: {normalizedPath}");
                var parentDir = Path.GetDirectoryName(normalizedPath);
                if (Directory.Exists(parentDir ?? ""))
                {
                    var filesInDir = Directory.GetFiles(parentDir ?? "");
                    _logger.LogWarning($"❌ Archivos en directorio ({parentDir}): {string.Join(", ", filesInDir)}");
                }
                return NotFound(new { error = $"Imagen no encontrada: {path}" });
            }

            _logger.LogInformation($"✅ Imagen encontrada: {path}");

            // Leer el archivo
            var fileBytes = await System.IO.File.ReadAllBytesAsync(normalizedPath);

            // Determinar el content type
            var contentType = GetContentType(normalizedPath);

            _logger.LogInformation($"✅ Sirviendo recurso: {Path.GetFileName(normalizedPath)} ({contentType})");

            // Añadir headers CORS explícitos
            Response.Headers.AccessControlAllowOrigin = "*";
            Response.Headers.AccessControlAllowMethods = "GET, OPTIONS";
            Response.Headers.AccessControlAllowHeaders = "Content-Type";

            // Servir el archivo
            return File(fileBytes, contentType, Path.GetFileName(normalizedPath), enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error al obtener imagen: {path} - {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = $"Error al obtener la imagen: {ex.Message}" });
        }
    }

    /// <summary>
    /// Determina el content type basado en la extensión del archivo
    /// </summary>
    private string GetContentType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };
    }
}
