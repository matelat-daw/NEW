<?php

namespace FuturePrograms\Clients\Middleware;

use Psr\Http\Message\ServerRequestInterface as Request;
use Psr\Http\Server\RequestHandlerInterface as RequestHandler;
use Slim\Psr7\Response;

class CorsMiddleware
{
    public function __invoke(Request $request, RequestHandler $handler): Response
    {
        $corsOrigins = 'http://localhost,http://127.0.0.1';
        $originList = explode(',', $corsOrigins);
        $origin = $request->getHeaderLine('Origin');
        
        // Permitir cualquier origen que coincida con la lista configurada
        $allowOrigin = '';
        foreach ($originList as $allowedOrigin) {
            $pattern = str_replace('*', '.*', trim($allowedOrigin));
            if (preg_match('#^' . $pattern . '$#', $origin)) {
                $allowOrigin = $origin;
                break;
            }
        }

        $response = $handler->handle($request);

        if ($allowOrigin) {
            $response = $response
                ->withHeader('Access-Control-Allow-Origin', $allowOrigin)
                ->withHeader('Access-Control-Allow-Credentials', 'true')
                ->withHeader('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, OPTIONS, PATCH')
                ->withHeader('Access-Control-Allow-Headers', 'Content-Type, Authorization, X-Requested-With')
                ->withHeader('Access-Control-Max-Age', '3600');
        }

        // Manejar OPTIONS request
        if ($request->getMethod() === 'OPTIONS') {
            return $response->withStatus(200);
        }

        return $response;
    }
}
