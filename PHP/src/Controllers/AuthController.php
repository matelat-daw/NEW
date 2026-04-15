<?php

namespace FuturePrograms\Clients\Controllers;

use FuturePrograms\Clients\Models\User;
use FuturePrograms\Clients\Utils\JwtHelper;
use FuturePrograms\Clients\Utils\ResponseFormatter;
use FuturePrograms\Clients\Utils\ImageHelper;
use Psr\Http\Message\ServerRequestInterface as Request;
use Psr\Http\Message\ResponseInterface as Response;

class AuthController
{
    /**
     * POST /api/auth/login - Login del usuario
     */
    public function login(Request $request, Response $response): Response
    {
        try {
            $data = $request->getParsedBody();
            $email = $data['email'] ?? null;
            $password = $data['password'] ?? null;

            if (!$email || !$password) {
                throw new \Exception('Email and password are required');
            }

            $userModel = new User();
            
            // Validar credenciales
            if (!$userModel->validateCredentials($email, $password)) {
                $result = ResponseFormatter::error('Invalid credentials', 401);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            $user = $userModel->getUserByEmail($email);
            if (!$user) {
                $result = ResponseFormatter::error('User not found', 404);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            // Generar token JWT
            $token = JwtHelper::generateToken($user['email'], $user['id'], $user['role']);

            // Preparar respuesta en formato esperado por frontend
            $result = [
                'success' => true,
                'message' => 'Login successful',
                'token' => $token,
                'data' => User::toDto($user),
            ];

            return $this->jsonResponse($response, $result, 200);

        } catch (\Exception $e) {
            $result = ResponseFormatter::error($e->getMessage(), 400);
            return $this->jsonResponse($response, $result['body'], $result['status']);
        }
    }

    /**
     * GET /api/auth/verify/{token} - Verifica el email del usuario
     */
    public function verifyEmail(Request $request, Response $response, array $args): Response
    {
        try {
            $token = $args['token'] ?? null;

            if (!$token) {
                throw new \Exception('Verification token is required');
            }

            $userModel = new User();
            $verified = $userModel->verifyEmail($token);

            if (!$verified) {
                $loginUrl = 'http://localhost/register';
                return $response
                    ->withStatus(302)
                    ->withHeader('Location', $loginUrl . '?verification=failed');
            }

            $loginUrl = 'http://localhost/login';
            return $response
                ->withStatus(302)
                ->withHeader('Location', $loginUrl . '?verified=1');

        } catch (\Exception $e) {
            return $response->withStatus(500);
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
