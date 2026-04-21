<?php

namespace FuturePrograms\Clients\Utils;

use Exception;
use InvalidArgumentException;
use RuntimeException;

final class ImageHelper
{
    private string $baseDir;
    private int $maxFileSize;
    private array $allowedMimes;
    private array $mimeToExt;
    private string $urlBasePath;
    private \finfo $finfo;

    public function __construct(
        string $baseDir,
        int $maxFileSize = 20 * 1024 * 1024,
        ?array $allowedMimes = null,
        string $urlBasePath = '/api/images'
    ) {
        $realBase = realpath($baseDir);
        if ($realBase === false) {
            throw new InvalidArgumentException("Base directory does not exist: {$baseDir}");
        }

        $this->baseDir       = rtrim($realBase, DIRECTORY_SEPARATOR);
        $this->maxFileSize   = $maxFileSize;
        $this->allowedMimes  = $allowedMimes ?? ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
        $this->mimeToExt     = [
            'image/jpeg' => 'jpg',
            'image/png'  => 'png',
            'image/gif'  => 'gif',
            'image/webp' => 'webp',
        ];
        $this->urlBasePath   = $urlBasePath;
        $this->finfo         = finfo_open(FILEINFO_MIME_TYPE);

        if ($this->finfo === false) {
            throw new RuntimeException("Failed to initialize finfo extension");
        }
    }

    public function __destruct()
    {
        if (is_resource($this->finfo)) {
            finfo_close($this->finfo);
        }
    }

    /**
     * Valida un archivo subido. Lanza Exception si no es válido.
     */
    public function validateUpload(array $file): void
    {
        if (!isset($file['error']) || $file['error'] !== UPLOAD_ERR_OK) {
            throw new InvalidArgumentException('Invalid file upload');
        }

        if ($file['size'] > $this->maxFileSize) {
            throw new InvalidArgumentException('File exceeds maximum size limit');
        }

        if (!is_uploaded_file($file['tmp_name'])) {
            throw new InvalidArgumentException('File is not uploaded via HTTP POST');
        }

        $mime = finfo_file($this->finfo, $file['tmp_name']);
        if ($mime === false || !in_array($mime, $this->allowedMimes, true)) {
            throw new InvalidArgumentException('Unsupported image type: ' . ($mime ?: 'unknown'));
        }
    }

    /**
     * Guarda una imagen de perfil. Retorna el nombre del archivo guardado.
     */
    public function saveProfileImage(array $file, string $userId): string
    {
        $this->validateUpload($file);

        $userDir = $this->baseDir . DIRECTORY_SEPARATOR . $userId;
        if (!is_dir($userDir)) {
            if (!mkdir($userDir, 0755, true)) {
                throw new RuntimeException('Failed to create user directory: ' . $userDir);
            }
        }

        // Reutilizamos finfo (ya validado) para obtener extensión
        $ext = $this->mimeToExt[finfo_file($this->finfo, $file['tmp_name'])] ?? 'jpg';
        $fileName = 'profile.' . $ext;
        $targetPath = $userDir . DIRECTORY_SEPARATOR . $fileName;

        // Evitar race condition simple
        if (file_exists($targetPath)) {
            @unlink($targetPath);
        }

        if (!move_uploaded_file($file['tmp_name'], $targetPath)) {
            throw new RuntimeException('Failed to save image to: ' . $targetPath);
        }

        // Asegurar permisos de lectura (seguro para web)
        @chmod($targetPath, 0644);

        return $fileName;
    }

    /**
     * Sirve una imagen. Retorna array con metadata o false si no existe.
     */
    public function serveImage(string $path): array|false
    {
        // Normalización segura de rutas
        $normalized = str_replace('\\', '/', $path);
        $relative   = trim($normalized, '/');
        $fullPath   = $this->baseDir . DIRECTORY_SEPARATOR . $relative;
        $realPath   = realpath($fullPath);

        if ($realPath === false || !is_file($realPath) || !is_readable($realPath)) {
            return false;
        }

        // 🔒 SEGURIDAD CRÍTICA: Prevenir symlink traversal fuera del baseDir
        if (strpos($realPath, $this->baseDir . DIRECTORY_SEPARATOR) !== 0) {
            return false;
        }

        $mime = finfo_file($this->finfo, $realPath);
        return [
            'path'     => $realPath,
            'mimeType' => $mime ?: 'application/octet-stream',
            'filename' => basename($realPath),
        ];
    }

    /**
     * Genera la URL pública de la imagen.
     */
    public function generateImageUrl(string $userIdOrPath): string
    {
        if ($userIdOrPath === '' || $userIdOrPath === null) {
            return $this->urlBasePath . '/default/profile.jpg';
        }

        $path = str_starts_with($userIdOrPath, '/') ? $userIdOrPath : '/' . $userIdOrPath;
        return $this->urlBasePath . $path;
    }

    /**
     * Asegura que el directorio del usuario existe.
     */
    public function ensureUserDirectory(string $userId): string
    {
        $userDir = $this->baseDir . DIRECTORY_SEPARATOR . $userId;
        if (!is_dir($userDir)) {
            mkdir($userDir, 0755, true);
        }
        return $userDir;
    }
}