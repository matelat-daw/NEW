<?php

namespace FuturePrograms\Clients\Utils;

class ResponseFormatter
{
    /**
     * Respuesta exitosa
     */
    public static function success($data = null, $message = "Success", $statusCode = 200)
    {
        $response = [
            'success' => true,
            'message' => $message
        ];

        if ($data !== null) {
            // Si es un array asociativo simple, usar como "data"
            if (is_array($data) && !isset($data['success'])) {
                $response['data'] = $data;
            } else {
                // Si es un objeto o array con estructura, fusionar
                if (is_array($data)) {
                    $response = array_merge($response, $data);
                } else {
                    $response['data'] = $data;
                }
            }
        }

        return [
            'status' => $statusCode,
            'body' => $response
        ];
    }

    /**
     * Respuesta de error
     */
    public static function error($message = "Error", $statusCode = 400, $data = null)
    {
        $response = [
            'success' => false,
            'message' => $message
        ];

        if ($data !== null) {
            $response['errors'] = $data;
        }

        return [
            'status' => $statusCode,
            'body' => $response
        ];
    }

    /**
     * Respuesta con paginación
     */
    public static function paginated($items, $total, $currentPage, $pageSize, $message = "Data fetched successfully")
    {
        $totalPages = ceil($total / $pageSize);
        
        return [
            'status' => 200,
            'body' => [
                'success' => true,
                'message' => $message,
                'users' => $items, // Compatible con formato del frontend
                'pagination' => [
                    'currentPage' => (int)$currentPage,
                    'totalItems' => (int)$total,
                    'totalPages' => (int)$totalPages,
                    'pageSize' => (int)$pageSize,
                    'hasNext' => $currentPage < $totalPages - 1,
                    'hasPrevious' => $currentPage > 0
                ]
            ]
        ];
    }

    /**
     * Respuesta con customers paginados
     */
    public static function paginatedCustomers($items, $total, $currentPage, $pageSize, $message = "Data fetched successfully")
    {
        $totalPages = ceil($total / $pageSize);
        
        return [
            'status' => 200,
            'body' => [
                'success' => true,
                'message' => $message,
                'customers' => $items,
                'pagination' => [
                    'currentPage' => (int)$currentPage,
                    'totalItems' => (int)$total,
                    'totalPages' => (int)$totalPages,
                    'pageSize' => (int)$pageSize,
                    'hasNext' => $currentPage < $totalPages - 1,
                    'hasPrevious' => $currentPage > 0
                ]
            ]
        ];
    }
}
