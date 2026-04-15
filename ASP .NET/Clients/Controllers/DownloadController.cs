using Microsoft.AspNetCore.Mvc;
using Clients.Services;

namespace Clients.Controllers;

/// <summary>
/// Controlador para descargar/servir archivos directamente
/// Proporciona una alternativa a UseStaticFiles() con control total sobre headers
/// </summary>
[ApiController]
[Route("download")]
public class DownloadController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly ILogger<DownloadController> _logger;

    public DownloadController(IImageService imageService, ILogger<DownloadController> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    /// <summary>
    /// Descarga un archivo de imagen del servidor
    /// GET /download/uploads/3/profile.png
    /// </summary>
    [HttpGet("{*path}")]
    public async Task<IActionResult> Download(string? path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                _logger.LogWarning("⚠️ Download - Path vacío");
                return BadRequest(new { error = "Path es requerido" });
            }

            _logger.LogInformation($"📥 Download request: {path}");

            // Validar que no sea path traversal
            if (!_imageService.IsValidImagePath(path))
            {
                _logger.LogWarning($"❌ Download - Path inválido: {path}");
                return BadRequest(new { error = "Path inválido" });
            }

            // Obtener el directorio de uploads
            var uploadDir = _imageService.GetUploadDirectory();
            var uploadPath = Path.IsPathRooted(uploadDir)
                ? uploadDir
                : Path.Combine(Directory.GetCurrentDirectory(), uploadDir);

            // Resolver la ruta completa
            var filePath = Path.Combine(uploadPath, path);
            var normalizedPath = Path.GetFullPath(filePath);

            // Verificar que no es path traversal
            var baseDir = Path.GetFullPath(uploadPath);
            if (!normalizedPath.StartsWith(baseDir + Path.DirectorySeparatorChar) && normalizedPath != baseDir)
            {
                _logger.LogWarning($"❌ Download - Path traversal detectado: {path}");
                return BadRequest(new { error = "Path inválido" });
            }

            // Verificar que el archivo existe
            if (!System.IO.File.Exists(normalizedPath))
            {
                _logger.LogWarning($"❌ Download - Archivo no encontrado: {path} | {normalizedPath}");
                return NotFound(new { error = $"Archivo no encontrado: {path}" });
            }

            _logger.LogInformation($"✅ Download - Sirviendo: {normalizedPath}");

            // Leer el archivo
            var fileBytes = await System.IO.File.ReadAllBytesAsync(normalizedPath);

            // Determinar el content type
            var contentType = GetContentType(normalizedPath);

            // Agregar headers CORS
            Response.Headers.AccessControlAllowOrigin = "*";
            Response.Headers.AccessControlAllowMethods = "GET, OPTIONS";
            Response.Headers.AccessControlAllowHeaders = "Content-Type";
            Response.Headers.CacheControl = "public, max-age=0, no-cache";

            return File(fileBytes, contentType, Path.GetFileName(normalizedPath));
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Download error: {path} - {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Error al descargar el archivo" });
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
