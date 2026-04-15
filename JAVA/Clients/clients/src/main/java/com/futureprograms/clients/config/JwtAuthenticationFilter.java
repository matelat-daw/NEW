package com.futureprograms.clients.config;

import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.security.core.userdetails.UserDetails;
import org.springframework.security.core.userdetails.UserDetailsService;
import org.springframework.security.web.authentication.WebAuthenticationDetailsSource;
import org.springframework.stereotype.Component;
import org.springframework.web.filter.OncePerRequestFilter;
import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import java.io.IOException;
import java.util.Set;

@Component
@Slf4j
@RequiredArgsConstructor
public class JwtAuthenticationFilter extends OncePerRequestFilter {

    /* private static final Set<String> PUBLIC_EXACT_PATHS = Set.of(
            "/api/auth/login",
            "/auth/login",
            "/api/user/register",
            "/user/register",
            "/api/auth/refresh",
            "/auth/refresh",
            "/api/auth/forgot-password",
            "/auth/forgot-password",
            "/api/auth/reset-password",
            "/auth/reset-password"
    ); */

    private static final Set<String> PUBLIC_EXACT_PATHS = Set.of(
        "/api/auth/login",
        "/api/user/register"
    );

    private final JwtProvider jwtProvider;
    private final UserDetailsService userDetailsService;

    @Override
    protected boolean shouldNotFilter(HttpServletRequest request) {
        String path = request.getRequestURI();
        if (PUBLIC_EXACT_PATHS.contains(path)) {
            return true;
        }

        return path.startsWith("/api/auth/verify/")
            || path.startsWith("/api/user/register/")
            || path.startsWith("/api/images/");
    }
    /* protected boolean shouldNotFilter(HttpServletRequest request) {
        String path = request.getRequestURI();
        if (PUBLIC_EXACT_PATHS.contains(path)) {
            return true;
        }

        return path.startsWith("/api/auth/verify/")
                || path.startsWith("/auth/verify/")
                || path.startsWith("/api/user/register/")
                || path.startsWith("/user/register/")
                || path.startsWith("/api/images/")
                || path.startsWith("/images/");
    } */

    @Override
    protected void doFilterInternal(
            HttpServletRequest request,
            HttpServletResponse response,
            FilterChain filterChain
    ) throws ServletException, IOException {
        try {
            String jwt = getJwtFromRequest(request);

            if (jwt != null && jwtProvider.isTokenValid(jwt) && !jwtProvider.isTokenExpired(jwt)) {
                String email = jwtProvider.getEmailFromToken(jwt);

                UserDetails userDetails = userDetailsService.loadUserByUsername(email);
                
                UsernamePasswordAuthenticationToken authentication =
                        new UsernamePasswordAuthenticationToken(
                                userDetails,
                                null,
                                userDetails.getAuthorities()
                        );

                authentication.setDetails(new WebAuthenticationDetailsSource().buildDetails(request));
                SecurityContextHolder.getContext().setAuthentication(authentication);
                log.debug("JWT válido para usuario: {}", email);
            }
        } catch (Exception e) {
            log.error("No se pudo autenticar con JWT: {}", e.getMessage());
        }

        filterChain.doFilter(request, response);
    }

    /**
     * Extrae el JWT del header Authorization o de una cookie
     */
    private String getJwtFromRequest(HttpServletRequest request) {
        String bearerToken = request.getHeader("Authorization");
        if (bearerToken != null && bearerToken.startsWith("Bearer ")) {
            return bearerToken.substring(7);
        }

        // Alternativamente, buscar en cookies
        if (request.getCookies() != null) {
            for (jakarta.servlet.http.Cookie cookie : request.getCookies()) {
                if ("auth_token".equals(cookie.getName())) {
                    return cookie.getValue();
                }
            }
        }

        return null;
    }
}
