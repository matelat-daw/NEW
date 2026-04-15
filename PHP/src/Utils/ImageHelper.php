<?php

namespace FuturePrograms\Clients\Utils;

use Exception;

class ImageHelper
{
    private static $allowedMimes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    private static $allowedExtensions = ['jpg', 'jpeg', 'png', 'gif', 'webp'];

    /**
     * Obtiene el directorio de uploads
     */
    public static function getUploadDir()
    {
        $dir = __DIR__ . '/../../uploads';
        if (!$dir) {
            $dir = dirname(dirname(dirname(__DIR__))) . '/uploads';
        }
        return rtrim($dir, '/\\');
    }

    /**
     * Valida que el archivo sea una imagen
     */
    public static function validateImageFile($file)
    {
        if (!$file || !isset($file['error'])) {
            throw new Exception('Invalid file');
        }

        if ($file['error'] !== UPLOAD_ERR_OK) {
            throw new Exception('File upload error: ' . $file['error']);
        }

        $maxSize = 20971520; // 20MB
        if ($file['size'] > $maxSize) {
            throw new Exception('File size exceeds limit');
        }

        $finfo = finfo_open(FILEINFO_MIME_TYPE);
        $mimeType = finfo_file($finfo, $file['tmp_name']);
        finfo_close($finfo);

        if (!in_array($mimeType, self::$allowedMimes)) {
            throw new Exception('Invalid file type: ' . $mimeType);
        }

        return true;
    }

    /**
     * Guarda una imagen de perfil
     */
    public static function saveProfileImage($file, $userId)
    {
        self::validateImageFile($file);

        $uploadDir = self::getUploadDir();
        $userDir = $uploadDir . '/' . $userId;

        // Crear directorio del usuario si no existe
        if (!is_dir($userDir)) {
            mkdir($userDir, 0755, true);
        }

        // Obtener extensión
        $ext = self::getExtensionFromMime(finfo_file(finfo_open(FILEINFO_MIME_TYPE), $file['tmp_name']));
        $fileName = 'profile.' . $ext;
        $filePath = $userDir . '/' . $fileName;

        // Eliminar imagen anterior si existe
        if (file_exists($filePath)) {
            unlink($filePath);
        }

        // Guardar nueva imagen
        if (!move_uploaded_file($file['tmp_name'], $filePath)) {
            throw new Exception('Failed to save image');
        }

        return $fileName;
    }

    /**
     * Obtiene la extensión del MIME type
     */
    private static function getExtensionFromMime($mime)
    {
        $extensions = [
            'image/jpeg' => 'jpg',
            'image/png' => 'png',
            'image/gif' => 'gif',
            'image/webp' => 'webp'
        ];
        return $extensions[$mime] ?? 'jpg';
    }

    /**
     * Obtiene la ruta de una imagen
     */
    public static function getImageUrl($userIdOrPath)
    {
        if (!$userIdOrPath) {
            return '/api/images/default/profile.jpg';
        }

        // Si comienza con /, usarlo directamente
        if (strpos($userIdOrPath, '/') === 0) {
            return '/api/images' . $userIdOrPath;
        }

        return '/api/images/' . $userIdOrPath;
    }

    /**
     * Sirve una imagen desde el directorio de uploads
     */
    public static function serveImage($path)
    {
        $uploadDir = self::getUploadDir();
        
        // Normalizar y sanitizar la ruta
        $requestedPath = str_replace('\\', '/', $path);
        $requestedPath = trim($requestedPath, '/');
        $requestedPath = str_replace('..', '', $requestedPath); // Prevenir directory traversal
        
        $filePath = realpath($uploadDir . '/' . $requestedPath);
        
        if (!$filePath || !file_exists($filePath) || !is_readable($filePath)) {
            return false;
        }

        // Verificar que el archivo está dentro del directorio de uploads
        if (strpos($filePath, realpath($uploadDir)) !== 0) {
            return false;
        }

        $mimeType = self::detectMimeType($filePath);
        
        return [
            'path' => $filePath,
            'mimeType' => $mimeType,
            'filename' => basename($filePath)
        ];
    }

    /**
     * Detecta el MIME type de forma segura.
     */
    private static function detectMimeType($filePath)
    {
        if (function_exists('finfo_open')) {
            $finfo = finfo_open(FILEINFO_MIME_TYPE);
            if ($finfo) {
                $mimeType = finfo_file($finfo, $filePath);
                finfo_close($finfo);

                if ($mimeType) {
                    return $mimeType;
                }
            }
        }

        $extension = strtolower(pathinfo($filePath, PATHINFO_EXTENSION));
        $mimeMap = [
            'jpg' => 'image/jpeg',
            'jpeg' => 'image/jpeg',
            'png' => 'image/png',
            'gif' => 'image/gif',
            'webp' => 'image/webp',
        ];

        return $mimeMap[$extension] ?? 'application/octet-stream';
    }

    /**
     * Crea el directorio del usuario si no existe
     */
    public static function ensureUserImageDirectory($userId)
    {
        $userDir = self::getUploadDir() . '/' . $userId;
        if (!is_dir($userDir)) {
            mkdir($userDir, 0755, true);
        }
    }
}
