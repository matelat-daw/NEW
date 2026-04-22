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
use Slim\Psr7\Stream;

// Cargar autoloader
require __DIR__ . '/vendor/autoload.php';

// Crear aplicación
$app = AppFactory::create();

// Middleware para parsear JSON bodies automáticamente
$app->addBodyParsingMiddleware();

// Middleware para manejo de excepciones (debe estar antes de middleware de negocio)
$errorMiddleware = $app->addErrorMiddleware(true, true, true);
$errorHandler = $errorMiddleware->getDefaultErrorHandler();
$errorHandler->forceContentType('application/json');

// Añadir middleware globales (CORS al final, se ejecuta primero)
$app->add(new CorsMiddleware());

// ==================== RUTAS PÚBLICAS ====================

// Auth endpoints
$app->post('/api/auth/login', [AuthController::class, 'login']);
$app->get('/api/auth/verify/{token}', [AuthController::class, 'verifyEmail']);

// User endpoints - Register es público
$app->post('/api/user/register', [UserController::class, 'register']);

// Health check
$app->get('/api/health', function ($request, $response) {
    $response->getBody()->write(json_encode(['status' => 'ok']));
    return $response->withHeader('Content-Type', 'application/json');
});

// ==================== RUTAS PROTEGIDAS CON JWT ====================

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

// Agregar middleware JWT al grupo (orden correcto: JWT primero)
$jwtGroup->add(new JwtMiddleware());

// ==================== RUTAS ADMIN O PREMIUM ====================

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

// Agregar middleware al grupo (JWT primero, luego rol - orden correcto LIFO)
$customerGroup->add(new AdminOrPremiumMiddleware());
$customerGroup->add(new JwtMiddleware());

// ==================== RUTAS DE IMÁGENES ====================

// Image endpoint - requerido por frontend para profileImg (sin protección para cargar desde frontend)
$app->get('/api/images/{path:.+}', [ImageController::class, 'serveImage'])->setName('serveImage');

// ==================== FRONTEND - Servir SPA ====================

// Ruta raíz
$app->get('/', function ($request, $response) {
    $indexPath = __DIR__ . '/index.html';
    if (file_exists($indexPath)) {
        return $response
            ->withHeader('Content-Type', 'text/html; charset=utf-8')
            ->withBody(new Stream(fopen($indexPath, 'r')));
    }
    return $response->withStatus(404);
});

// Ruta catch-all: Servir index.html para todas las rutas no-API (ÚLTIMA RUTA)
$app->get('/{routes:.+}', function ($request, $response, $args) {
    $indexPath = __DIR__ . '/index.html';
    if (file_exists($indexPath)) {
        return $response
            ->withHeader('Content-Type', 'text/html; charset=utf-8')
            ->withBody(new Stream(fopen($indexPath, 'r')));
    }
    return $response->withStatus(404);
});

$app->options('/{routes:.+}', function ($request, $response) {
    return $response;
});

// Ejecutar aplicación
$app->run();