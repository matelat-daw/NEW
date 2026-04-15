/**
 * navbar.component.js - Componente de la barra de navegación
 */

class NavbarComponent {
    constructor() {
        this.selector = '#navbar-container';
    }

    /**
     * Reinicializa el navbar (para cambios de autenticación)
     * Solo actualiza el estado sin recargar el HTML
     */
    async reinit() {
        try {
            // No recargar el HTML, solo actualizar el estado de autenticación
            this.updateAuthStatus();
            this.loadUserData();
        } catch (error) {
        }
    }

    /**
     * Inicializa el componente
     */
    async init() {
        try {
            const response = await fetch('/frontend/src/app/components/navbar/navbar.component.html');
            const html = await response.text();
            
            const container = document.querySelector(this.selector);
            if (container) {
                // Resetear el marcador de listeners ANTES de cambiar el HTML
                container.dataset.listenersAttached = 'false';
                container.innerHTML = html;
                // Usar setTimeout para asegurar que AuthService esté cargado
                // 200ms es suficiente para que todos los scripts se ejecuten
                setTimeout(() => {
                    this.updateAuthStatus();
                    this.attachEventListeners();
                }, 200);
            }
        } catch (error) {
        }
    }

    /**
     * Actualiza el estado de autenticación del navbar
     */
    updateAuthStatus() {
        // Validar que AuthService esté definido - SIN reintentos, solo una verificación
        if (typeof AuthService === 'undefined') {
this.showUnauthenticatedView();
            return;
        }

        const isAuthenticated = AuthService.isAuthenticated();
        
        // Mostrar/ocultar elementos según autenticación
        const guestLinks = document.getElementById('guestLinks');
        const registerLink = document.getElementById('registerLink');
        const loginLink = document.getElementById('loginLink');
        const authenticatedLinks = document.getElementById('authenticatedLinks');
        const usersLink = document.getElementById('usersLink');
        const customersLink = document.getElementById('customersLink');
        const userDropdown = document.getElementById('userDropdown');

        if (isAuthenticated) {
            // Usuario autenticado - mostrar panel de usuario
            if (guestLinks) guestLinks.style.display = 'none';
            if (registerLink) registerLink.style.display = 'none';
            if (loginLink) loginLink.style.display = 'none';
            if (authenticatedLinks) authenticatedLinks.style.display = 'block';
            if (userDropdown) userDropdown.style.display = 'block';

            // Mostrar users y customers links si es ADMIN o PREMIUM
            const userRole = AuthService.getRole();
            if (usersLink) {
                usersLink.style.display = (userRole === 'ADMIN' || userRole === 'PREMIUM') ? 'block' : 'none';
            }
            if (customersLink) {
                customersLink.style.display = (userRole === 'ADMIN' || userRole === 'PREMIUM') ? 'block' : 'none';
            }

            // Cargar datos del usuario
            this.loadUserData();
        } else {
            // Usuario no autenticado - mostrar links de login/register
            if (guestLinks) guestLinks.style.display = 'block';
            if (registerLink) registerLink.style.display = 'block';
            if (loginLink) loginLink.style.display = 'block';
            if (authenticatedLinks) authenticatedLinks.style.display = 'none';
            if (usersLink) usersLink.style.display = 'none';
            if (customersLink) customersLink.style.display = 'none';
            if (userDropdown) userDropdown.style.display = 'none';
        }
    }

    /**
     * Muestra la vista de no autenticado (cuando AuthService no está disponible)
     */
    showUnauthenticatedView() {
        const guestLinks = document.getElementById('guestLinks');
        const registerLink = document.getElementById('registerLink');
        const loginLink = document.getElementById('loginLink');
        const authenticatedLinks = document.getElementById('authenticatedLinks');
        const usersLink = document.getElementById('usersLink');
        const customersLink = document.getElementById('customersLink');
        const userDropdown = document.getElementById('userDropdown');

        if (guestLinks) guestLinks.style.display = 'block';
        if (registerLink) registerLink.style.display = 'block';
        if (loginLink) loginLink.style.display = 'block';
        if (authenticatedLinks) authenticatedLinks.style.display = 'none';
        if (usersLink) usersLink.style.display = 'none';
        if (customersLink) customersLink.style.display = 'none';
        if (userDropdown) userDropdown.style.display = 'none';
    }

    /**
     * Carga los datos del usuario en el navbar
     */
    loadUserData() {
        try {
            
            // Validar que AuthService esté definido
            if (typeof AuthService === 'undefined') {
                return;
            }

            const user = AuthService.getUserSession();
            if (!user) {
                return;
            }

            // Actualizar foto de perfil - SIMPLE, SIN ERRORS HANDLERS INNECESARIOS
            const profilePic = document.getElementById('navbarProfilePic');
            if (profilePic) {
                const imageUrl = AuthService.getProfilePictureUrl();
                
                if (imageUrl) {
                    // Usuario tiene foto - mostrarla con versionado
                    const version = window.__profileImageVersion || Date.now();
                    const sep = imageUrl.includes('?') ? '&' : '?';
                    const finalUrl = `${imageUrl}${sep}v=${version}`;
                    
                    profilePic.src = finalUrl;
                } else {
                    // Sin foto - vacío
                    profilePic.src = '';
                }
            } else {
            }

            // Actualizar nickname
            const userNick = document.getElementById('navbarUserNick');
            if (userNick) {
                userNick.textContent = user.nick || 'User';
            }

            // Actualizar nombre completo en dropdown
            const fullName = document.getElementById('dropdownFullName');
            if (fullName) {
                fullName.textContent = AuthService.getFullName();
            }
            
        } catch (error) {
        }
    }

    /**
     * Adjunta eventos a los elementos del navbar (SIN duplicados)
     */
    attachEventListeners() {
        // Marcar para evitar listeners duplicados
        const navbarContainer = document.getElementById('navbar-container');
        if (navbarContainer && navbarContainer.dataset.listenersAttached === 'true') {
            return;
        }
        if (navbarContainer) {
            navbarContainer.dataset.listenersAttached = 'true';
        }

        // Eventos de dropdown de usuario
        const logoutLink = document.getElementById('logoutLink');
        if (logoutLink) {
            logoutLink.addEventListener('click', (e) => this.handleLogout(e));
        }

        // Evento para confirmar logout en el modal
        const confirmLogoutBtn = document.getElementById('confirmLogoutBtn');
        if (confirmLogoutBtn) {
            confirmLogoutBtn.addEventListener('click', () => this.confirmLogoutAction());
        }

        // Cerrar dropdown al hacer click en un enlace (en mobile)
        const dropdownItems = document.querySelectorAll('.dropdown-item');
        dropdownItems.forEach(item => {
            item.addEventListener('click', () => {
                const dropdown = document.querySelector('.navbar-collapse');
                if (dropdown && dropdown.classList.contains('show')) {
                    const toggler = document.querySelector('.navbar-toggler');
                    toggler.click();
                }
            });
        });

        // Adjuntar click handlers a los nav links
        const navLinks = document.querySelectorAll('.navbar-nav .nav-link');
        navLinks.forEach(link => {
            link.addEventListener('click', () => {
                this.setActiveLink(link);
            });
        });
    }

    /**
     * Maneja el logout del usuario - Muestra el modal de confirmación
     * @param {Event} e - Evento del click
     */
    handleLogout(e) {
        e.preventDefault();

        // Validar que AuthService esté definido
        if (typeof AuthService === 'undefined') {
return;
        }

        // Mostrar modal de confirmación
        const logoutModal = document.getElementById('logoutConfirmModal');
        if (logoutModal) {
            const modal = new bootstrap.Modal(logoutModal);
            modal.show();
        }
    }

    /**
     * Expone la apertura del modal de logout para otros componentes.
     */
    showLogoutModal() {
        const logoutModal = document.getElementById('logoutConfirmModal');
        if (logoutModal) {
            const modal = new bootstrap.Modal(logoutModal);
            modal.show();
        }
    }

    /**
     * Realiza el logout después de confirmar en el modal
     */
    confirmLogoutAction() {
        AuthService.logout();
        
        // Limpiar instancias de componentes
        const app = App.getInstance();
        if (app && typeof app.clearComponentInstances === 'function') {
            app.clearComponentInstances();
        }

        // Refrescar el navbar para mostrar la vista de invitado sin destruir el HTML
        if (typeof NavBar !== 'undefined' && NavBar && typeof NavBar.reinit === 'function') {
            NavBar.reinit();
        }
        
        Utils.showMessage(
            'Logged Out',
            'You have been successfully logged out.',
            'success'
        );

        // Ocultar modal
        const logoutModal = document.getElementById('logoutConfirmModal');
        if (logoutModal) {
            const modal = bootstrap.Modal.getInstance(logoutModal);
            if (modal) modal.hide();
        }

        // Redirigir a home
        setTimeout(() => {
            App.getInstance().navigateTo('/');
        }, 300);
    }

    /**
     * Establece el enlace activo en el navbar
     * @param {HTMLElement} link - Elemento de enlace
     */
    setActiveLink(link) {
        const navLinks = document.querySelectorAll('.navbar-nav .nav-link');
        navLinks.forEach(l => l.classList.remove('active'));
        link.classList.add('active');
    }

    /**
     * Actualiza el enlace activo según la ruta actual
     * @param {string} route - Ruta actual
     */
    updateActiveLink(route) {
        const routeMap = {
            'home': '.nav-link-home',
            'register': '.nav-link-register',
            'login': '.nav-link-login',
            'dashboard': '.nav-link-dashboard',
            'users': '.nav-link-users'
        };

        const selector = routeMap[route] || '.nav-link-home';
        const link = document.querySelector(selector);
        if (link) {
            this.setActiveLink(link);
        }
    }
}

// Crear instancia global de manera segura
let NavBar = null;
try {
    NavBar = new NavbarComponent();
    // Exponerla también como window.NavBarComponentInstance para mejor accesibilidad
    window.NavBarComponentInstance = NavBar;
} catch (error) {
    NavBar = null;
}

// Exponer globalmente para la verificación de carga
window.NavbarComponent = NavbarComponent;

// Registrar que este script se ha cargado
if (typeof AppScripts !== 'undefined') AppScripts.register('navbar');



