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
        try {
            $user = $request->getAttribute('user');
            if (!$user || !isset($user['userId'])) {
                $result = ResponseFormatter::error('Unauthorized', 401);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            $uploadedFiles = $request->getUploadedFiles();
            $profilePicture = $uploadedFiles['profilePicture'] ?? null;

            if (!$profilePicture) {
                throw new \Exception('No image file provided');
            }

            // Convertir PSR-7 UploadedFileInterface a formato estándar $_FILES
            $file = [
                'name'     => $profilePicture->getClientFilename(),
                'type'     => $profilePicture->getClientMediaType(),
                'size'     => $profilePicture->getSize(),
                'tmp_name' => $profilePicture->getStream()->getMetadata('uri'),
                'error'    => $profilePicture->getError(),
            ];

            // Utilizar ImageHelper optimizado para validación y guardado
            $uploadDir = dirname(dirname(__DIR__)) . DIRECTORY_SEPARATOR . 'uploads';
            $imageHelper = new ImageHelper($uploadDir);
            
            // Validar archivo
            $imageHelper->validateUpload($file);

            // Guardar imagen y obtener nombre del archivo
            $fileName = $imageHelper->saveProfileImage($file, $user['userId']);

            // Actualizar base de datos con referencia al archivo
            $userModel = new User();
            $updated = $userModel->updateProfileImage($user['userId'], $fileName);

            $result = ResponseFormatter::success(
                User::toDto($updated),
                'Profile picture updated successfully',
                200
            );

            return $this->jsonResponse($response, $result['body'], $result['status']);

        } catch (\InvalidArgumentException $e) {
            $result = ResponseFormatter::error($e->getMessage(), 400);
            return $this->jsonResponse($response, $result['body'], $result['status']);
        } catch (\RuntimeException $e) {
            $result = ResponseFormatter::error($e->getMessage(), 500);
            return $this->jsonResponse($response, $result['body'], $result['status']);
        } catch (\Exception $e) {
            $result = ResponseFormatter::error($e->getMessage(), 400);
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
