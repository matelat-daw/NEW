<?php

namespace FuturePrograms\Clients\Models;

use FuturePrograms\Clients\Config\Database;
use Exception;

class Customer
{
    private $pdo;

    public function __construct()
    {
        $this->pdo = Database::getMyikeaConnection();
    }

    /**
     * Obtiene todos los customers con paginación
     */
    public function getAllCustomers($page = 0, $size = 10)
    {
        $offset = $page * $size;
        
        $stmt = $this->pdo->prepare('
            SELECT
                customer_id AS customerId,
                first_name AS firstName,
                last_name AS lastName,
                telefono,
                email,
                fecha_de_nacimiento AS fechaDeNacimiento
            FROM customer
            ORDER BY customer_id DESC
            LIMIT :limit OFFSET :offset
        ');
        
        $stmt->bindValue(':limit', $size, \PDO::PARAM_INT);
        $stmt->bindValue(':offset', $offset, \PDO::PARAM_INT);
        $stmt->execute();
        
        $results = $stmt->fetchAll();
        
        // Obtener total de registros
        $countStmt = $this->pdo->query('SELECT COUNT(*) as total FROM customer');
        $total = $countStmt->fetch()['total'];
        
        return [
            'customers' => $results,
            'total' => $total
        ];
    }

    /**
     * Obtiene customer por ID
     */
    public function getCustomerById($id)
    {
        $stmt = $this->pdo->prepare('
            SELECT
                customer_id AS customerId,
                first_name AS firstName,
                last_name AS lastName,
                telefono,
                email,
                fecha_de_nacimiento AS fechaDeNacimiento
            FROM customer
            WHERE customer_id = ?
        ');
        
        $stmt->execute([$id]);
        return $stmt->fetch();
    }

    /**
     * Busca customers por nombre
     */
    public function searchByFirstName($firstName)
    {
        $stmt = $this->pdo->prepare('
            SELECT
                customer_id AS customerId,
                first_name AS firstName,
                last_name AS lastName,
                telefono,
                email,
                fecha_de_nacimiento AS fechaDeNacimiento
            FROM customer
            WHERE first_name LIKE ?
            ORDER BY first_name ASC
        ');
        
        $stmt->execute(['%' . $firstName . '%']);
        return $stmt->fetchAll();
    }

    /**
     * Busca customers por apellido
     */
    public function searchByLastName($lastName)
    {
        $stmt = $this->pdo->prepare('
            SELECT
                customer_id AS customerId,
                first_name AS firstName,
                last_name AS lastName,
                telefono,
                email,
                fecha_de_nacimiento AS fechaDeNacimiento
            FROM customer
            WHERE last_name LIKE ?
            ORDER BY last_name ASC
        ');
        
        $stmt->execute(['%' . $lastName . '%']);
        return $stmt->fetchAll();
    }

    /**
     * Elimina un customer
     */
    public function deleteCustomer($customerId)
    {
        $stmt = $this->pdo->prepare('DELETE FROM customer WHERE customer_id = ?');
        $stmt->execute([$customerId]);

        return true;
    }

    /**
     * Convierte resultado de BD a DTO
     */
    public static function toDto($customer)
    {
        if (!$customer) {
            return null;
        }

        return [
            'customerId' => (int)$customer['customerId'],
            'firstName' => $customer['firstName'],
            'lastName' => $customer['lastName'],
            'telefono' => $customer['telefono'],
            'email' => $customer['email'],
            'fechaDeNacimiento' => $customer['fechaDeNacimiento']
        ];
    }
}
