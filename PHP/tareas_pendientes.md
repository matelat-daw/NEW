# Tareas Pendientes - Arreglar Subida de Imágenes

**Problema:** La subida de la foto de perfil falla con un error 500 (Internal Server Error).

**Causa Raíz:** El servidor web (Nginx/PHP) no tiene los permisos necesarios para crear carpetas dentro del directorio `uploads` en el sistema de archivos de Windows.

**Fecha para reintentar:** Dentro de 10 horas (o cuando tengas tiempo).

---

## Plan de Acción

Para solucionar este problema, necesitas ajustar los permisos de la carpeta `uploads`.

**Pasos a seguir:**

1.  **Abrir Propiedades de la Carpeta:**
    *   Abre tu explorador de archivos.
    *   Navega a la ruta: `d:\BackUp\NEW\PHP`
    *   Haz clic derecho en la carpeta `uploads`.
    *   Selecciona **Propiedades**.

2.  **Editar Permisos de Seguridad:**
    *   Ve a la pestaña **Seguridad**.
    *   Haz clic en el botón **Editar...** para cambiar los permisos.

3.  **Dar Permisos al Usuario del Servidor Web:**
    *   En la lista "Nombres de grupos o usuarios", necesitas encontrar el usuario que utiliza tu servidor web. Busca uno de los siguientes:
        *   `IUSR` (muy común para servidores web en Windows)
        *   `SYSTEM`
        *   `Servicio local`
        *   El usuario con el que se ejecuta el servicio de Nginx o PHP-FPM.
    *   Selecciona el usuario correcto.

4.  **Asignar Control Total:**
    *   En el panel de permisos de abajo, marca la casilla **Permitir** para **Control total**. Esto le dará todos los permisos necesarios (leer, escribir, crear, eliminar).
    *   Haz clic en **Aplicar** y luego en **Aceptar** en todas las ventanas.

5.  **Verificar:**
    *   Una vez aplicados los permisos, intenta subir una foto de perfil de nuevo desde la aplicación. El problema debería estar resuelto.

---
**Nota:** Este es un problema de configuración del entorno de desarrollo local y no un error en el código de la aplicación. El código está intentando realizar una operación válida, pero el sistema operativo se lo impide.