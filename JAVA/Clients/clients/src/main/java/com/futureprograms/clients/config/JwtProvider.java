package com.futureprograms.clients.config;

import io.jsonwebtoken.Claims;
import io.jsonwebtoken.Jwts;
import io.jsonwebtoken.security.Keys;
import jakarta.annotation.PostConstruct;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

import javax.crypto.SecretKey;
import java.nio.charset.StandardCharsets;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;

@Component
@Slf4j
public class JwtProvider {

    @Value("${jwt.secret}")
    private String jwtSecret;

    @Value("${jwt.expiration}")
    private long jwtExpiration;

    @Value("${jwt.refresh-expiration}")
    private long refreshExpiration;

    private SecretKey key;
    private io.jsonwebtoken.JwtParser jwtParser;

    @PostConstruct
    public void init() {
        if (jwtSecret == null || jwtSecret.length() < 32) {
            log.warn("JWT secret is not provided or too short. Using a fallback generated key for security.");
            this.key = Jwts.SIG.HS256.key().build();
        } else {
            this.key = Keys.hmacShaKeyFor(jwtSecret.getBytes(StandardCharsets.UTF_8));
        }
        
        // Reusar el parser para mejor rendimiento
        this.jwtParser = Jwts.parser()
                .verifyWith(key)
                .build();
    }

    /**
     * Genera un JWT token para el usuario
     */
    public String generateToken(String email) {
        return generateToken(email, new HashMap<>());
    }

    /**
     * Genera un JWT token con claims adicionales
     */
    public String generateToken(String email, Map<String, Object> claims) {
        return createToken(claims, email, jwtExpiration);
    }

    /**
     * Genera un refresh token de larga duración
     */
    public String generateRefreshToken(String email) {
        Map<String, Object> claims = new HashMap<>();
        claims.put("type", "REFRESH");
        return createToken(claims, email, refreshExpiration);
    }

    /**
     * Crea el token JWT con firma HS512
     */
    private String createToken(Map<String, Object> claims, String subject, long expiration) {
        Date now = new Date();
        Date expiryDate = new Date(now.getTime() + expiration);

        return Jwts.builder()
                .claims(claims)
                .subject(subject)
                .issuedAt(now)
                .expiration(expiryDate)
                .signWith(key)
                .compact();
    }

    /**
     * Obtiene el email del JWT
     */
    public String getEmailFromToken(String token) {
        return getAllClaimsFromToken(token).getSubject();
    }

    /**
     * Obtiene los claims del JWT
     */
    public Claims getAllClaimsFromToken(String token) {
        return jwtParser
                .parseSignedClaims(token)
                .getPayload();
    }

    /**
     * Valida si el token es válido
     */
    public Boolean isTokenValid(String token) {
        try {
            jwtParser.parseSignedClaims(token);
            return true;
        } catch (Exception e) {
            log.error("Invalid JWT token: {}", e.getMessage());
            return false;
        }
    }

    /**
     * Verifica si el token ha expirado
     */
    public boolean isTokenExpired(String token) {
        try {
            Claims claims = getAllClaimsFromToken(token);
            return claims.getExpiration().before(new Date());
        } catch (Exception e) {
            return true;
        }
    }
}
