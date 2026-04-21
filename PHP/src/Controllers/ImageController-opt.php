<?php

namespace FuturePrograms\Clients\Controllers;

use FuturePrograms\Clients\Utils\ImageHelper;
use Psr\Http\Message\ServerRequestInterface as Request;
use Psr\Http\Message\ResponseInterface as Response;

class ImageController
{
    private string $baseImagePath;

    public function __construct(string $baseImagePath)
    {
        $this->baseImagePath = realpath($baseImagePath);
    }

    public function serveImage(Request $request, Response $response, array $args): Response
    {
        $path = $args['path'] ?? null;

        // Health check
        if ($path === 'health') {
            return $response
                ->withHeader('Content-Type', 'application/json')
                ->withStatus(200)
                ->withBody(new \Slim\Psr7\Stream(fopen('php://temp', 'r+')))
                ->getBody()->write(json_encode(['status' => 'ok']));
        }

        if (!$path) {
            return $response
                ->withHeader('Content-Type', 'application/json')
                ->withStatus(400)
                ->withBody(new \Slim\Psr7\Stream(fopen('php://temp', 'r+')))
                ->getBody()->write(json_encode(['error' => 'Missing image path']));
        }

        // 🔒 Seguridad: Prevenir Path Traversal
        $safeFilename = basename($path);
        $fullPath     = $this->baseImagePath . DIRECTORY_SEPARATOR . $safeFilename;
        $realPath     = realpath($fullPath);

        if ($realPath === false || strpos($realPath, $this->baseImagePath) !== 0) {
            return $response
                ->withHeader('Content-Type', 'application/json')
                ->withStatus(403)
                ->withBody(new \Slim\Psr7\Stream(fopen('php://temp', 'r+')))
                ->getBody()->write(json_encode(['error' => 'Access denied or file not found']));
        }

        // Obtener metadata (debería incluir mimeType)
        $imageInfo = ImageHelper::serveImage($realPath);
        if ($imageInfo === false || !isset($imageInfo['mimeType'])) {
            return $response
                ->withHeader('Content-Type', 'application/json')
                ->withStatus(404)
                ->withBody(new \Slim\Psr7\Stream(fopen('php://temp', 'r+')))
                ->getBody()->write(json_encode(['error' => 'Invalid or missing image metadata']));
        }

        // 🌐 Cabeceras de Caché & Condicionales
        $etag     = '"' . md5_file($realPath) . '"';
        $modified = date('D, d M Y H:i:s', filemtime($realPath)) . ' GMT';

        if ($request->getHeaderLine('If-None-Match') === $etag || 
            $request->getHeaderLine('If-Modified-Since') === $modified) {
            return $response->withStatus(304);
        }

        // 📦 Servir imagen
        $stream = fopen($realPath, 'rb');
        if (!$stream) {
            throw new \RuntimeException('Failed to open image file');
        }

        return $response
            ->withHeader('Content-Type', $imageInfo['mimeType'])
            ->withHeader('Content-Disposition', 'inline; filename="' . $safeFilename . '"')
            ->withHeader('Cache-Control', 'public, max-age=2592000') // 30 días
            ->withHeader('ETag', $etag)
            ->withHeader('Last-Modified', $modified)
            ->withBody(new \Slim\Psr7\Stream($stream))
            ->withStatus(200);
    }
}