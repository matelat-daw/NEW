package com.futureprograms.clients.config;

import com.futureprograms.clients.util.ApiConstants;
import com.futureprograms.clients.util.ApiResponse;
import com.futureprograms.clients.util.ApiResponseBuilder;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.AccessDeniedException;
import org.springframework.security.core.AuthenticationException;
import org.springframework.web.HttpRequestMethodNotSupportedException;
import org.springframework.web.bind.MethodArgumentNotValidException;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;
import org.springframework.web.servlet.NoHandlerFoundException;

/**
 * Manejador global de excepciones centralizado
 * Proporciona respuestas JSON consistentes para todos los errores
 */
@RestControllerAdvice
@Slf4j
public class GlobalExceptionHandler {

    @ExceptionHandler(HttpRequestMethodNotSupportedException.class)
    public ResponseEntity<ApiResponse<Void>> handleHttpRequestMethodNotSupported(HttpRequestMethodNotSupportedException ex) {
        log.error("Método HTTP no soportado: {}", ex.getMessage());
        return ApiResponseBuilder.error(HttpStatus.METHOD_NOT_ALLOWED, ApiConstants.ERR_METHOD_NOT_ALLOWED);
    }

    @ExceptionHandler(AccessDeniedException.class)
    public ResponseEntity<ApiResponse<Void>> handleAccessDeniedException(AccessDeniedException ex) {
        log.error("Acceso denegado: {}", ex.getMessage());
        return ApiResponseBuilder.forbidden(ApiConstants.ERR_FORBIDDEN);
    }

    @ExceptionHandler(AuthenticationException.class)
    public ResponseEntity<ApiResponse<Void>> handleAuthenticationException(AuthenticationException ex) {
        log.error("Error de autenticación: {}", ex.getMessage());
        return ApiResponseBuilder.unauthorized(ApiConstants.ERR_UNAUTHORIZED);
    }

    @ExceptionHandler(MethodArgumentNotValidException.class)
    public ResponseEntity<ApiResponse<Void>> handleValidationExceptions(MethodArgumentNotValidException ex) {
        String errorMessage = ex.getBindingResult().getFieldErrors().stream()
                .map(error -> error.getField() + ": " + error.getDefaultMessage())
                .collect(java.util.stream.Collectors.joining(", "));
        log.error("Error de validación: {}", errorMessage);
        return ApiResponseBuilder.badRequest("Error de validación: " + errorMessage);
    }

    @ExceptionHandler(NoHandlerFoundException.class)
    public ResponseEntity<ApiResponse<Void>> handleNoHandlerFoundException(NoHandlerFoundException ex) {
        log.error("Ruta no encontrada: {}", ex.getRequestURL());
        return ApiResponseBuilder.notFound(ApiConstants.ERR_NOT_FOUND);
    }

    @ExceptionHandler(IllegalArgumentException.class)
    public ResponseEntity<ApiResponse<Void>> handleIllegalArgumentException(IllegalArgumentException ex) {
        log.error("Argumento inválido: {}", ex.getMessage());
        return ApiResponseBuilder.badRequest(ex.getMessage());
    }

    @ExceptionHandler(IllegalStateException.class)
    public ResponseEntity<ApiResponse<Void>> handleIllegalStateException(IllegalStateException ex) {
        log.error("Estado inválido: {}", ex.getMessage());
        return ApiResponseBuilder.badRequest(ex.getMessage());
    }

    @ExceptionHandler(org.springframework.web.bind.MissingServletRequestParameterException.class)
    public ResponseEntity<ApiResponse<Void>> handleMissingServletRequestParameter(org.springframework.web.bind.MissingServletRequestParameterException ex) {
        log.error("Parámetro faltante: {}", ex.getParameterName());
        return ApiResponseBuilder.badRequest("Parámetro requerido: " + ex.getParameterName());
    }

    @ExceptionHandler(org.springframework.dao.DataIntegrityViolationException.class)
    public ResponseEntity<ApiResponse<Void>> handleDataIntegrityViolation(org.springframework.dao.DataIntegrityViolationException ex) {
        log.error("Error de integridad de datos: {}", ex.getMessage());
        return ApiResponseBuilder.badRequest("Error de integridad: Ya existe un registro con esos datos únicos.");
    }

    @ExceptionHandler(RuntimeException.class)
    public ResponseEntity<ApiResponse<Void>> handleRuntimeException(RuntimeException ex) {
        log.error("Error de runtime: {}", ex.getMessage(), ex);
        return ApiResponseBuilder.internalServerError(ApiConstants.ERR_INTERNAL_ERROR + ": " + ex.getMessage());
    }

    @ExceptionHandler(Exception.class)
    public ResponseEntity<ApiResponse<Void>> handleGlobalException(Exception ex) {
        log.error("Error no controlado: {} ({})", ex.getMessage(), ex.getClass().getName(), ex);
        return ApiResponseBuilder.internalServerError(ApiConstants.ERR_INTERNAL_ERROR + " [" + ex.getClass().getSimpleName() + "]: " + ex.getMessage());
    }
}
