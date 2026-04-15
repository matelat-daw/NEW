<?php

namespace FuturePrograms\Clients\Controllers;

use FuturePrograms\Clients\Utils\ImageHelper;
use Psr\Http\Message\ServerRequestInterface as Request;
use Psr\Http\Message\ResponseInterface as Response;

class ImageController
{
    /**
     * GET /api/images/** - Sirve imágenes
     */
    public function serveImage(Request $request, Response $response, array $args): Response
    {
        try {
            $path = $args['path'] ?? null;

            if (!$path || $path === 'health') {
                // Health check
                $response->getBody()->write(json_encode(['status' => 'ok']));
                return $response
                    ->withHeader('Content-Type', 'application/json')
                    ->withStatus(200);
            }

            $imageInfo = ImageHelper::serveImage($path);

            if ($imageInfo === false) {
                $response->getBody()->write(json_encode(['error' => 'Image not found: ' . $path]));
                return $response
                    ->withHeader('Content-Type', 'application/json')
                    ->withStatus(404);
            }

            // Servir la imagen
            $stream = fopen($imageInfo['path'], 'rb');
            if (!$stream) {
                throw new \Exception('Cannot open file');
            }

            return $response
                ->withHeader('Content-Type', $imageInfo['mimeType'])
                ->withHeader('Content-Disposition', 'inline; filename="' . $imageInfo['filename'] . '"')
                ->withBody(new \Slim\Psr7\Stream($stream))
                ->withStatus(200);

        } catch (\Exception $e) {
            $response->getBody()->write(json_encode(['error' => 'Error serving image: ' . $e->getMessage()]));
            return $response
                ->withHeader('Content-Type', 'application/json')
                ->withStatus(500);
        }
    }
}
