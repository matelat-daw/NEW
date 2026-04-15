package com.futureprograms.clients.config;

import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Configuration;
import org.springframework.web.servlet.config.annotation.ResourceHandlerRegistry;
import org.springframework.web.servlet.config.annotation.WebMvcConfigurer;

import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

/**
 * Configuración de Spring Web para servir archivos estáticos desde directorios externos
 * Sirve tanto imágenes de usuario como imágenes por defecto desde la carpeta de uploads
 */
@Configuration
@Slf4j
public class WebConfig implements WebMvcConfigurer {

    @Value("${upload.dir:./uploads}")
    private String uploadDir;

    @Override
    public void addResourceHandlers(ResourceHandlerRegistry registry) {
        // Obtener la ruta absoluta del directorio de carga
        Path uploadPath = Paths.get(uploadDir).toAbsolutePath();
        
        log.info("📁 Configurando servicio de imágenes desde: {}", uploadPath);
        
        // Crear el directorio si no existe
        try {
            if (!Files.exists(uploadPath)) {
                log.info("📁 Creando directorio de imágenes: {}", uploadPath);
                Files.createDirectories(uploadPath);
                log.info("✅ Directorio de imágenes creado");
            }
            
            // Crear subdirectorio de imágenes por defecto si no existe
            Path defaultPath = uploadPath.resolve("default");
            if (!Files.exists(defaultPath)) {
                log.info("📁 Creando subdirectorio de imágenes por defecto: {}", defaultPath);
                Files.createDirectories(defaultPath);
                log.info("✅ Subdirectorio de imágenes por defecto creado");
            }
        } catch (Exception e) {
            log.error("❌ Error al crear directorio de imágenes: {}", e.getMessage());
        }

        // Mapear /images/** a la carpeta de carga externa (imágenes de usuario e imágenes por defecto)
        registry.addResourceHandler("/images/**")
                .addResourceLocations("file:" + uploadPath + "/");
        
        // IMPORTANTE: NO mapear /api/images/** aquí
        // El ImageController maneja /api/images/** con CORS headers correctos
        // Si lo mapeamos como ResourceHandler, los headers CORS no se aplican
        // y las imágenes se bloquean por OpaqueResponseBlocking

        log.info("✅ Mapeando /images/** -> file:{}/ ", uploadPath);
        log.info("✅ /api/images/** es manejado por ImageController con CORS headers");
    }
}
