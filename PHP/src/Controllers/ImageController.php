<?php

namespace FuturePrograms\Clients\Controllers;

use FuturePrograms\Clients\Utils\ImageHelper;
use Psr\Http\Message\ServerRequestInterface as Request;
use Psr\Http\Message\ResponseInterface as Response;

class ImageController
{
    public function serveImage(Request $request, Response $response, array $args): Response
    {
        $path = $args['path'] ?? null;

        // Health check
        if ($path === 'health') {
            $body = fopen('php://temp', 'r+');
            fwrite($body, json_encode(['status' => 'ok']));
            rewind($body);
            
            return $response
                ->withHeader('Content-Type', 'application/json')
                ->withStatus(200)
                ->withBody(new \Slim\Psr7\Stream($body));
        }

        // Validar path
        if (!$path) {
            $body = fopen('php://temp', 'r+');
            fwrite($body, json_encode(['error' => 'Missing image path']));
            rewind($body);
            
            return $response
                ->withHeader('Content-Type', 'application/json')
                ->withStatus(400)
                ->withBody(new \Slim\Psr7\Stream($body));
        }

        try {
            // Crear ImageHelper para servir la imagen
            $uploadsDir = dirname(dirname(__DIR__)) . DIRECTORY_SEPARATOR . 'uploads';
            $imageHelper = new ImageHelper($uploadsDir);
            
            // Obtener metadata de la imagen
            $imageInfo = $imageHelper->serveImage($path);
            
            if ($imageInfo === false) {
                $body = fopen('php://temp', 'r+');
                fwrite($body, json_encode(['error' => 'Image not found: ' . $path]));
                rewind($body);
                
                return $response
                    ->withHeader('Content-Type', 'application/json')
                    ->withStatus(404)
                    ->withBody(new \Slim\Psr7\Stream($body));
            }

            // 🌐 Cabeceras de Caché & Condicionales
            $etag     = '"' . md5_file($imageInfo['path']) . '"';
            $modified = date('D, d M Y H:i:s', filemtime($imageInfo['path'])) . ' GMT';

            // Si el cliente tiene la misma versión, no enviar contenido (304)
            if ($request->getHeaderLine('If-None-Match') === $etag || 
                $request->getHeaderLine('If-Modified-Since') === $modified) {
                return $response->withStatus(304);
            }

            // 📦 Servir imagen con headers de caché
            $stream = fopen($imageInfo['path'], 'rb');
            if (!$stream) {
                throw new \RuntimeException('Failed to open image file');
            }

            return $response
                ->withHeader('Content-Type', $imageInfo['mimeType'])
                ->withHeader('Content-Disposition', 'inline; filename="' . $imageInfo['filename'] . '"')
                ->withHeader('Cache-Control', 'public, max-age=2592000') // 30 días
                ->withHeader('ETag', $etag)
                ->withHeader('Last-Modified', $modified)
                ->withBody(new \Slim\Psr7\Stream($stream))
                ->withStatus(200);

        } catch (\Exception $e) {
            $body = fopen('php://temp', 'r+');
            fwrite($body, json_encode(['error' => 'Error serving image: ' . $e->getMessage()]));
            rewind($body);
            
            return $response
                ->withHeader('Content-Type', 'application/json')
                ->withStatus(500)
                ->withBody(new \Slim\Psr7\Stream($body));
        }
    }
}

