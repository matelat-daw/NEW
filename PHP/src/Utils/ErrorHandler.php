<?php

namespace FuturePrograms\Clients\Utils;

use Throwable;

/**
 * Manejador de excepciones global para la API
 */
class ErrorHandler
{
    /**
     * Registra y maneja excepciones
     */
    public static function handle(Throwable $thrown): array
    {
        $statusCode = 500;
        $message = 'Internal Server Error';

        if ($thrown instanceof \PDOException) {
            $statusCode = 500;
            $message = 'Database connection error';
            if (false) { // APP_DEBUG deshabilitado
                $message = $thrown->getMessage();
            }
        } elseif ($thrown instanceof \InvalidArgumentException) {
            $statusCode = 400;
            $message = $thrown->getMessage();
        } elseif ($thrown instanceof \RuntimeException) {
            $statusCode = 400;
            $message = $thrown->getMessage();
        } else {
            $message = $thrown->getMessage() ?: 'Unknown error';
        }

        $response = ResponseFormatter::error($message, $statusCode);
        
        // Log del error
        self::logError($thrown, $statusCode);

        return $response;
    }

    /**
     * Log de errores
     */
    private static function logError(Throwable $thrown, $statusCode): void
    {
        $logFile = dirname(dirname(dirname(__DIR__))) . '/logs/errors.log';
        
        if (!is_dir(dirname($logFile))) {
            mkdir(dirname($logFile), 0755, true);
        }

        $message = sprintf(
            "[%s] %s - %s:%d %s\n",
            date('Y-m-d H:i:s'),
            get_class($thrown),
            $thrown->getFile(),
            $thrown->getLine(),
            $thrown->getMessage()
        );

        error_log($message, 3, $logFile);
    }

    /**
     * Valida un email
     */
    public static function validateEmail($email): bool
    {
        return filter_var($email, FILTER_VALIDATE_EMAIL) !== false;
    }

    /**
     * Valida una contraseña segura
     * Debe tener mayúscula, minúscula, número y carácter especial
     */
    public static function validatePassword($password): bool
    {
        if (strlen($password) < 8) {
            return false;
        }

        $hasUppercase = preg_match('/[A-Z]/', $password);
        $hasLowercase = preg_match('/[a-z]/', $password);
        $hasNumber = preg_match('/[0-9]/', $password);
        $hasSpecial = preg_match('/[@#$%^&+=!_.\-*]/', $password);

        return $hasUppercase && $hasLowercase && $hasNumber && $hasSpecial;
    }

    /**
     * Valida un teléfono
     */
    public static function validatePhone($phone): bool
    {
        $phone = preg_replace('/[^0-9+]/', '', $phone);
        return strlen($phone) >= 7 && strlen($phone) <= 15;
    }

    /**
     * Valida una fecha
     */
    public static function validateDate($date, $format = 'Y-m-d'): bool
    {
        $d = \DateTime::createFromFormat($format, $date);
        return $d && $d->format($format) === $date;
    }

    /**
     * Sanitiza un string
     */
    public static function sanitizeString($str): string
    {
        return trim(htmlspecialchars($str, ENT_QUOTES, 'UTF-8'));
    }

    /**
     * Valida un rol
     */
    public static function validateRole($role): bool
    {
        return in_array(strtoupper($role), ['USER', 'PREMIUM', 'ADMIN']);
    }

    /**
     * Valida un género
     */
    public static function validateGender($gender): bool
    {
        return in_array(strtoupper($gender), ['M', 'F', 'O']);
    }
}
