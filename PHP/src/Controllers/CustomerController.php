<?php

namespace FuturePrograms\Clients\Controllers;

use FuturePrograms\Clients\Models\Customer;
use FuturePrograms\Clients\Utils\ResponseFormatter;
use Psr\Http\Message\ServerRequestInterface as Request;
use Psr\Http\Message\ResponseInterface as Response;

class CustomerController
{
    /**
     * GET /api/myikea/customer - Obtiene lista de customers con paginación
     */
    public function getAllCustomers(Request $request, Response $response): Response
    {
        try {
            $user = $request->getAttribute('user');
            if (!$user) {
                $result = ResponseFormatter::error('Unauthorized', 401);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            // Verificar rol (ADMIN o PREMIUM)
            if (!in_array($user['role'], ['ADMIN', 'PREMIUM'])) {
                $result = ResponseFormatter::error('Unauthorized: Admin or Premium access required', 403);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            $page = (int)($request->getQueryParams()['page'] ?? 0);
            $size = (int)($request->getQueryParams()['size'] ?? 10);

            $customerModel = new Customer();
            $data = $customerModel->getAllCustomers($page, $size);

            $result = ResponseFormatter::paginatedCustomers(
                array_map([Customer::class, 'toDto'], $data['customers']),
                $data['total'],
                $page,
                $size,
                'Customers fetched successfully'
            );

            return $this->jsonResponse($response, $result['body'], $result['status']);

        } catch (\Exception $e) {
            $result = ResponseFormatter::error($e->getMessage(), 400);
            return $this->jsonResponse($response, $result['body'], $result['status']);
        }
    }

    /**
     * GET /api/myikea/customer/{id} - Obtiene customer por ID
     */
    public function getCustomerById(Request $request, Response $response, array $args): Response
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

            $customerId = $args['id'] ?? null;
            if (!$customerId) {
                throw new \Exception('Customer ID is required');
            }

            $customerModel = new Customer();
            $customer = $customerModel->getCustomerById($customerId);

            if (!$customer) {
                $result = ResponseFormatter::error('Customer not found', 404);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            $result = [
                'success' => true,
                'message' => 'Customer fetched successfully',
                'customer' => Customer::toDto($customer),
            ];

            return $this->jsonResponse($response, $result, 200);

        } catch (\Exception $e) {
            $result = ResponseFormatter::error($e->getMessage(), 400);
            return $this->jsonResponse($response, $result['body'], $result['status']);
        }
    }

    /**
     * GET /api/myikea/customer/search/firstName/{firstName} - Busca customers por nombre
     */
    public function searchByFirstName(Request $request, Response $response, array $args): Response
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

            $firstName = $args['firstName'] ?? null;
            if (!$firstName) {
                throw new \Exception('First name is required');
            }

            $customerModel = new Customer();
            $customers = $customerModel->searchByFirstName(urldecode($firstName));

            $result = [
                'success' => true,
                'message' => 'Search completed successfully',
                'customers' => array_map([Customer::class, 'toDto'], $customers),
                'totalResults' => count($customers),
            ];

            return $this->jsonResponse($response, $result, 200);

        } catch (\Exception $e) {
            $result = ResponseFormatter::error($e->getMessage(), 400);
            return $this->jsonResponse($response, $result['body'], $result['status']);
        }
    }

    /**
     * GET /api/myikea/customer/search/lastName/{lastName} - Busca customers por apellido
     */
    public function searchByLastName(Request $request, Response $response, array $args): Response
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

            $lastName = $args['lastName'] ?? null;
            if (!$lastName) {
                throw new \Exception('Last name is required');
            }

            $customerModel = new Customer();
            $customers = $customerModel->searchByLastName(urldecode($lastName));

            $result = [
                'success' => true,
                'message' => 'Search completed successfully',
                'customers' => array_map([Customer::class, 'toDto'], $customers),
                'totalResults' => count($customers),
            ];

            return $this->jsonResponse($response, $result, 200);

        } catch (\Exception $e) {
            $result = ResponseFormatter::error($e->getMessage(), 400);
            return $this->jsonResponse($response, $result['body'], $result['status']);
        }
    }

    /**
     * DELETE /api/myikea/customer/{id} - Elimina un customer
     */
    public function deleteCustomer(Request $request, Response $response, array $args): Response
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

            $customerId = $args['id'] ?? null;
            if (!$customerId) {
                throw new \Exception('Customer ID is required');
            }

            $customerModel = new Customer();
            $customer = $customerModel->getCustomerById($customerId);

            if (!$customer) {
                $result = ResponseFormatter::error('Customer not found', 404);
                return $this->jsonResponse($response, $result['body'], $result['status']);
            }

            $customerModel->deleteCustomer($customerId);

            $result = ResponseFormatter::success([], 'Customer deleted successfully', 200);
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
