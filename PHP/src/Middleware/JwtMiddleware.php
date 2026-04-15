<?php

namespace FuturePrograms\Clients\Middleware;

use FuturePrograms\Clients\Utils\JwtHelper;
use FuturePrograms\Clients\Utils\ResponseFormatter;
use Psr\Http\Message\ServerRequestInterface as Request;
use Psr\Http\Server\RequestHandlerInterface as RequestHandler;
use Slim\Psr7\Response;

class JwtMiddleware
{
    /**
     * Middleware para validar JWT token
     */
    public function __invoke(Request $request, RequestHandler $handler): Response
    {
        $token = JwtHelper::getTokenFromRequest($request);

        if (!$token) {
            $response = new Response();
            $response->getBody()->write(json_encode(ResponseFormatter::error('Token not found', 401)['body']));
            return $response
                ->withHeader('Content-Type', 'application/json')
                ->withStatus(401);
        }

        $decoded = JwtHelper::validateToken($token);

        if (!$decoded) {
            $response = new Response();
            $response->getBody()->write(json_encode(ResponseFormatter::error('Invalid token', 401)['body']));
            return $response
                ->withHeader('Content-Type', 'application/json')
                ->withStatus(401);
        }

        // Almacenar información del usuario en el request
        $request = $request
            ->withAttribute('token', $token)
            ->withAttribute('user', (array)$decoded);

        return $handler->handle($request);
    }
}

