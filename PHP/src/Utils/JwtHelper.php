<?php

namespace FuturePrograms\Clients\Utils;

use Firebase\JWT\JWT;
use Firebase\JWT\Key;

class JwtHelper
{
    /**
     * Genera un token JWT
     */
    public static function generateToken($email, $userId = null, $role = null)
    {
        $secret = getenv('JWT-Secret');
        $expiration = 86400; // 24 horas

        $payload = [
            'iss' => 'http://localhost',
            'aud' => 'http://localhost',
            'iat' => time(),
            'exp' => time() + $expiration,
            'email' => $email,
            'userId' => $userId,
            'role' => $role
        ];

        return JWT::encode($payload, $secret, 'HS256');
    }

    /**
     * Valida y decodifica un token JWT
     */
    public static function validateToken($token)
    {
        try {
            $secret = getenv('JWT-Secret');
            $decoded = JWT::decode($token, new Key($secret, 'HS256'));
            return $decoded;
        } catch (\Exception $e) {
            return null;
        }
    }

    /**
     * Obtiene el token del header Authorization
     */
    public static function getTokenFromRequest($request)
    {
        $header = $request->getHeaderLine('Authorization');
        if (preg_match('/^Bearer\s+(.+)$/', $header, $matches)) {
            return $matches[1];
        }
        return null;
    }

    /**
     * Extrae el email del payload del token
     */
    public static function getEmailFromToken($token)
    {
        $decoded = self::validateToken($token);
        return $decoded ? $decoded->email : null;
    }

    /**
     * Extrae el userId del payload del token
     */
    public static function getUserIdFromToken($token)
    {
        $decoded = self::validateToken($token);
        return $decoded ? $decoded->userId : null;
    }

    /**
     * Extrae el role del payload del token
     */
    public static function getRoleFromToken($token)
    {
        $decoded = self::validateToken($token);
        return $decoded ? $decoded->role : null;
    }
}
