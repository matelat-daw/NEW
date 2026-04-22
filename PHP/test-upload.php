<?php
/**
 * Test de upload - Verificar que todo funciona
 */

// Responder con CORS headers
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type');
header('Content-Type: application/json');

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    die(json_encode(['status' => 'ok']));
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $response = [
        'status' => 'Test Upload Endpoint',
        'php_info' => [
            'upload_max_filesize' => ini_get('upload_max_filesize'),
            'post_max_size' => ini_get('post_max_size'),
            'memory_limit' => ini_get('memory_limit'),
        ],
        'files_received' => $_FILES,
        'file_count' => count($_FILES),
    ];

    if (!empty($_FILES['profilePicture'])) {
        $file = $_FILES['profilePicture'];
        $response['file_details'] = [
            'name' => $file['name'],
            'type' => $file['type'],
            'size' => $file['size'],
            'tmp_name' => $file['tmp_name'],
            'error' => $file['error'],
            'is_uploaded' => is_uploaded_file($file['tmp_name']),
            'file_exists' => file_exists($file['tmp_name']),
            'mime_type' => mime_content_type($file['tmp_name']),
        ];
    }

    echo json_encode($response, JSON_PRETTY_PRINT | JSON_UNESCAPED_SLASHES);
}
?>
