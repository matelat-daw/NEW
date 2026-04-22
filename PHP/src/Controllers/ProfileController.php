<?php

namespace FuturePrograms\Clients\Controllers;

use FuturePrograms\Clients\Models\User;
use FuturePrograms\Clients\Utils\ResponseFormatter;
use FuturePrograms\Clients\Utils\ImageHelper;
use Psr\Http\Message\ServerRequestInterface as Request;
use Psr\Http\Message\ResponseInterface as Response;

class ProfileController
{
    /**
     * GET /api/profile - Obtiene el perfil del usuario autenticado
     */
    public function getProfile(Request $request, Response $response): Response
    {
        try {
            $user = $request->getAttribute('user');
            if (!$user || !isset($user['userId'])) {
                $result = ResponseFormatter::error('Unauthorized', 401);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            $userModel = new User();
            $userData = $userModel->getUserById($user['userId']);

            if (!$userData) {
                $result = ResponseFormatter::error('User not found', 404);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            $result = ResponseFormatter::success(
                User::toDto($userData),
                'Profile fetched successfully',
                200
            );

            return $this->jsonResponse($response, $result['body'], $result['status']);

        } catch (\Exception $e) {
            $result = ResponseFormatter::error($e->getMessage(), 400);
            return $this->jsonResponse($response, $result['body'], $result['status']);
        }
    }

    /**
     * PUT /api/profile - Actualiza el perfil del usuario
     */
    public function updateProfile(Request $request, Response $response): Response
    {
        try {
            $user = $request->getAttribute('user');
            if (!$user || !isset($user['userId'])) {
                $result = ResponseFormatter::error('Unauthorized', 401);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            $data = $request->getParsedBody();
            
            $userModel = new User();
            $updated = $userModel->updateProfile(
                $user['userId'],
                $data['name'] ?? null,
                $data['surname1'] ?? null,
                $data['surname2'] ?? null,
                $data['phone'] ?? null
            );

            $result = ResponseFormatter::success(
                User::toDto($updated),
                'Profile updated successfully',
                200
            );

            return $this->jsonResponse($response, $result['body'], $result['status']);

        } catch (\Exception $e) {
            $result = ResponseFormatter::error($e->getMessage(), 400);
            return $this->jsonResponse($response, $result['body'], $result['status']);
        }
    }

    /**
     * POST /api/profile/picture - Actualiza la foto de perfil
     */
    public function updateProfilePicture(Request $request, Response $response): Response
    {
        $logFile = dirname(dirname(__DIR__)) . '/debug_log.txt';
        file_put_contents($logFile, "--- New Upload Request ---\n", FILE_APPEND);

        try {
            $user = $request->getAttribute('user');
            if (!$user || !isset($user['userId'])) {
                file_put_contents($logFile, "Error: Unauthorized access attempt.\n", FILE_APPEND);
                $result = ResponseFormatter::error('Unauthorized', 401);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }
            file_put_contents($logFile, "User authenticated: ID " . $user['userId'] . "\n", FILE_APPEND);

            // Usar $_FILES directamente
            if (empty($_FILES['profilePicture'])) {
                file_put_contents($logFile, "Error: \$_FILES['profilePicture'] is empty.\n", FILE_APPEND);
                throw new \Exception('No file provided');
            }
            file_put_contents($logFile, "File data received: " . print_r($_FILES, true) . "\n", FILE_APPEND);


            $file = $_FILES['profilePicture'];

            // Validar error
            if ($file['error'] !== UPLOAD_ERR_OK) {
                $errorMessage = 'Upload error code: ' . $file['error'];
                file_put_contents($logFile, "Error: " . $errorMessage . "\n", FILE_APPEND);
                if ($file['error'] === UPLOAD_ERR_NO_FILE) {
                    throw new \Exception('No file uploaded');
                }
                throw new \Exception($errorMessage);
            }

            // Validar archivo
            if (!is_uploaded_file($file['tmp_name'])) {
                file_put_contents($logFile, "Error: Not an uploaded file.\n", FILE_APPEND);
                throw new \Exception('Invalid upload');
            }
            file_put_contents($logFile, "File is a valid uploaded file.\n", FILE_APPEND);

            // Crear directorio del usuario
            $baseDir = dirname(dirname(__DIR__)); // Should be d:\BackUp\NEW\PHP\src -> d:\BackUp\NEW\PHP
            file_put_contents($logFile, "Base directory: $baseDir\n", FILE_APPEND);
            
            $uploadDir = $baseDir . DIRECTORY_SEPARATOR . 'uploads';
            file_put_contents($logFile, "Uploads directory path: $uploadDir\n", FILE_APPEND);

            if (!is_dir($uploadDir)) {
                 file_put_contents($logFile, "Warning: Main uploads directory does not exist.\n", FILE_APPEND);
            } elseif (!is_writable($uploadDir)) {
                file_put_contents($logFile, "Error: Main uploads directory is not writable.\n", FILE_APPEND);
                throw new \Exception('Uploads directory is not writable.');
            }

            $userDir = $uploadDir . DIRECTORY_SEPARATOR . $user['userId'];
            file_put_contents($logFile, "User directory path: $userDir\n", FILE_APPEND);
            
            if (!is_dir($userDir)) {
                file_put_contents($logFile, "User directory does not exist. Attempting to create.\n", FILE_APPEND);
                if (!@mkdir($userDir, 0755, true)) {
                    $error = error_get_last();
                    file_put_contents($logFile, "Error: Failed to create directory $userDir. " . $error['message'] . "\n", FILE_APPEND);
                    throw new \Exception('Failed to create user directory.');
                }
                file_put_contents($logFile, "User directory created successfully.\n", FILE_APPEND);
            }

            // Detectar tipo MIME
            $mimeType = mime_content_type($file['tmp_name']);
            file_put_contents($logFile, "MIME type detected: $mimeType\n", FILE_APPEND);
            $ext = 'jpg';
            
            if ($mimeType === 'image/png') {
                $ext = 'png';
            } else if ($mimeType === 'image/gif') {
                $ext = 'gif';
            } else if ($mimeType === 'image/webp') {
                $ext = 'webp';
            } else if ($mimeType !== 'image/jpeg') {
                file_put_contents($logFile, "Error: Invalid image type '$mimeType'.\n", FILE_APPEND);
                throw new \Exception('Invalid image type');
            }
            file_put_contents($logFile, "File extension set to: $ext\n", FILE_APPEND);

            // Ruta final
            $fileName = 'profile.' . $ext;
            $targetPath = $userDir . DIRECTORY_SEPARATOR . $fileName;
            file_put_contents($logFile, "Target path: $targetPath\n", FILE_APPEND);

            // Eliminar anterior
            if (file_exists($targetPath)) {
                if(!@unlink($targetPath)) {
                     file_put_contents($logFile, "Warning: Could not delete old file $targetPath.\n", FILE_APPEND);
                } else {
                     file_put_contents($logFile, "Old file $targetPath deleted.\n", FILE_APPEND);
                }
            }

            // Mover archivo
            if (!move_uploaded_file($file['tmp_name'], $targetPath)) {
                $error = error_get_last();
                file_put_contents($logFile, "Error: Failed to move uploaded file to $targetPath. " . $error['message'] . "\n", FILE_APPEND);
                throw new \Exception('Failed to save file');
            }
            file_put_contents($logFile, "File moved to $targetPath.\n", FILE_APPEND);


            @chmod($targetPath, 0644);

            // Actualizar DB
            file_put_contents($logFile, "Updating database with new profile image: $fileName\n", FILE_APPEND);
            $userModel = new User();
            $updated = $userModel->updateProfileImage($user['userId'], $fileName);
            file_put_contents($logFile, "Database updated successfully.\n", FILE_APPEND);


            $result = ResponseFormatter::success(
                User::toDto($updated),
                'Profile picture updated successfully',
                200
            );

            return $this->jsonResponse($response, $result['body'], $result['status']);

        } catch (\Exception $e) {
            file_put_contents($logFile, "FATAL ERROR: " . $e->getMessage() . "\n" . $e->getTraceAsString() . "\n", FILE_APPEND);
            $result = ResponseFormatter::error($e->getMessage(), 500);
            return $this->jsonResponse($response, $result['body'], $result['status']);
        }
    }

    /**
     * PUT /api/profile/password - Cambia la contraseña del usuario
     */
    public function updatePassword(Request $request, Response $response): Response
    {
        try {
            $user = $request->getAttribute('user');
            if (!$user || !isset($user['userId'])) {
                $result = ResponseFormatter::error('Unauthorized', 401);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            $data = $request->getParsedBody();
            $currentPassword = $data['currentPassword'] ?? null;
            $newPassword = $data['newPassword'] ?? null;
            $confirmPassword = $data['confirmPassword'] ?? null;

            if (!$currentPassword || !$newPassword || !$confirmPassword) {
                throw new \Exception('All password fields are required');
            }

            if ($newPassword !== $confirmPassword) {
                throw new \Exception('New passwords do not match');
            }

            $userModel = new User();
            $userData = $userModel->getUserById($user['userId']);

            if (!$userData || !password_verify($currentPassword, $userData['password'])) {
                throw new \Exception('Current password is incorrect');
            }

            $userModel->updatePassword($user['userId'], $newPassword);

            $result = ResponseFormatter::success([], 'Password updated successfully', 200);
            return $this->jsonResponse($response, $result['body'], $result['status']);

        } catch (\Exception $e) {
            $result = ResponseFormatter::error($e->getMessage(), 400);
            return $this->jsonResponse($response, $result['body'], $result['status']);
        }
    }

    /**
     * POST /api/profile/delete - Elimina la cuenta del usuario
     */
    public function deleteProfile(Request $request, Response $response): Response
    {
        try {
            $user = $request->getAttribute('user');
            if (!$user || !isset($user['userId'])) {
                $result = ResponseFormatter::error('Unauthorized', 401);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            $data = $request->getParsedBody();
            $password = $data['password'] ?? null;

            if (!$password) {
                throw new \Exception('Password is required to delete account');
            }

            $userModel = new User();
            $userData = $userModel->getUserById($user['userId']);

            if (!$userData || !password_verify($password, $userData['password'])) {
                throw new \Exception('Invalid password');
            }

            $userModel->deleteUser($user['userId']);

            $result = ResponseFormatter::success([], 'Account deleted successfully', 200);
            return $this->jsonResponse($response, $result['body'], $result['status']);

        } catch (\Exception $e) {
            $result = ResponseFormatter::error($e->getMessage(), 400);
            return $this->jsonResponse($response, $result['body'], $result['status']);
        }
    }

    /**
     * Convierte respuesta a JSON
     */
    private function jsonResponse(Response $response, $data, $statusCode = 200): Response
    {
        $response->getBody()->write(json_encode($data));
        return $response
            ->withHeader('Content-Type', 'application/json')
            ->withStatus($statusCode);
    }
}
