package com.futureprograms.clients.controller;

import com.futureprograms.clients.config.JwtProvider;
import com.futureprograms.clients.dto.LoginRequest;
import com.futureprograms.clients.dto.UserDto;
import com.futureprograms.clients.entity.User;
import com.futureprograms.clients.service.UserService;
import com.futureprograms.clients.util.ApiConstants;
import com.futureprograms.clients.util.AuthenticationHelper;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.*;
import jakarta.servlet.http.HttpServletResponse;
import java.io.IOException;
import java.util.Map;

/**
 * Controlador optimizado de autenticación
 * Maneja login, logout, verificación de email y operaciones de auth
 */
@RestController
@RequestMapping(ApiConstants.AUTH_ENDPOINT)
@Slf4j
@RequiredArgsConstructor
public class AuthController {

    private final UserService userService;
    private final JwtProvider jwtProvider;
    private final AuthenticationHelper authenticationHelper;

    @Value("${app.frontend.login-url:http://localhost/login}")
    private String frontendLoginUrl;

    @Value("${app.frontend.register-url:http://localhost/register}")
    private String frontendRegisterUrl;

    /**
     * POST /api/auth/login - Autentica al usuario
     */
    @PostMapping("/login")
    public ResponseEntity<?> login(
            @Valid @RequestBody LoginRequest request,
            HttpServletResponse response
    ) {
        if (!userService.validateCredentials(request.getEmail(), request.getPassword())) {
            throw new IllegalArgumentException(ApiConstants.ERR_INVALID_CREDENTIALS);
        }

        User user = userService.getUserByEmail(request.getEmail())
                .orElseThrow(() -> new IllegalStateException(ApiConstants.ERR_USER_NOT_FOUND));

        String token = jwtProvider.generateToken(user.getEmail());
        addAuthCookie(response, token);

        authenticationHelper.logAuthEvent(user.getEmail(), "login_successful");

        // Retornar token en el body + datos del usuario como data
        // Frontend extrae el token de response.token
        return ResponseEntity.ok()
                .body(Map.of(
                        "success", true,
                        "message", ApiConstants.MSG_LOGIN_SUCCESS,
                        "token", token,
                        "data", UserDto.fromEntity(user)
                ));
    }

    /**
     * GET /api/auth/verify/{token} - Verifica el email del usuario
     */
    @GetMapping("/verify/{token}")
    public void verifyEmail(@PathVariable String token, HttpServletResponse response) throws IOException {
        try {
            userService.verifyEmail(token);
            response.sendRedirect(frontendLoginUrl + "?verified=1");
        } catch (Exception e) {
            log.error("Error en verificación: {}", e.getMessage());
            response.sendRedirect(frontendRegisterUrl + "?verification=failed");
        }
    }

    /**
     * POST /api/auth/refresh - Refresca el JWT token
     */
    @PostMapping("/refresh")
    public ResponseEntity<?> refreshToken(
            Authentication authentication,
            HttpServletResponse response
    ) {
        User user = authenticationHelper.requireAuthenticatedUser(authentication);

        String newToken = jwtProvider.generateToken(user.getEmail());
        addAuthCookie(response, newToken);

        authenticationHelper.logAuthEvent(user.getEmail(), "token_refreshed");

        return ResponseEntity.ok(Map.of(
                "success", true,
                "message", ApiConstants.MSG_TOKEN_REFRESHED,
                "data", Map.of("token", newToken)
        ));
    }

    /**
     * POST /api/auth/logout - Cierra la sesión del usuario
     */
    @PostMapping("/logout")
    public ResponseEntity<?> logout(HttpServletResponse response) {
        removeAuthCookie(response);
        log.info("Logout realizado");

        return ResponseEntity.ok(Map.of(
                "success", true,
                "message", "Logout exitoso"
        ));
    }

    /**
     * Agrega cookie de autenticación segura
     */
    private void addAuthCookie(HttpServletResponse response, String token) {
        // En Spring Boot 3+ (Jakarta), es mejor usar un header Set-Cookie manual 
        // para tener control total sobre SameSite y asegurar que se use el mismo formato.
        String cookieHeader = String.format(
                "%s=%s; Path=%s; Max-Age=%d; HttpOnly; SameSite=%s",
                ApiConstants.JWT_COOKIE_NAME, token, ApiConstants.JWT_COOKIE_PATH,
                ApiConstants.JWT_COOKIE_MAX_AGE, ApiConstants.JWT_SAME_SITE
        );
        
        // Agregar flag Secure si no es localhost o si se desea forzar
        // (En desarrollo local a veces se prefiere false para evitar problemas con HTTP)
        // cookieHeader += "; Secure"; 
        
        response.addHeader("Set-Cookie", cookieHeader);
    }

    /**
     * Elimina la cookie de autenticación
     */
    private void removeAuthCookie(HttpServletResponse response) {
        String cookieHeader = String.format(
                "%s=; Path=%s; Max-Age=0; HttpOnly; SameSite=%s",
                ApiConstants.JWT_COOKIE_NAME, ApiConstants.JWT_COOKIE_PATH,
                ApiConstants.JWT_SAME_SITE
        );
        response.addHeader("Set-Cookie", cookieHeader);
    }
}
