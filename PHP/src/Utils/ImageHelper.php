<?php

namespace FuturePrograms\Clients\Utils;

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
     * Guarda una imagen de perfil desde PSR-7 UploadedFileInterface. Retorna el nombre del archivo guardado.
     */
    public function saveProfileImageFromStream($uploadedFile, string $userId): string
    {
        // Validación básica
        if (!$uploadedFile) {
            throw new InvalidArgumentException('No file provided');
        }

        $error = $uploadedFile->getError();
        if ($error !== UPLOAD_ERR_OK) {
            $errorMessages = [
                UPLOAD_ERR_INI_SIZE => 'File exceeds upload_max_filesize',
                UPLOAD_ERR_FORM_SIZE => 'File exceeds form MAX_FILE_SIZE',
                UPLOAD_ERR_PARTIAL => 'File upload was incomplete',
                UPLOAD_ERR_NO_FILE => 'No file was uploaded',
                UPLOAD_ERR_NO_TMP_DIR => 'Temporary folder missing',
                UPLOAD_ERR_CANT_WRITE => 'Failed to write file to disk',
                UPLOAD_ERR_EXTENSION => 'File upload stopped by extension',
            ];
            throw new InvalidArgumentException($errorMessages[$error] ?? 'Unknown upload error');
        }

        $size = $uploadedFile->getSize();
        if ($size === null || $size === 0) {
            throw new InvalidArgumentException('File is empty');
        }

        if ($size > $this->maxFileSize) {
            throw new InvalidArgumentException('File exceeds maximum size limit of ' . ($this->maxFileSize / 1024 / 1024) . 'MB');
        }

        // Obtener MIME type
        $mime = $uploadedFile->getClientMediaType();
        if (!$mime) {
            throw new InvalidArgumentException('Unable to determine file type');
        }

        if (!in_array($mime, $this->allowedMimes, true)) {
            throw new InvalidArgumentException('Unsupported image type: ' . $mime . '. Allowed: ' . implode(', ', $this->allowedMimes));
        }

        // Crear directorio de usuario si no existe
        $userDir = $this->baseDir . DIRECTORY_SEPARATOR . $userId;
        if (!is_dir($userDir)) {
            if (!mkdir($userDir, 0755, true)) {
                throw new RuntimeException('Failed to create user directory: ' . $userDir);
            }
        }

        // Obtener extensión del MIME type
        $ext = $this->mimeToExt[$mime] ?? 'jpg';
        $fileName = 'profile.' . $ext;
        $targetPath = $userDir . DIRECTORY_SEPARATOR . $fileName;

        // Eliminar archivo anterior si existe
        if (file_exists($targetPath)) {
            if (!@unlink($targetPath)) {
                throw new RuntimeException('Failed to remove existing profile image');
            }
        }

        // Obtener stream del archivo subido
        $stream = $uploadedFile->getStream();
        if (!$stream) {
            throw new RuntimeException('Failed to get upload stream');
        }

        // Escribir stream al archivo de destino
        $targetHandle = null;
        try {
            $targetHandle = fopen($targetPath, 'wb');
            if (!$targetHandle) {
                throw new RuntimeException('Failed to open target file for writing: ' . $targetPath);
            }

            // Intentar rewind stream si es seekable
            try {
                if (method_exists($stream, 'isSeekable') && $stream->isSeekable()) {
                    $stream->rewind();
                } elseif (method_exists($stream, 'seekable') && $stream->seekable()) {
                    $stream->rewind();
                }
            } catch (\Exception $seekException) {
                // Si no se puede rewind, simplemente continuamos
            }

            // Leer y escribir en chunks
            $bytesWritten = 0;
            while (!$stream->eof()) {
                $chunk = $stream->read(65536); // 64KB chunks
                if ($chunk === '') {
                    break;
                }
                $written = fwrite($targetHandle, $chunk);
                if ($written === false) {
                    throw new RuntimeException('Error writing to target file');
                }
                $bytesWritten += $written;
            }

            if ($bytesWritten === 0) {
                throw new RuntimeException('No data was written to the target file');
            }

        } catch (\Exception $e) {
            // Limpiar si falla
            if ($targetHandle) {
                fclose($targetHandle);
            }
            if (file_exists($targetPath)) {
                @unlink($targetPath);
            }
            throw $e;
        }

        // Cerrar archivo correctamente
        if ($targetHandle) {
            fclose($targetHandle);
        }

        // Asegurar permisos de lectura
        @chmod($targetPath, 0644);

        return $fileName;
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

    /**
     * Obtiene el directorio base
     */
    public function getBaseDir(): string
    {
        return $this->baseDir;
    }
}

