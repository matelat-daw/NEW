// Auth Service
var authService = {
    user: null,
    
    init: function() {
        // Cargar usuario del localStorage (NO el token, viene en cookie)
        var userData = localStorage.getItem('user');
        if (userData) {
            try {
                this.user = JSON.parse(userData);
            } catch (e) {
                this.logout();
            }
        }
    },
    
    login: function(email, password) {
        var self = this;
        return apiService.post('/login', { email: email, password: password })
            .then(function(response) {
                if (response.success && response.data) {
                    self.user = response.data;
                    // Solo guardar usuario, NO el token (va en cookie HTTP-only)
                    localStorage.setItem('user', JSON.stringify(response.data));
                    
                    // Actualizar menú de usuario
                    if (window.headerComponent) {
                        window.headerComponent.updateUserMenu();
                    }
                    
                    return response;
                }
                throw new Error(response.message || 'Error en login');
            });
    },
    
    register: function(userData) {
        var self = this;
        return apiService.post('/register', userData)
            .then(function(response) {
                if (response.success && response.data) {
                    // Si requiere activación, NO guardar el usuario
                    // El usuario se guardará después de activar su cuenta
                    if (response.data.requiresActivation) {
                        // No guardar nada en localStorage
                        // El usuario debe activar su cuenta primero
                        return response;
                    }
                    
                    // Solo guardar si NO requiere activación
                    self.user = response.data;
                    localStorage.setItem('user', JSON.stringify(response.data));
                    
                    // Actualizar menú de usuario
                    if (window.headerComponent) {
                        window.headerComponent.updateUserMenu();
                    }
                    
                    return response;
                }
                throw new Error(response.message || 'Error en registro');
            });
    },
    
    logout: function() {
        this.user = null;
        localStorage.removeItem('user');
        
        // Actualizar menú de usuario
        if (window.headerComponent) {
            window.headerComponent.updateUserMenu();
        }
        
        // Llamar al endpoint de logout para eliminar cookie
        apiService.post('/logout', {}).catch(function() {
            // Ignorar errores del logout
        });
    },
    
    isLoggedIn: function() {
        return this.user !== null;
    },
    
    getUser: function() {
        return this.user;
    },
    
    hasRole: function(role) {
        if (!this.user || !this.user.roles) return false;
        return this.user.roles.includes(role);
    }
};

// Inicializar al cargar
window.authService = authService;
authService.init();
