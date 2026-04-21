<?php

use FuturePrograms\Clients\Controllers\AuthController;
use FuturePrograms\Clients\Controllers\ProfileController;
use FuturePrograms\Clients\Controllers\UserController;
use FuturePrograms\Clients\Controllers\CustomerController;
use FuturePrograms\Clients\Controllers\ImageController;
use FuturePrograms\Clients\Utils\ImageHelper;
use FuturePrograms\Clients\Middleware\CorsMiddleware;
use FuturePrograms\Clients\Middleware\JwtMiddleware;
use FuturePrograms\Clients\Middleware\AdminMiddleware;
use FuturePrograms\Clients\Middleware\AdminOrPremiumMiddleware;
use Slim\Factory\AppFactory;

// Cargar autoloader
require __DIR__ . '/vendor/autoload.php';

// Crear aplicación
$app = AppFactory::create();

// Middleware para parsear JSON bodies automáticamente
$app->addBodyParsingMiddleware();

// Añadir middleware globales
$app->add(new CorsMiddleware());

// Middleware para manejo de excepciones
$errorMiddleware = $app->addErrorMiddleware(true, true, true);
$errorHandler = $errorMiddleware->getDefaultErrorHandler();
$errorHandler->forceContentType('application/json');

// ==================== RUTAS PÚBLICAS ====================

// Auth endpoints
$app->post('/api/auth/login', [AuthController::class, 'login']);
$app->get('/api/auth/verify/{token}', [AuthController::class, 'verifyEmail']);

// User endpoints - Register es público
$app->post('/api/user/register', [UserController::class, 'register']);

// ==================== RUTAS PROTEGIDAS ====================

// Group para rutas que requieren JWT
$jwtGroup = $app->group('', function ($group) {
    // Profile endpoints
    $group->get('/api/profile', [ProfileController::class, 'getProfile']);
    $group->put('/api/profile', [ProfileController::class, 'updateProfile']);
    $group->post('/api/profile/picture', [ProfileController::class, 'updateProfilePicture']);
    $group->put('/api/profile/password', [ProfileController::class, 'updatePassword']);
    $group->post('/api/profile/delete', [ProfileController::class, 'deleteProfile']);

    // User endpoints protegidos
    $group->get('/api/user', [UserController::class, 'getAllUsers']);
    $group->get('/api/user/{id}', [UserController::class, 'getUserById']);
});

// Agregar middleware JWT al grupo
$jwtGroup->add(new JwtMiddleware());

// Group para rutas ADMIN o PREMIUM
$customerGroup = $app->group('/api/myikea', function ($group) {
    // Rutas de búsqueda (más específicas, PRIMERO)
    $group->get('/customer/search/firstName/{firstName}', [CustomerController::class, 'searchByFirstName']);
    $group->get('/customer/search/lastName/{lastName}', [CustomerController::class, 'searchByLastName']);

    // Rutas genéricas (menos específicas, después)
    $group->get('/customer/{id}', [CustomerController::class, 'getCustomerById']);
    $group->delete('/customer/{id}', [CustomerController::class, 'deleteCustomer']);
    $group->get('/customer', [CustomerController::class, 'getAllCustomers']);
});

// Agregar JWT + AdminOrPremium middleware
$customerGroup->add(new AdminOrPremiumMiddleware());
$customerGroup->add(new JwtMiddleware());

// Image endpoint - requerido por frontend para profileImg
$app->get('/api/images/{path:.+}', [ImageController::class, 'serveImage'])->setName('serveImage');

// ==================== HEALTH CHECK ====================

$app->get('/api/health', function ($request, $response) {
    $response->getBody()->write(json_encode(['status' => 'ok']));
    return $response->withHeader('Content-Type', 'application/json');
});

// ==================== FRONTEND - Servir index.html ====================

// Ruta catch-all: Servir index.html para todas las rutas que no sean API
$app->get('/{routes:.+}', function ($request, $response, $args) {
    $indexPath = __DIR__ . '/index.html';
    if (file_exists($indexPath)) {
        return $response
            ->withHeader('Content-Type', 'text/html; charset=utf-8')
            ->withBody(new \Slim\Psr7\Stream(fopen($indexPath, 'r')));
    }
    return $response->withStatus(404);
});

// Ruta raíz: Servir index.html
$app->get('/', function ($request, $response) {
    $indexPath = __DIR__ . '/index.html';
    if (file_exists($indexPath)) {
        return $response
            ->withHeader('Content-Type', 'text/html; charset=utf-8')
            ->withBody(new \Slim\Psr7\Stream(fopen($indexPath, 'r')));
    }
    return $response->withStatus(404);
});

$app->options('/{routes:.+}', function ($request, $response) {
    return $response;
});

// Ejecutar aplicación
$app->run();