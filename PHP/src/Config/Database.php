<?php

namespace FuturePrograms\Clients\Config;

use Exception;

class Database
{
    private static $instanceClients = null;
    private static $instanceMyikea = null;

    /**
     * Obtiene conexión a la BD de Clients
     */
    public static function getClientConnection()
    {
        if (self::$instanceClients === null) {
            self::$instanceClients = self::connect('clients');
        }
        return self::$instanceClients;
    }

    /**
     * Obtiene conexión a la BD de MyIkea
     */
    public static function getMyikeaConnection()
    {
        if (self::$instanceMyikea === null) {
            self::$instanceMyikea = self::connect('myikea');
        }
        return self::$instanceMyikea;
    }

    /**
     * Crea conexión a la BD
     */
    private static function connect($prefix)
    {
        $host = 'localhost';
        $name = $prefix;
        $user = 'root';
        $pass = getenv('MySQL') ?: '';
        $port = 3306;

        if (!$host || !$name) {
            throw new Exception('Database configuration missing for ' . $prefix);
        }

        try {
            $dsn = "mysql:host=$host;port=$port;dbname=$name;charset=utf8mb4";
            $pdo = new \PDO($dsn, $user, $pass, [
                \PDO::ATTR_ERRMODE => \PDO::ERRMODE_EXCEPTION,
                \PDO::ATTR_DEFAULT_FETCH_MODE => \PDO::FETCH_ASSOC,
                \PDO::ATTR_EMULATE_PREPARES => false,
            ]);
            return $pdo;
        } catch (\PDOException $e) {
            throw new Exception('Database connection failed: ' . $e->getMessage());
        }
    }

    /**
     * Cierra todas las conexiones
     */
    public static function closeAll()
    {
        self::$instanceClients = null;
        self::$instanceMyikea = null;
    }
}