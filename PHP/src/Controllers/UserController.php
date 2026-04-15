<?php

namespace FuturePrograms\Clients\Controllers;

use FuturePrograms\Clients\Models\User;
use FuturePrograms\Clients\Utils\JwtHelper;
use FuturePrograms\Clients\Utils\ResponseFormatter;
use FuturePrograms\Clients\Utils\ImageHelper;
use Psr\Http\Message\ServerRequestInterface as Request;
use Psr\Http\Message\ResponseInterface as Response;

class UserController
{
    /**
     * POST /api/user/register - Registra un nuevo usuario
     */
    public function register(Request $request, Response $response): Response
    {
        try {
            $params = $request->getParsedBody();
            
            // Validar campos requeridos
            $required = ['nick', 'name', 'surname1', 'email', 'phone', 'password', 'gender'];
            foreach ($required as $field) {
                if (empty($params[$field])) {
                    throw new \Exception(ucfirst($field) . ' is required');
                }
            }

            $userModel = new User();
            
            // Registrar usuario
            $userId = $userModel->registerUser(
                $params['nick'],
                $params['name'],
                $params['surname1'],
                $params['surname2'] ?? null,
                $params['email'],
                $params['phone'],
                $params['password'],
                $params['gender'],
                $params['bday'] ?? null
            );

            // Manejar foto de perfil si existe
            if ($request->getUploadedFiles() && isset($request->getUploadedFiles()['profilePicture'])) {
                try {
                    $profilePicture = $request->getUploadedFiles()['profilePicture'];
                    
                    $fileArray = [
                        'tmp_name' => $profilePicture->getStream()->getMetadata('uri'),
                        'name' => $profilePicture->getClientFilename(),
                        'size' => $profilePicture->getSize(),
                        'error' => UPLOAD_ERR_OK
                    ];

                    ImageHelper::validateImageFile($fileArray);
                    ImageHelper::ensureUserImageDirectory($userId);

                    $extension = pathinfo($profilePicture->getClientFilename(), PATHINFO_EXTENSION);
                    $filePath = ImageHelper::getUploadDir() . '/' . $userId . '/profile.' . $extension;
                    $profilePicture->moveTo($filePath);

                    $userModel->updateProfileImage($userId, 'profile.' . $extension);
                } catch (\Exception $e) {
                    // Continuar sin imagen si falla
                }
            }

            // Obtener usuario registrado
            $user = $userModel->getUserById($userId);
            $token = JwtHelper::generateToken($user['email'], $user['id'], $user['role']);

            $result = ResponseFormatter::success([
                'token' => $token,
                'data' => User::toDto($user)
            ], 'User registered successfully', 200);

            return $this->jsonResponse($response, $result['body'], $result['status']);

        } catch (\Exception $e) {
            $statusCode = 400;
            if (strpos($e->getMessage(), 'already') !== false) {
                $statusCode = 409; // Conflict
            }
            $result = ResponseFormatter::error($e->getMessage(), $statusCode);
            return $this->jsonResponse($response, $result['body'], $result['status']);
        }
    }

    /**
     * GET /api/user - Obtiene lista de usuarios con paginación
     */
    public function getAllUsers(Request $request, Response $response): Response
    {
        try {
            $user = $request->getAttribute('user');
            if (!$user) {
                $result = ResponseFormatter::error('Unauthorized', 401);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            if (!in_array($user['role'], ['ADMIN', 'PREMIUM'])) {
                $result = ResponseFormatter::error('Unauthorized', 403);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            $page = (int)($request->getQueryParams()['page'] ?? 0);
            $size = (int)($request->getQueryParams()['size'] ?? 10);

            $userModel = new User();
            $resultData = $userModel->getAllUsers($page, $size, $user['userId'] ?? null);

            $result = ResponseFormatter::paginated(
                array_map([User::class, 'toDto'], $resultData['users']),
                $resultData['total'],
                $page,
                $size,
                'Users fetched successfully'
            );

            return $this->jsonResponse($response, $result['body'], $result['status']);

        } catch (\Exception $e) {
            $result = ResponseFormatter::error($e->getMessage(), 400);
            return $this->jsonResponse($response, $result['body'], $result['status']);
        }
    }

    /**
     * GET /api/user/{id} - Obtiene usuario por ID
     */
    public function getUserById(Request $request, Response $response, array $args): Response
    {
        try {
            $user = $request->getAttribute('user');
            if (!$user) {
                $result = ResponseFormatter::error('Unauthorized', 401);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            // Verificar rol
            if (!in_array($user['role'], ['ADMIN', 'PREMIUM'])) {
                $result = ResponseFormatter::error('Unauthorized', 403);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            $userId = $args['id'] ?? null;
            if (!$userId) {
                throw new \Exception('User ID is required');
            }

            $userModel = new User();
            $userData = $userModel->getUserById($userId);

            if (!$userData) {
                $result = ResponseFormatter::error('User not found', 404);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            $result = ResponseFormatter::success(
                User::toDto($userData),
                'User fetched successfully',
                200
            );

            return $this->jsonResponse($response, $result['body'], $result['status']);

        } catch (\Exception $e) {
            $result = ResponseFormatter::error($e->getMessage(), 400);
            return $this->jsonResponse($response, $result['body'], $result['status']);
        }
    }

    /**
     * DELETE /api/user/{id} - Elimina un usuario (ADMIN only)
     */
    public function deleteUser(Request $request, Response $response, array $args): Response
    {
        try {
            $user = $request->getAttribute('user');
            if (!$user || $user['role'] !== 'ADMIN') {
                $result = ResponseFormatter::error('Unauthorized: Admin access required', 403);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            $userId = $args['id'] ?? null;
            if (!$userId) {
                throw new \Exception('User ID is required');
            }

            $userModel = new User();
            $userData = $userModel->getUserById($userId);

            if (!$userData) {
                $result = ResponseFormatter::error('User not found', 404);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            if ($userData['role'] === 'ADMIN') {
                throw new \Exception('Cannot delete ADMIN users');
            }

            $userModel->deleteUser($userId);

            $result = ResponseFormatter::success([], 'User deleted successfully', 200);
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
