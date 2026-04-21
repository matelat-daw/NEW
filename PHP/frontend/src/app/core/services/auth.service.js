/**
 * auth.service.js - Servicio de autenticación
 */

class AuthService {
    /**
     * Información del usuario autenticado en sesión
     */
    static userSession = null;
    
    /**
     * Token JWT del usuario
     */
    static jwtToken = null;

    /**
     * Login del usuario
     * @param {string} email - Email del usuario
     * @param {string} password - Contraseña del usuario
     * @returns {Promise<Object>}
     */
    static async login(email, password) {
        try {
            const url = API_CONFIG.BASE_URL + API_CONFIG.ENDPOINTS.LOGIN;
            const data = {
                email: email,
                password: password
            };
const response = await Utils.makeRequest('POST', url, data);
// Detectar si el login fue exitoso - flexible para diferentes formatos de API
            let isSuccess = false;
            let userData = null;
            let token = null;

            // Formato 1: response.success
            if (response.success === true) {
                isSuccess = true;
                userData = response.data || response.user || response;
                token = response.token;
            }
            // Formato 2: response.code === 200 con data
            else if (response.code === 200 && (response.data || response.user)) {
                isSuccess = true;
                userData = response.data || response.user;
                token = response.token;
            }
            // Formato 3: respuesta directa con token (API .NET)
            else if (response.token) {
                isSuccess = true;
                userData = response.data || response.user || response;
                token = response.token;
            }
            // Formato 4: HTTP 200 sin estructura específica (asumir éxito con token)
            else if (!response.error && !response.message?.includes('failed') && !response.message?.includes('invalid')) {
                isSuccess = true;
                userData = response.data || response.user || response;
                token = response.token || response.access_token;
            }

            if (isSuccess && token) {
                // Guardar datos del usuario en sessionStorage
                this.setUserSession(userData);
                
                // Guardar token JWT
                this.setJwtToken(token);
return {
                    success: true,
                    message: response.message || 'Login exitoso',
                    user: userData,
                    token: token
                };
            } else if (isSuccess) {
                // Login exitoso pero sin token
                this.setUserSession(userData);
return {
                    success: true,
                    message: response.message || 'Login exitoso',
                    user: userData,
                    token: null
                };
            }

            // Si llegamos aquí, algo salió mal
            const errorMsg = response.message || response.error || 'Error en login';
return {
                success: false,
                message: errorMsg
            };
        } catch (error) {
throw error;
        }
    }

    /**
     * Guarda los datos del usuario en sesión
     * @param {Object} userData - Datos del usuario
     */
    static setUserSession(userData) {
        this.userSession = userData;
        sessionStorage.setItem('user_session', JSON.stringify(userData));
    }

    /**
     * Obtiene los datos del usuario de sesión
     * @returns {Object|null}
     */
    static getUserSession() {
        if (!this.userSession) {
            const stored = sessionStorage.getItem('user_session');
            if (stored) {
                this.userSession = JSON.parse(stored);
            }
        }
        return this.userSession;
    }

    /**
     * Verifica si el usuario está autenticado
     * @returns {boolean}
     */
    static isAuthenticated() {
        const user = this.getUserSession();
        return user !== null && user !== undefined;
    }

    /**
     * Cierra la sesión del usuario (logout)
     */
    static async logout() {
        // Logout local: no depende del backend
        this.userSession = null;
        this.jwtToken = null;
        sessionStorage.removeItem('user_session');
        sessionStorage.removeItem('jwt_token');

        if (Utils.clearCache) Utils.clearCache();

        const loginUrl = ROUTES && ROUTES.LOGIN ? ROUTES.LOGIN : (APP_BASE_PATH + 'login');
        window.location.href = loginUrl;
    }

    /**
     * Obtiene el nombre completo del usuario
     * @returns {string}
     */
    static getFullName() {
        const user = this.getUserSession();
        if (!user) return '';
        
        const name = user.name || '';
        const surname1 = user.surname1 || '';
        const surname2 = user.surname2 || '';
        
        return `${name} ${surname1} ${surname2}`.trim();
    }

    /**
     * Obtiene la URL de la foto de perfil del usuario
     * @returns {string|null} - La URL de la imagen o null si no existe
     */
    static getProfilePictureUrl() {
        const user = this.getUserSession();
        if (!user) {
            return null;
        }

        let imgPath = String(user.profileImg || '').trim();
        if (!imgPath) {
            // Fallback si no hay profileImg (shouldn't happen after toDto())
            const gender = String(user.gender || '').trim().toUpperCase();
            if (gender === 'MALE') {
                imgPath = 'uploads/default/male.png';
            } else if (gender === 'FEMALE') {
                imgPath = 'uploads/default/female.png';
            } else {
                imgPath = 'uploads/default/other.png';
            }
        }
        
        // La ruta ya viene completa de la API: uploads/{userId}/profile.jpg
        // Devolver la ruta absoluta: /uploads/{userId}/profile.jpg
        if (imgPath.startsWith('uploads/')) {
            return '/' + imgPath;
        }
        
        // Compatibilidad con formatos antiguos
        if (imgPath.startsWith('default/')) {
            return '/uploads/' + imgPath;
        }
        
        return '/' + imgPath;
    }

    /**
     * Obtiene el nickname del usuario
     * @returns {string}
     */
    static getNick() {
        const user = this.getUserSession();
        return user ? user.nick : '';
    }

    /**
     * Obtiene el email del usuario
     * @returns {string}
     */
    static getEmail() {
        const user = this.getUserSession();
        return user ? user.email : '';
    }

    /**
     * Obtiene el rol del usuario
     * @returns {string}
     */
    static getRole() {
        const user = this.getUserSession();
        return user ? user.role : 'USER';
    }

    /**
     * Guarda el token JWT
     * @param {string} token - Token JWT
     */
    static setJwtToken(token) {
        this.jwtToken = token;
        if (token) {
            sessionStorage.setItem('jwt_token', token);
}
    }

    /**
     * Obtiene el token JWT
     * @returns {string|null}
     */
    static getJwtToken() {
        if (!this.jwtToken) {
            const stored = sessionStorage.getItem('jwt_token');
            if (stored) {
                this.jwtToken = stored;
            }
        }
        return this.jwtToken;
    }

    /**
     * Obtiene el header Authorization con el token JWT
     * @returns {Object}
     */
    static getAuthorizationHeader() {
        const token = this.getJwtToken();
        if (token) {
            return {
                'Authorization': `Bearer ${token}`
            };
        }
        return {};
    }

    /**
     * Cierra la sesión del usuario (logout) - Actualizado para limpiar también el JWT
     */
    static logoutWithJwt() {
        this.userSession = null;
        this.jwtToken = null;
        sessionStorage.removeItem('user_session');
        sessionStorage.removeItem('jwt_token');
}
}

// Exponer globalmente para la verificación de carga
window.AuthService = AuthService;

// Registrar que este script se ha cargado
if (typeof AppScripts !== 'undefined') AppScripts.register('auth.service');



