/**
 * utils.js - Utilidades generales de la aplicación
 */

const Utils = {
    /**
     * Valida un campo según un patrón
     * @param {string} value - Valor a validar
     * @param {RegExp} pattern - Patrón de validación
     * @returns {boolean}
     */
    validateField: (value, pattern) => {
        return pattern.test(value);
    },

    /**
     * Valida un formulario
     * @param {Object} data - Datos del formulario
     * @param {Object} rules - Reglas de validación
     * @returns {Object} {valid: boolean, errors: Object}
     */
    validateForm: (data, rules) => {
        const errors = {};
        
        for (const field in rules) {
            if (rules.hasOwnProperty(field)) {
                const rule = rules[field];
                const value = data[field] || '';
                
                if (rule.required && !value.trim()) {
                    errors[field] = `${rule.label} es requerido`;
                    continue;
                }
                
                if (value && rule.pattern && !Utils.validateField(value, rule.pattern)) {
                    errors[field] = rule.message || `${rule.label} no es válido`;
                }
                
                if (value && rule.minLength && value.length < rule.minLength) {
                    errors[field] = `${rule.label} debe tener al menos ${rule.minLength} caracteres`;
                }
            }
        }
        
        return {
            valid: Object.keys(errors).length === 0,
            errors
        };
    },

    /**
     * Muestra un modal de mensaje
     * @param {string} title - Título del mensaje
     * @param {string} body - Contenido del mensaje
     * @param {string} type - Tipo de mensaje: 'success', 'error', 'info', 'warning'
     */
    showMessage: (title, body, type = 'info') => {
        // Limpiar cualquier modal anterior y su backdrop
        const existingModals = document.querySelectorAll('.modal.show');
        existingModals.forEach(modal => {
            const bsModal = bootstrap.Modal.getInstance(modal);
            if (bsModal) {
                bsModal.hide();
            }
        });

        // Limpiar backdrops residuales
        const residualBackdrops = document.querySelectorAll('.modal-backdrop');
        residualBackdrops.forEach(backdrop => backdrop.remove());

        // Limpiar la clase modal-open del body
        document.body.classList.remove('modal-open');

        const messageModal = document.getElementById('messageModal');
        const messageTitle = document.getElementById('messageTitle');
        const messageBody = document.getElementById('messageBody');
        
        // Limpiar clases anteriores
        messageTitle.className = '';
        
        // Agregar icono y clase según el tipo
        let icon = '';
        switch(type) {
            case 'success':
                icon = '✓ ';
                messageTitle.className = 'text-success';
                break;
            case 'error':
                icon = '✕ ';
                messageTitle.className = 'text-danger';
                break;
            case 'warning':
                icon = '⚠ ';
                messageTitle.className = 'text-warning';
                break;
            case 'info':
                icon = 'ℹ ';
                messageTitle.className = 'text-info';
                break;
        }
        
        messageTitle.textContent = icon + title;
        messageBody.innerHTML = body;
        
        // Mostrar modal con opciones mejoradas
        const modal = new bootstrap.Modal(messageModal, {
            backdrop: true,  // Permitir cerrar con backdrop
            keyboard: true   // Permitir cerrar con ESC
        });
        modal.show();

        // Limpiar cuando se cierre
        messageModal.addEventListener('hidden.bs.modal', () => {
            // Asegurar que se limpie el backdrop
            const backdrops = document.querySelectorAll('.modal-backdrop');
            backdrops.forEach(backdrop => backdrop.remove());
            document.body.classList.remove('modal-open');
            document.body.style.paddingRight = '';
        }, { once: true });
    },

    /**
     * Limpia los campos de un formulario
     * @param {string} formId - ID del formulario
     */
    clearForm: (formId) => {
        const form = document.getElementById(formId);
        if (form) {
            form.reset();
            // Limpiar clases de validación
            form.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
        }
    },

    /**
     * Muestra errores en un formulario
     * @param {string} formId - ID del formulario
     * @param {Object} errors - Errores a mostrar
     */
    showFormErrors: (formId, errors) => {
        const form = document.getElementById(formId);
        if (!form) return;
        
        // Limpiar errores anteriores
        form.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
        form.querySelectorAll('.invalid-feedback').forEach(el => el.remove());
        
        // Mostrar nuevos errores
        for (const field in errors) {
            if (errors.hasOwnProperty(field)) {
                const input = form.querySelector(`[name="${field}"]`);
                if (input) {
                    input.classList.add('is-invalid');
                    const feedbackDiv = document.createElement('div');
                    feedbackDiv.className = 'invalid-feedback d-block';
                    feedbackDiv.textContent = errors[field];
                    input.parentNode.appendChild(feedbackDiv);
                }
            }
        }
    },

    /**
     * Cache para peticiones GET
     */
    _cache: new Map(),

    /**
     * Tiempo de vida del cache en milisegundos (5 minutos)
     */
    _cacheTTL: 5 * 60 * 1000,

    /**
     * Realiza una llamada HTTP con cache opcional para GET
     * @param {string} method - Método HTTP
     * @param {string} url - URL
     * @param {Object} data - Datos a enviar
     * @param {boolean} useCache - Si debe intentar usar el cache (solo para GET)
     * @returns {Promise}
     */
    makeRequest: async (method, url, data = null, useCache = false) => {
        // Solo aplicar cache para GET
        if (method.toUpperCase() === 'GET' && useCache) {
            const cached = Utils._cache.get(url);
            if (cached && (Date.now() - cached.timestamp < Utils._cacheTTL)) {
                console.debug(`🚀 Cache hit: ${url}`);
                return cached.data;
            }
        }

        try {
            const options = {
                method: method,
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json',
					...AuthService.getAuthorizationHeader()
                },
                mode: 'cors',
                credentials: 'include'
            };
            
            if (data) {
                options.body = JSON.stringify(data);
            }
            
            const response = await fetch(url, options);
            
            // Interceptor global para error 401 (Token expirado o inválido)
            if (response.status === 401) {
                console.warn('⚠️ Sesión expirada o token inválido (401)');
                // Evitar bucles de redirección si ya estamos en login
                if (!window.location.pathname.endsWith('/login')) {
                    AuthService.logout();
                    return;
                }
            }

            if (!response.ok) {
                // ... (existing error handling)
                const errorText = await response.text();
                let backendPayload = null;
                try {
                    backendPayload = JSON.parse(errorText);
                } catch (_) {}

                const backendMessage = backendPayload?.message || backendPayload?.error || response.statusText;
                const error = new Error(`HTTP ${response.status}: ${backendMessage}`);
                error.status = response.status;
                error.backendMessage = backendMessage;
                error.backendErrors = backendPayload?.errors;
                error.backendPayload = backendPayload;
                throw error;
            }
            
            const responseData = await response.json();
            
            // Guardar en cache si es GET y se solicitó cache
            if (method.toUpperCase() === 'GET' && useCache) {
                Utils._cache.set(url, {
                    data: responseData,
                    timestamp: Date.now()
                });
            }
            
            return responseData;
        } catch (error) {
            // ... (existing error handling)
            if (error.message.includes('Failed to fetch') || error.message.includes('NetworkError')) {
                throw new Error('CORS_ERROR: No API available. Make sure the API is running.');
            }
            throw error;
        }
    },

    /**
     * Limpia el cache de peticiones
     */
    clearCache: () => {
        Utils._cache.clear();
        console.debug('🧹 Cache de peticiones limpiado');
    },

    /**
     * Realiza una llamada HTTP con FormData (para archivos)
     * @param {string} method - Método HTTP (GET, POST, etc.)
     * @param {string} url - URL
     * @param {FormData} formData - Datos en FormData
     * @returns {Promise}
     */
    makeRequestWithFormData: async (method, url, formData) => {
        try {
            const options = {
                method: method,
                // NO establecer Content-Type - el navegador lo hará automáticamente con boundary
				headers: {
					...AuthService.getAuthorizationHeader()  // ← AGREGAR ESTO
				},
                mode: 'cors',
                credentials: 'include',
                body: formData
            };
const response = await fetch(url, options);
if (!response.ok) {
                const errorText = await response.text();
let backendPayload = null;
                try {
                    backendPayload = JSON.parse(errorText);
                } catch (_) {
                    // Ignore parse errors and keep raw status text fallback
                }

                const backendMessage = backendPayload?.message || backendPayload?.error || response.statusText;
                const error = new Error(`HTTP ${response.status}: ${backendMessage}`);
                error.status = response.status;
                error.backendMessage = backendMessage;
                error.backendErrors = backendPayload?.errors;
                error.backendPayload = backendPayload;
                throw error;
            }
            
            const responseData = await response.json();
return responseData;
        } catch (error) {
// Detectar tipo de error
            if (error.message.includes('Failed to fetch') || error.message.includes('NetworkError')) {
throw new Error('CORS_ERROR: No API available. Make sure the backend API is running.');
            }
            
            throw error;
        }
    },

    /**
     * Obtiene la ruta actual
     * @returns {string}
     */
    getCurrentRoute: () => {
        return window.location.pathname || '/';
    },

    /**
     * Navega a una ruta
     * @param {string} route - Ruta a la que navegar
     */
    navigate: (route) => {
        const app = App.getInstance();
        app.navigateTo(route);
    }
};

// Exponer globalmente para la verificación de carga
window.Utils = Utils;

// Registrar que este script se ha cargado
if (typeof AppScripts !== 'undefined') AppScripts.register('utils');


