package com.futureprograms.clients.service;

import com.futureprograms.clients.dto.RegisterRequest;
import com.futureprograms.clients.entity.RoleEntity;
import com.futureprograms.clients.entity.User;
import com.futureprograms.clients.enums.Gender;
import com.futureprograms.clients.enums.Role;
import com.futureprograms.clients.repository.RoleRepository;
import com.futureprograms.clients.repository.UserRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;
import java.time.LocalDateTime;
import java.util.HashSet;
import java.util.Optional;
import java.util.Set;
import java.util.UUID;

@Service
@Slf4j
@RequiredArgsConstructor
public class UserService {

    private final UserRepository userRepository;
    private final PasswordEncoder passwordEncoder;
    private final RoleRepository roleRepository;
    private final EmailService emailService;
    private final ImageService imageService;

    /**
     * Registra un nuevo usuario
     */
    @Transactional
    public User registerUser(RegisterRequest request) {
        // Validar que el email no exista
        if (userRepository.existsByEmail(request.getEmail())) {
            throw new IllegalArgumentException("El email ya está registrado");
        }

        // Validar que el nick no exista
        if (userRepository.existsByNick(request.getNick())) {
            throw new IllegalArgumentException("El nick ya está en uso");
        }

        // Convertir gender string a enum
        Gender gender = Gender.fromEnglishName(request.getGender());

        RoleEntity userRole = roleRepository.findByName(Role.USER.name())
            .orElseThrow(() -> new IllegalStateException("El rol USER no existe en la tabla roles"));

        // Asignar imagen por defecto si no se proporciona
        String profileImg = request.getProfileImg();
        if (profileImg == null || profileImg.trim().isEmpty()) {
            profileImg = gender.getDefaultImagePath();
            log.info("Asignando imagen por defecto para gender: {}", gender.getDisplayName());
        }

        // Crear nuevo usuario
        User user = User.builder()
                .nick(request.getNick())
                .name(request.getName())
                .surname1(request.getSurname1())
                .surname2(request.getSurname2())
                .phone(request.getPhone())
                .gender(gender)
                .bday(request.getBday())
                .email(request.getEmail())
                .password(passwordEncoder.encode(request.getPassword()))
                .profileImg(profileImg)
                .roles(new HashSet<>(Set.of(userRole)))
                .active(false)
                .emailVerified(false)
                .verificationToken(UUID.randomUUID().toString())
                .verificationTokenExpiry(LocalDateTime.now().plusHours(24))
                .build();

        user = userRepository.save(user);

        log.info("Usuario registrado: {}", user.getEmail());

        // Enviar email de verificación
        try {
            emailService.sendVerificationEmail(
                    user.getEmail(),
                    user.getName(),
                    user.getVerificationToken()
            );
        } catch (Exception e) {
            log.warn("No se pudo enviar email de verificacion a {}: {}", user.getEmail(), e.getMessage());
            // No interrumpir el registro si falla el proveedor de correo.
        }

        return user;
    }

    /**
     * Verifica el email del usuario
     */
    @Transactional
    public User verifyEmail(String token) {
        User user = userRepository.findByVerificationToken(token)
                .orElseThrow(() -> new IllegalArgumentException("Token de verificación inválido"));

        // Verificar que el token no haya expirado
        if (user.getVerificationTokenExpiry().isBefore(LocalDateTime.now())) {
            throw new IllegalArgumentException("El token de verificación ha expirado");
        }

        // Marcar como verificado
        user.setEmailVerified(true);
        user.setActive(true);
        user.setVerificationToken(null);
        user.setVerificationTokenExpiry(null);
        user = userRepository.save(user);

        log.info("Email verificado para usuario: {}", user.getEmail());

        // Enviar email de bienvenida
        emailService.sendWelcomeEmail(user.getEmail(), user.getName());

        return user;
    }

    /**
     * Obtiene un usuario por email
     */
    public Optional<User> getUserByEmail(String email) {
        return userRepository.findByEmail(email);
    }

    /**
     * Obtiene un usuario por nick
     */
    public Optional<User> getUserByNick(String nick) {
        return userRepository.findByNick(nick);
    }

    /**
     * Obtiene un usuario por ID
     */
    public Optional<User> getUserById(Long id) {
        return userRepository.findById(id);
    }

    /**
     * Valida las credenciales del usuario
     */
    public boolean validateCredentials(String email, String password) {
        return userRepository.findByEmail(email)
                .map(user -> {
                    if (!user.getActive() || !user.getEmailVerified()) {
                        throw new IllegalStateException("Usuario no verificado o inactivo");
                    }
                    return passwordEncoder.matches(password, user.getPassword());
                })
                .orElse(false);
    }

    /**
     * Actualiza el perfil del usuario
     */
    @Transactional
    public User updateUserProfile(Long id, String name, String surname1, String surname2, String phone) {
        User user = userRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Usuario no encontrado"));

        user.setName(name);
        user.setSurname1(surname1);
        user.setSurname2(surname2);
        user.setPhone(phone);

        return userRepository.save(user);
    }

    /**
     * Actualiza la imagen de perfil de un usuario
     */
    @Transactional
    public User updateProfileImage(Long userId, String imagePath) {
        User user = userRepository.findById(userId)
                .orElseThrow(() -> new IllegalArgumentException("Usuario no encontrado"));
        
        // Eliminar imagen anterior si no es por defecto
        if (user.getProfileImg() != null
                && !imageService.isProtectedImage(user.getProfileImg())
                && !user.getProfileImg().equals(imagePath)) {
            try {
                imageService.deleteImage(user.getProfileImg());
            } catch (Exception e) {
                log.warn("No se pudo eliminar imagen anterior de usuario {}: {}", userId, e.getMessage());
            }
        }

        user.setProfileImg(imagePath);
        return userRepository.save(user);
    }

    /**
     * Cambia la contraseña del usuario
     */
    @Transactional
    public User changePassword(Long id, String oldPassword, String newPassword) {
        User user = userRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Usuario no encontrado"));

        // Validar contraseña anterior
        if (!passwordEncoder.matches(oldPassword, user.getPassword())) {
            throw new IllegalArgumentException("Contraseña actual incorrecta");
        }

        // Validar que la nueva contraseña sea diferente
        if (passwordEncoder.matches(newPassword, user.getPassword())) {
            throw new IllegalArgumentException("La nueva contraseña no puede ser igual a la actual");
        }

        // Cambiar contraseña
        user.setPassword(passwordEncoder.encode(newPassword));
        user = userRepository.save(user);

        log.info("Contraseña cambiada para usuario: {}", user.getEmail());
        return user;
    }

    /**
     * Elimina la cuenta del usuario (requiere confirmación con contraseña)
     */
    @Transactional
    public void deleteUserAccount(Long id, String password) {
        User user = userRepository.findById(id)
                .orElseThrow(() -> new IllegalArgumentException("Usuario no encontrado"));

        if (!passwordEncoder.matches(password, user.getPassword())) {
            throw new IllegalArgumentException("Contraseña incorrecta");
        }

        // Eliminar imagen de perfil si existe
        if (user.getProfileImg() != null && !user.getProfileImg().isEmpty()) {
            try {
                imageService.deleteProfileImage(user.getId(), user.getProfileImg());
            } catch (Exception e) {
                log.warn("Error al eliminar imágenes de perfil: {}", e.getMessage());
            }
        }

        userRepository.delete(user);
        log.info("Cuenta eliminada permanentemente: {}", user.getEmail());
    }

    /**
     * Elimina un usuario por ID (Admin only)
     */
    @Transactional
    public void deleteUser(Long id) {
        userRepository.findById(id).ifPresent(user -> {
            if (user.getRole() == Role.ADMIN) {
                throw new IllegalStateException("No se puede eliminar usuarios con rol ADMIN");
            }
            // Eliminar imagen de perfil si existe
            if (user.getProfileImg() != null && !user.getProfileImg().isEmpty()) {
                try {
                    imageService.deleteProfileImage(user.getId(), user.getProfileImg());
                } catch (Exception e) {
                    log.warn("Error al eliminar imágenes de perfil: {}", e.getMessage());
                }
            }
            userRepository.delete(user);
        });
    }

    /**
     * Cambia el rol de un usuario
     */
    @Transactional
    public User changeUserRole(Long userId, String roleName) {
        User user = userRepository.findById(userId)
                .orElseThrow(() -> new IllegalArgumentException("Usuario no encontrado"));

        Role role = Role.fromDisplayName(roleName);
        RoleEntity roleEntity = roleRepository.findByName(role.name())
                .orElseThrow(() -> new IllegalStateException("Rol no encontrado en la base de datos: " + role.name()));

        user.getRoles().clear();
        user.getRoles().add(roleEntity);
        
        return userRepository.save(user);
    }
}
