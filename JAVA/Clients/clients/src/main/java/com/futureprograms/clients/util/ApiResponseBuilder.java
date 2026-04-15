package com.futureprograms.clients.util;

import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;

/**
 * Utilidad para construir respuestas HTTP consistentes
 * Implementa patrón Builder para mejor legibilidad
 */
public final class ApiResponseBuilder {

    private ApiResponseBuilder() {
        throw new AssertionError("No se puede instanciar esta clase");
    }

    /**
     * Crea una respuesta exitosa
     */
    public static <T> ResponseEntity<ApiResponse<T>> success(String message, T data) {
        return ResponseEntity.ok(ApiResponse.<T>ok(message, data));
    }

    /**
     * Crea una respuesta exitosa sin datos
     */
    public static <T> ResponseEntity<ApiResponse<T>> success(String message) {
        return ResponseEntity.ok(ApiResponse.ok(message, null));
    }

    /**
     * Crea una respuesta de creado (201)
     */
    public static <T> ResponseEntity<ApiResponse<T>> created(String message, T data) {
        return ResponseEntity.status(HttpStatus.CREATED).body(ApiResponse.ok(message, data));
    }

    /**
     * Crea una respuesta de error 400
     */
    public static <T> ResponseEntity<ApiResponse<T>> badRequest(String message) {
        return ResponseEntity.status(HttpStatus.BAD_REQUEST)
                .body(ApiResponse.<T>error(HttpStatus.BAD_REQUEST, message));
    }

    /**
     * Crea una respuesta de error 401
     */
    public static <T> ResponseEntity<ApiResponse<T>> unauthorized(String message) {
        return ResponseEntity.status(HttpStatus.UNAUTHORIZED)
                .body(ApiResponse.<T>error(HttpStatus.UNAUTHORIZED, message));
    }

    /**
     * Crea una respuesta de error 403
     */
    public static <T> ResponseEntity<ApiResponse<T>> forbidden(String message) {
        return ResponseEntity.status(HttpStatus.FORBIDDEN)
                .body(ApiResponse.<T>error(HttpStatus.FORBIDDEN, message));
    }

    /**
     * Crea una respuesta de error 404
     */
    public static <T> ResponseEntity<ApiResponse<T>> notFound(String message) {
        return ResponseEntity.status(HttpStatus.NOT_FOUND)
                .body(ApiResponse.<T>error(HttpStatus.NOT_FOUND, message));
    }

    /**
     * Crea una respuesta de error 500
     */
    public static <T> ResponseEntity<ApiResponse<T>> internalServerError(String message) {
        return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR)
                .body(ApiResponse.<T>error(HttpStatus.INTERNAL_SERVER_ERROR, message));
    }

    /**
     * Crea una respuesta de error genérica
     */
    public static <T> ResponseEntity<ApiResponse<T>> error(HttpStatus status, String message) {
        return ResponseEntity.status(status).body(ApiResponse.error(status, message));
    }
}
