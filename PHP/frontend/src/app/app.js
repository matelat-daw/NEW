/**
 * app.js - Controlador principal de la aplicación
 * Maneja las rutas y la lógica central
 */

class App {
    constructor() {
        this.currentRoute = 'home';
        this.basePath = window.APP_BASE_PATH || '/';
        this.routes = {
            'home': this.loadHome,
            'register': this.loadRegister,
            'users': this.loadUsers,
            'login': this.loadLogin,
            'dashboard': this.loadDashboard,
            'profile': this.loadProfile,
            'customers': this.loadCustomers
        };
        // Instancias de componentes
        this.loginComponent = null;
        this.dashboardComponent = null;
        this.registerComponent = null;
        this.profileComponent = null;
        this.usersComponent = null;
        this.customersComponent = null;
    }

    normalizeBasePath(path) {
        if (!path || path === '/') return '/';
        const withLeading = path.startsWith('/') ? path : `/${path}`;
        return withLeading.endsWith('/') ? withLeading : `${withLeading}/`;
    }

    toAbsolutePath(path) {
        // Asegurar que path siempre comience con /
        if (!path) return '/';
        if (!path.startsWith('/')) {
            path = '/' + path;
        }
        
        // Si el basePath es /, simplemente devolver path
        if (this.basePath === '/') {
            return path;
        }
        
        const normalizedBase = this.normalizeBasePath(this.basePath);
        if (path === '/') return normalizedBase;
        if (path.startsWith(normalizedBase)) return path;

        const normalizedTarget = path.startsWith('/') ? path.substring(1) : path;
        return `${normalizedBase}${normalizedTarget}`;
    }

    /**
     * Inicializa la aplicación
     */
    static init() {
        const instance = App.getInstance();
        
        // Configurar rutas INMEDIATAMENTE (no esperar al navbar)
        instance.setupRouting();
        
        // Cargar navbar en paralelo (sin bloquear)
        instance.loadNavbar().catch(error => {
            console.error('Error al cargar navbar:', error);
        });
    }

    /**
     * Obtiene la instancia singleton de App
     * @returns {App}
     */
    static getInstance() {
        if (!window._appInstance) {
            window._appInstance = new App();
        }
        return window._appInstance;
    }

    /**
     * Limpia todas las instancias de componentes
     * Se llama cuando el usuario hace logout
     */
    clearComponentInstances() {
        this.loginComponent = null;
        this.dashboardComponent = null;
        this.registerComponent = null;
        this.profileComponent = null;
        this.usersComponent = null;
        this.customersComponent = null;
}

    /**
     * Configura el sistema de rutas
     */
    setupRouting() {
        // Manejar cambios de ruta usando popstate
        window.addEventListener('popstate', () => this.handleRouteChange());
        
        // Interceptar clics en enlaces internos
        document.addEventListener('click', (e) => this.handleLinkClick(e));
        
        // Cargar ruta inicial
        this.handleRouteChange();
    }

    /**
     * Intercepta clics en enlaces internos
     */
    handleLinkClick(e) {
        const link = e.target.closest('a');
        if (!link) return;
        
        let href = link.getAttribute('href');
        if (!href) return;
        
        // Filtrar enlaces externos y especiales
        if (href.startsWith('http://') || href.startsWith('https://') || 
            href.startsWith('#') || href.startsWith('javascript:')) {
            return;
        }

        // Ignorar dropdowns
        if (link.getAttribute('data-bs-toggle') === 'dropdown') return;

        // Solo procesar rutas que comienzan con /
        if (!href.startsWith('/')) return;
        
        e.preventDefault();
        this.navigateTo(href);
    }

    /**
     * Navega a una ruta
     */
    navigateTo(path) {
        // Validar que path es una ruta relativa válida
        if (!path || typeof path !== 'string') {
            console.error('Invalid path:', path);
            return;
        }
        
        // Si path comienza con http, ignorar (no es ruta interna)
        if (path.includes('://')) {
            console.error('Cannot navigate to external URL:', path);
            return;
        }
        
        // Asegurar que path comienza con /
        if (!path.startsWith('/')) {
            path = '/' + path;
        }
        
        let fullPath = this.toAbsolutePath(path);
        
        // Validar que fullPath NUNCA contiene protocolo
        if (fullPath.includes('://')) {
            console.error('Invalid path generated (contains protocol):', fullPath);
            return;
        }
        
        // Validar que fullPath comienza con /
        if (!fullPath.startsWith('/')) {
            console.error('Invalid path (no leading slash):', fullPath);
            return;
        }
        
        try {
            if (window.location.pathname !== fullPath) {
                window.history.pushState(null, '', fullPath);
            }
            this.handleRouteChange();
        } catch (error) {
            console.error('Error during navigation:', error, 'path:', fullPath);
            // Si falla pushState, intentar navegación normal
            window.location.pathname = fullPath;
        }
    }

    /**
     * Maneja los cambios de ruta
     */
    handleRouteChange() {
        const path = window.location.pathname;
        const normalizedBase = this.normalizeBasePath(this.basePath);
        const relativePath = path.startsWith(normalizedBase)
            ? path.substring(normalizedBase.length)
            : path.replace(/^\//, '');
        const parts = relativePath.split('/').filter(Boolean);
        const route = parts.length > 0 ? parts[0] : 'home';
        const param = parts.length > 1 ? parts[1] : null;
        this.currentRoute = route;
        
        // Cargar ruta sin esperar
        if (this.routes[route]) {
            this.routes[route].call(this, param);
        } else {
            this.loadHome();
        }

        // Actualizar navbar sin esperar (asincrónico)
        this.updateNavbarAsync(route);
    }

    /**
     * Actualiza el navbar de forma asincrónica (sin bloquear)
     */
    updateNavbarAsync(route) {
        // Si el navbar no existe, intentar cargarlo
        if (!window.NavBar) {
            this.loadNavbar(); // Sin await
            return;
        }

        // Actualizar navbar activo
        if (typeof window.NavBar.updateActiveLink === 'function') {
            try {
                window.NavBar.updateActiveLink(route);
            } catch (error) {
                console.warn('Error al actualizar navbar active link:', error);
            }
        }
        
        // Reinicializar navbar
        if (typeof window.NavBar.reinit === 'function') {
            try {
                window.NavBar.reinit();
            } catch (error) {
                console.warn('Error al reinicializar navbar:', error);
            }
        }
    }

    /**
     * Carga el navbar de forma asincrónica (nunca bloquea)
     */
    async loadNavbar() {
        try {
            // Si el navbar ya está inicializado y existe, no reinicializar
            const navbarContainer = document.getElementById('navbar-container');
            if (navbarContainer && navbarContainer.innerHTML.trim() !== '' && window.NavBar) {
                // Navbar ya existe, solo actualizar su estado sin esperar
                if (typeof window.NavBar.reinit === 'function') {
                    window.NavBar.reinit();
                }
                return;
            }

            // Crear NavBar si no existe
            if (!window.NavBar) {
                if (typeof NavbarComponent !== 'undefined') {
                    window.NavBar = new NavbarComponent();
                } else {
                    console.warn('NavbarComponent no está disponible');
                    return;
                }
            }
            
            // Verificar que NavBar tenga el método init - sin esperar, solo iniciar
            if (typeof window.NavBar.init === 'function') {
                // Iniciar sin await - dejar que se cargue en background
                window.NavBar.init().catch(error => {
                    console.warn('Error al inicializar navbar:', error);
                });
            }
        } catch (error) {
            console.error('Error en loadNavbar:', error);
            // No lanzar error - permitir que la aplicación continúe
        }
    }

    /**
     * Carga la vista de inicio
     */
    loadHome() {
        const outlet = document.getElementById('router-outlet');
        if (outlet) {
            const isAuthenticated = typeof AuthService !== 'undefined' && AuthService.isAuthenticated();
            
            if (isAuthenticated) {
                // Si el usuario est� autenticado, mostrar bienvenida sin botones
                outlet.innerHTML = `
                    <div class="container mt-5">
                        <div class="row justify-content-center">
                            <div class="col-md-8">
                                <div class="card shadow">
                                    <div class="card-body text-center py-5">
                                        <i class="fas fa-users display-1 text-primary mb-3"></i>
                                        <h1 class="card-title mb-3">Welcome to Clients API</h1>
                                        <p class="lead text-muted mb-4">
                                            Manage your users easily and efficiently
                                        </p>
                                        <p class="text-muted">
                                            Use the navigation menu to access the dashboard or manage your account.
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
            } else {
                // Si el usuario NO est� autenticado, mostrar con botones de login/register
                outlet.innerHTML = `
                    <div class="container mt-5">
                        <div class="row justify-content-center">
                            <div class="col-md-8">
                                <div class="card shadow">
                                    <div class="card-body text-center py-5">
                                        <i class="fas fa-users display-1 text-primary mb-3"></i>
                                        <h1 class="card-title mb-3">Welcome to Clients API</h1>
                                        <p class="lead text-muted mb-4">
                                            Manage your users easily and efficiently
                                        </p>
                                        <div class="d-flex justify-content-center gap-3">
                                            <a href="/register" class="btn btn-primary btn-lg d-flex align-items-center">
                                                <i class="fas fa-user-plus me-2"></i> <span>Register</span>
                                            </a>
                                            <a href="/login" class="btn btn-outline-primary btn-lg d-flex align-items-center">
                                                <i class="fas fa-sign-in-alt me-2"></i> <span>Sign In</span>
                                            </a>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
            }
        }
    }

    /**
     * Carga el formulario de registro
     */
    loadRegister() {
// Mostrar HTML de registro inline mientras se carga
        const outlet = document.getElementById('router-outlet');
        if (outlet) {
            outlet.innerHTML = `
                <div class="register-container">
                    <div class="text-center">
                        <div class="spinner-border text-primary" role="status" style="margin-top: 50px;">
                            <span class="visually-hidden">Cargando...</span>
                        </div>
                        <p class="mt-3">Cargando formulario de registro...</p>
                    </div>
                </div>
            `;
        }

        // Crear instancia bajo demanda si no existe
        if (!this.registerComponent) {
            // Verificar si RegisterComponent está disponible
            if (typeof RegisterComponent === 'undefined') {
setTimeout(() => {
if (typeof RegisterComponent !== 'undefined') {
this.registerComponent = new RegisterComponent();
                        this.registerComponent.init();
                    } else {
outlet.innerHTML = `
                            <div class="alert alert-danger m-5">
                                <h4>Error al cargar el formulario</h4>
                                <p>No se pudo cargar el componente de registro. Por favor, recarga la página.</p>
                                <button class="btn btn-primary" onclick="window.location.reload()">Recargar página</button>
                            </div>
                        `;
                    }
                }, 100);
                return;
            }
            this.registerComponent = new RegisterComponent();
        }
        this.registerComponent.init();
    }

    /**
     * Carga el formulario de login
     */
    loadLogin() {
// Mostrar HTML de login inline mientras se carga
        const outlet = document.getElementById('router-outlet');
        if (outlet) {
            outlet.innerHTML = `
                <div class="login-container">
                    <div class="text-center">
                        <div class="spinner-border text-primary" role="status" style="margin-top: 50px;">
                            <span class="visually-hidden">Cargando...</span>
                        </div>
                        <p class="mt-3">Cargando formulario de login...</p>
                    </div>
                </div>
            `;
        }

        // Crear instancia bajo demanda si no existe
        if (!this.loginComponent) {
            // Verificar si LoginComponent está disponible
            if (typeof LoginComponent === 'undefined') {
// Intentar cargar después de un delay
                setTimeout(() => {
if (typeof LoginComponent !== 'undefined') {
this.loginComponent = new LoginComponent();
                        this.loginComponent.init();
                    } else {
this.loadLoginFallback();
                    }
                }, 100);
                return;
            }
            this.loginComponent = new LoginComponent();
        }
        this.loginComponent.init();
    }

    /**
     * Formulario de login fallback si el componente no carga
     */
    loadLoginFallback() {
        const outlet = document.getElementById('router-outlet');
        if (outlet) {
            outlet.innerHTML = `
                <div class="container mt-5">
                    <div class="row justify-content-center">
                        <div class="col-md-6">
                            <div class="card shadow-lg">
                                <div class="card-header bg-primary text-white">
                                    <h4 class="mb-0">
                                        <i class="fas fa-sign-in-alt"></i> Sign In
                                    </h4>
                                </div>
                                <div class="card-body">
                                    <form id="loginForm">
                                        <div class="mb-3">
                                            <label class="form-label"><i class="fas fa-envelope"></i> Email</label>
                                            <input type="email" class="form-control" name="email" placeholder="your.email@example.com" required>
                                        </div>
                                        <div class="mb-3">
                                            <label class="form-label"><i class="fas fa-lock"></i> Password</label>
                                            <input type="password" class="form-control" name="password" placeholder="Enter your password" required>
                                        </div>
                                        <button type="submit" class="btn btn-primary w-100">Sign In</button>
                                    </form>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;

            // Adjuntar evento al formulario
            const form = document.getElementById('loginForm');
            if (form) {
                form.addEventListener('submit', async (e) => {
                    e.preventDefault();
                    
                    const email = form.querySelector('input[name="email"]').value;
                    const password = form.querySelector('input[name="password"]').value;

                    // Verificar que AuthService esté disponible
                    if (typeof AuthService === 'undefined') {
if (typeof Utils !== 'undefined' && typeof Utils.showMessage === 'function') {
                            Utils.showMessage('Error', 'Sistema no disponible. Por favor, recarga la página.', 'error');
                        } else {
                            alert('Sistema no disponible. Por favor, recarga la página.');
                        }
                        return;
                    }

                    try {
                        // Mostrar estado de carga
                        const btn = form.querySelector('button[type="submit"]');
                        const originalText = btn.innerHTML;
                        btn.disabled = true;
                        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Signing in...';

                        const result = await AuthService.login(email, password);
                        
                        if (result.success) {
                            // Mostrar éxito
                            if (typeof Utils !== 'undefined' && typeof Utils.showMessage === 'function') {
                                Utils.showMessage('Success', '¡Bienvenido!', 'success');
                            }
                            // Redirigir al dashboard
                            setTimeout(() => {
                                this.navigateTo('/dashboard');
                            }, 1500);
                        } else {
                            // Error del servidor
                            if (typeof Utils !== 'undefined' && typeof Utils.showMessage === 'function') {
                                Utils.showMessage('Error', result.message || 'Invalid credentials', 'error');
                            } else {
                                alert(result.message || 'Invalid credentials');
                            }
                            btn.disabled = false;
                            btn.innerHTML = originalText;
                        }
                    } catch (error) {
if (typeof Utils !== 'undefined' && typeof Utils.showMessage === 'function') {
                            Utils.showMessage('Error', 'Error during login: ' + error.message, 'error');
                        } else {
                            alert('Error during login: ' + error.message);
                        }
                        const btn = form.querySelector('button[type="submit"]');
                        btn.disabled = false;
                        btn.innerHTML = 'Sign In';
                    }
                });
            }
        }
    }

    /**
     * Carga el dashboard/bienvenida del usuario autenticado
     */
    loadDashboard() {
// Mostrar HTML de dashboard inline mientras se carga
        const outlet = document.getElementById('router-outlet');
        if (outlet) {
            outlet.innerHTML = `
                <div class="dashboard-container">
                    <div class="text-center">
                        <div class="spinner-border text-primary" role="status" style="margin-top: 50px;">
                            <span class="visually-hidden">Cargando...</span>
                        </div>
                        <p class="mt-3">Cargando tu dashboard...</p>
                    </div>
                </div>
            `;
        }

        // Crear instancia bajo demanda si no existe
        if (!this.dashboardComponent) {
            // Verificar si DashboardComponent está disponible
            if (typeof DashboardComponent === 'undefined') {
setTimeout(() => {
if (typeof DashboardComponent !== 'undefined') {
this.dashboardComponent = new DashboardComponent();
                        this.dashboardComponent.init();
                    } else {
outlet.innerHTML = `
                            <div class="alert alert-danger m-5">
                                <h4>Error al cargar el dashboard</h4>
                                <p>No se pudo cargar el componente de dashboard. Por favor, recarga la página.</p>
                                <button class="btn btn-primary" onclick="window.location.reload()">Recargar página</button>
                            </div>
                        `;
                    }
                }, 100);
                return;
            }
            this.dashboardComponent = new DashboardComponent();
        }
        this.dashboardComponent.init();
    }

    /**     * Carga la página de perfil del usuario
     */
    loadProfile() {
// Verificar si el usuario está autenticado
        if (!AuthService.isAuthenticated()) {
this.navigateTo('/login');
            return;
        }

        // Mostrar HTML de perfil inline mientras se carga
        const outlet = document.getElementById('router-outlet');
        if (outlet) {
            outlet.innerHTML = `
                <div class="profile-container">
                    <div class="text-center">
                        <div class="spinner-border text-primary" role="status" style="margin-top: 50px;">
                            <span class="visually-hidden">Cargando perfil...</span>
                        </div>
                        <p class="mt-3">Cargando tu perfil...</p>
                    </div>
                </div>
            `;
        }

        // Crear instancia bajo demanda si no existe
        if (!this.profileComponent) {
            // Verificar si ProfileComponent está disponible
            if (typeof ProfileComponent === 'undefined') {
setTimeout(() => {
if (typeof ProfileComponent !== 'undefined') {
this.profileComponent = new ProfileComponent();
                        this.profileComponent.init();
                    } else {
outlet.innerHTML = `
                            <div class="alert alert-danger m-5">
                                <h4>Error al cargar el perfil</h4>
                                <p>No se pudo cargar el componente de perfil. Por favor, recarga la página.</p>
                                <button class="btn btn-primary" onclick="window.location.reload()">Recargar página</button>
                            </div>
                        `;
                    }
                }, 100);
                return;
            }
            this.profileComponent = new ProfileComponent();
        }
        this.profileComponent.init();
    }

    /**     * Carga la lista de usuarios (placeholder)
     */
    loadUsers(userId) {
// Si hay un userId, cargar detalles
        if (userId) {
            return this.loadUserDetails(userId);
        }
        
        // Mostrar HTML de usuarios inline mientras se carga
        const outlet = document.getElementById('router-outlet');
        if (outlet) {
            outlet.innerHTML = `
                <div class="users-container">
                    <div class="text-center">
                        <div class="spinner-border text-primary" role="status" style="margin-top: 50px;">
                            <span class="visually-hidden">Cargando usuarios...</span>
                        </div>
                        <p class="mt-3">Cargando lista de usuarios...</p>
                    </div>
                </div>
            `;
        }

        // Crear instancia bajo demanda si no existe
        if (!this.usersComponent) {
            // Verificar si UsersComponent está disponible
            if (typeof UsersComponent === 'undefined') {
setTimeout(() => {
if (typeof UsersComponent !== 'undefined') {
this.usersComponent = new UsersComponent();
                        window.UsersComponentInstance = this.usersComponent;
                        this.usersComponent.init();
                    } else {
outlet.innerHTML = `
                            <div class="alert alert-danger m-5">
                                <h4>Error al cargar la lista de usuarios</h4>
                                <p>No se pudo cargar el componente de usuarios. Por favor, recarga la página.</p>
                                <button class="btn btn-primary" onclick="window.location.reload()">Recargar página</button>
                            </div>
                        `;
                    }
                }, 100);
                return;
            }
            this.usersComponent = new UsersComponent();
            window.UsersComponentInstance = this.usersComponent;
        }
        this.usersComponent.init();
    }

    /**
     * Carga los detalles de un usuario específico
     */
    loadUserDetails(userId) {
// DEBUGGING: Verificar qué está disponible en window
// Listar todas los objetos en AppScripts si existen
        if (typeof AppScripts !== 'undefined') {
}

        try {
            if (typeof UserDetailsComponent === 'undefined') {
throw new Error('UserDetailsComponent no esta disponible. Intenta recargar la pagina (Ctrl+Shift+R)');
            }
const component = new UserDetailsComponent();
component.init(userId);
        } catch (error) {
const outlet = document.getElementById('router-outlet');
            if (outlet) {
                outlet.innerHTML = '<div class="alert alert-danger m-5"><h5>Error</h5><p>' + error.message + '</p><a href="/users" class="btn btn-primary">Volver</a></div>';
            }
        }
    }

    /**
     * Carga la lista de customers de MyIkea o los detalles de uno específico
     */
    loadCustomers(customerId) {
        // Verificar si el usuario está autenticado
        if (!AuthService.isAuthenticated()) {
            this.navigateTo('/login');
            return;
        }

        // Verificar si el usuario es ADMIN o PREMIUM
        const userRole = AuthService.getRole();
        if (userRole !== 'ADMIN' && userRole !== 'PREMIUM') {
            const outlet = document.getElementById('router-outlet');
            if (outlet) {
                outlet.innerHTML = `
                    <div class="container py-5">
                        <div class="row justify-content-center">
                            <div class="col-md-8">
                                <div class="card shadow-sm border-warning">
                                    <div class="card-body text-center py-5">
                                        <div style="font-size: 80px; margin-bottom: 20px;">🔐</div>
                                        <h2 class="card-title fw-bold mb-3 text-danger">Acceso Denegado</h2>
                                        <p class="lead mb-3">No tienes permisos para acceder a esta sección</p>
                                        <p class="text-muted mb-4">
                                            <i class="fas fa-info-circle me-2"></i>
                                            <strong>Esta funcionalidad solo está disponible para usuarios con rol ADMIN o PREMIUM</strong>
                                        </p>
                                        <a href="/dashboard" class="btn btn-primary mt-3">
                                            <i class="fas fa-arrow-left me-2"></i>Volver al Dashboard
                                        </a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
            }
            return;
        }

        const outlet = document.getElementById('router-outlet');

        // Si hay customerId, cargar los detalles
        if (customerId) {
            // Mostrar loading
            if (outlet) {
                outlet.innerHTML = `
                    <div class="text-center py-5">
                        <div class="spinner-border text-primary" role="status">
                            <span class="visually-hidden">Cargando detalles...</span>
                        </div>
                        <p class="mt-3">Cargando detalles del cliente...</p>
                    </div>
                `;
            }

            // Cargar componente de detalles
            if (typeof CustomerDetailsComponent === 'undefined') {
                setTimeout(() => {
                    if (typeof CustomerDetailsComponent !== 'undefined') {
                        const detailComponent = new CustomerDetailsComponent();
                        detailComponent.init(customerId);
                    }
                }, 100);
                return;
            }
            const detailComponent = new CustomerDetailsComponent();
            detailComponent.init(customerId);
            return;
        }

        // Si no hay customerId, mostrar lista de customers
        if (outlet) {
            outlet.innerHTML = `
                <div class="customers-container">
                    <div class="text-center">
                        <div class="spinner-border text-primary" role="status" style="margin-top: 50px;">
                            <span class="visually-hidden">Cargando customers...</span>
                        </div>
                        <p class="mt-3">Cargando lista de customers...</p>
                    </div>
                </div>
            `;
        }

        // Crear instancia bajo demanda si no existe
        if (!this.customersComponent) {
            // Verificar si CustomersComponent está disponible
            if (typeof CustomersComponent === 'undefined') {
                setTimeout(() => {
                    if (typeof CustomersComponent !== 'undefined') {
                        this.customersComponent = new CustomersComponent();
                        window.CustomersComponentInstance = this.customersComponent;
                        this.customersComponent.init();
                    } else {
                        outlet.innerHTML = `
                            <div class="alert alert-danger m-5">
                                <h4>Error al cargar la lista de customers</h4>
                                <p>No se pudo cargar el componente de customers. Por favor, recarga la página.</p>
                                <button class="btn btn-primary" onclick="window.location.reload()">Recargar página</button>
                            </div>
                        `;
                    }
                }, 100);
                return;
            }
            this.customersComponent = new CustomersComponent();
            window.CustomersComponentInstance = this.customersComponent;
        }
        this.customersComponent.init();
    }
}

// Exponer globalmente para la verificación de carga
window.App = App;

// Registrar que este script se ha cargado
if (typeof AppScripts !== 'undefined') AppScripts.register('app');





