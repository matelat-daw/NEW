package com.futureprograms.clients.util;

import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.security.core.Authentication;
import org.springframework.stereotype.Component;
import com.futureprograms.clients.entity.User;
import com.futureprograms.clients.service.UserService;

import java.util.Optional;

/**
 * Utilidad centralizada para operaciones relacionadas con autenticación
 * Elimina validaciones repetidas en los controllers
 */
@Component
@Slf4j
@RequiredArgsConstructor
public class AuthenticationHelper {

    private final UserService userService;

    /**
     * Valida que el usuario esté autenticado
     * @return true si está autenticado, false si no
     */
    public boolean isAuthenticated(Authentication authentication) {
        return authentication != null && authentication.isAuthenticated();
    }

    /**
     * Obtiene el email del usuario autenticado
     */
    public String getAuthenticatedEmail(Authentication authentication) {
        if (!isAuthenticated(authentication)) {
            return null;
        }
        return authentication.getName();
    }

    /**
     * Obtiene el usuario autenticado actual
     */
    public Optional<User> getAuthenticatedUser(Authentication authentication) {
        String email = getAuthenticatedEmail(authentication);
        if (email == null) {
            return Optional.empty();
        }
        return userService.getUserByEmail(email);
    }

    /**
     * Valida autenticación y retorna el usuario o lanza excepción
     */
    public User requireAuthenticatedUser(Authentication authentication) {
        return getAuthenticatedUser(authentication)
                .orElseThrow(() -> new IllegalStateException(ApiConstants.ERR_USER_NOT_FOUND));
    }

    /**
     * Log de evento de autenticación
     */
    public void logAuthEvent(String email, String action) {
        log.info("Auth event for user '{}': {}", email, action);
    }

    /**
     * Log de intento de acceso no autorizado
     */
    public void logUnauthorizationAttempt(String reason) {
        log.warn("Unauthorized access attempt: {}", reason);
    }
}
