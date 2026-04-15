package com.futureprograms.clients.controller;

import com.futureprograms.clients.dto.ProfileDeleteRequest;
import com.futureprograms.clients.dto.UpdatePasswordRequest;
import com.futureprograms.clients.dto.UpdateProfileRequest;
import com.futureprograms.clients.dto.UserDto;
import com.futureprograms.clients.entity.User;
import com.futureprograms.clients.service.ImageService;
import com.futureprograms.clients.service.UserService;
import com.futureprograms.clients.util.ApiConstants;
import com.futureprograms.clients.util.AuthenticationHelper;
import com.futureprograms.clients.util.ValidationHelper;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;
import java.util.Map;

/**
 * Controlador optimizado para gestión del perfil del usuario
 * Todas las responsabilidades comunes han sido centralizadas en servicios y helpers
 */
@RestController
@RequestMapping(ApiConstants.PROFILE_ENDPOINT)
@Slf4j
@RequiredArgsConstructor
public class ProfileController {

    private final UserService userService;
    private final ImageService imageService;
    private final AuthenticationHelper authenticationHelper;

    /**
     * GET /api/profile - Obtiene el perfil del usuario autenticado
     */
    @GetMapping
    public ResponseEntity<?> getProfile(Authentication authentication) {
        User user = authenticationHelper.requireAuthenticatedUser(authentication);
        authenticationHelper.logAuthEvent(user.getEmail(), "profile_retrieved");
        
        // Devolver estructura plana: success, data = usuario
        return ResponseEntity.ok(Map.of(
                "success", true,
                "message", ApiConstants.MSG_PROFILE_FETCHED,
                "data", UserDto.fromEntity(user)
        ));
    }

    /**
     * PUT /api/profile - Actualiza datos del perfil
     */
    @PutMapping
    public ResponseEntity<?> updateProfile(
            Authentication authentication,
            @Valid @RequestBody UpdateProfileRequest request
    ) {
        User user = authenticationHelper.requireAuthenticatedUser(authentication);
        
        User updatedUser = userService.updateUserProfile(
                user.getId(),
                request.getName(),
                request.getSurname1(),
                request.getSurname2(),
                request.getPhone()
        );

        authenticationHelper.logAuthEvent(user.getEmail(), "profile_updated");
        
        return ResponseEntity.ok(Map.of(
                "success", true,
                "message", ApiConstants.MSG_PROFILE_UPDATED,
                "data", UserDto.fromEntity(updatedUser)
        ));
    }

    /**
     * POST /api/profile/picture - Sube nueva foto de perfil (MultipartFile)
     */
    @PostMapping(value = "/picture", consumes = MediaType.MULTIPART_FORM_DATA_VALUE)
    public ResponseEntity<?> updateProfilePicture(
            Authentication authentication,
            @RequestParam("profilePicture") MultipartFile file
    ) {
        User user = authenticationHelper.requireAuthenticatedUser(authentication);
        
        // Validar imagen
        ValidationHelper.validateImageFile(file);

        try {
            imageService.ensureUserImageDirectory(user.getId());
            String fileName = imageService.saveProfileImage(file, user.getId());
            User updatedUser = userService.updateProfileImage(user.getId(), fileName);

            authenticationHelper.logAuthEvent(user.getEmail(), "profile_picture_updated");
            
            return ResponseEntity.ok(Map.of(
                    "success", true,
                    "message", ApiConstants.MSG_PROFILE_PICTURE_UPDATED,
                    "data", UserDto.fromEntity(updatedUser)
            ));
        } catch (Exception e) {
            log.error("Error al guardar imagen: {}", e.getMessage());
            throw new RuntimeException(ApiConstants.ERR_IMAGE_SAVE_FAILED);
        }
    }

    /**
     * PUT /api/profile/password - Cambia la contraseña del usuario
     */
    @PutMapping("/password")
    public ResponseEntity<?> updatePassword(
            Authentication authentication,
            @Valid @RequestBody UpdatePasswordRequest request
    ) {
        User user = authenticationHelper.requireAuthenticatedUser(authentication);
        
        if (!request.getNewPassword().equals(request.getConfirmPassword())) {
            throw new IllegalArgumentException("Las nuevas contraseñas no coinciden");
        }

        User updatedUser = userService.changePassword(
                user.getId(),
                request.getCurrentPassword(),
                request.getNewPassword()
        );

        authenticationHelper.logAuthEvent(user.getEmail(), "password_changed");
        
        return ResponseEntity.ok(Map.of(
                "success", true,
                "message", ApiConstants.MSG_PASSWORD_UPDATED,
                "data", UserDto.fromEntity(updatedUser)
        ));
    }

    /**
     * POST /api/profile/delete - Elimina la cuenta del usuario
     */
    @PostMapping("/delete")
    public ResponseEntity<?> deleteAccount(
            Authentication authentication,
            @Valid @RequestBody ProfileDeleteRequest request
    ) {
        User user = authenticationHelper.requireAuthenticatedUser(authentication);
        
        // Eliminar todas las imágenes del usuario (directorio completo)
        try {
            imageService.deleteUserProfileImages(user.getId());
            log.info("✅ Carpeta de imágenes del usuario {} eliminada", user.getId());
        } catch (Exception e) {
            log.warn("⚠️ No se pudo eliminar imágenes del usuario {}: {}", user.getId(), e.getMessage());
        }

        userService.deleteUserAccount(user.getId(), request.getPassword());

        authenticationHelper.logAuthEvent(user.getEmail(), "account_deleted");
        
        return ResponseEntity.ok(Map.of(
                "success", true,
                "message", ApiConstants.MSG_PROFILE_DELETED
        ));
    }

    /**
     * POST /api/profile/validate-password - Valida la contraseña del usuario
     */
    @PostMapping("/validate-password")
    public ResponseEntity<?> validatePassword(
            Authentication authentication,
            @RequestBody Map<String, String> request
    ) {
        User user = authenticationHelper.requireAuthenticatedUser(authentication);
        
        String password = request.get("password");
        if (!ValidationHelper.isValidString(password)) {
            throw new IllegalArgumentException("La contraseña es requerida");
        }

        boolean isValid = userService.validateCredentials(user.getEmail(), password);
        
        return ResponseEntity.ok(Map.of(
                "success", true,
                "message", "Validación completada",
                "data", Map.of("valid", isValid)
        ));
    }
}