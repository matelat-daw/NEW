<?php

namespace FuturePrograms\Clients\Middleware;

use FuturePrograms\Clients\Utils\ResponseFormatter;
use Psr\Http\Message\ServerRequestInterface as Request;
use Psr\Http\Server\RequestHandlerInterface as RequestHandler;
use Slim\Psr7\Response;

/**
 * Middleware para validar rol de ADMIN o PREMIUM
 */
class AdminOrPremiumMiddleware
{
    public function __invoke(Request $request, RequestHandler $handler): Response
    {
        $user = $request->getAttribute('user');

        if (!$user || !in_array($user['role'], ['ADMIN', 'PREMIUM'])) {
            $response = new Response();
            $response->getBody()->write(json_encode(ResponseFormatter::error('Unauthorized: Admin or Premium access required', 403)['body']));
            return $response
                ->withHeader('Content-Type', 'application/json')
                ->withStatus(403);
        }

        return $handler->handle($request);
    }
}
