/**
 * dashboard.component.js - Componente de Dashboard/Welcome
 */

class DashboardComponent {
    constructor() {
        this.selector = '#router-outlet';
    }

    /**
     * Inicializa el componente
     */
    async init() {
        try {
            // Verificar si el usuario está autenticado
            if (!AuthService.isAuthenticated()) {
                // Redirigir al login si no está autenticado
                App.getInstance().navigateTo('/login');
                return;
            }

            const response = await fetch('/frontend/src/app/components/dashboard/dashboard.component.html');
            const html = await response.text();
            
            const container = document.querySelector(this.selector);
            if (container) {
                container.innerHTML = html;
                
                // Cargar datos del usuario (esperar a que termine)
                await this.loadUserData();
                
                // Adjuntar eventos
                this.attachEventListeners();
            }
        } catch (error) {
Utils.showMessage('Error', 'No se pudo cargar el dashboard', 'error');
        }
    }

    /**
     * Carga los datos del usuario en la página
     */
    async loadUserData() {
        let user = AuthService.getUserSession();
        
        if (!user) {
            // Si no hay sesión, no hacer nada
            return;
        }

        // Intentar refrescar los datos desde la API
        try {
            const url = API_CONFIG.BASE_URL + API_CONFIG.ENDPOINTS.PROFILE;
            const response = await Utils.makeRequest('GET', url);
            
            if (response.success && response.data) {
                // Actualizar sesión con datos frescos del servidor
                AuthService.setUserSession(response.data);
                user = response.data;
            }
        } catch (error) {
            // Si falla el refresh, usar datos del sessionStorage
        }

        // Actualizar nombre en bienvenida
        const fullName = AuthService.getFullName();
        const userNameDisplay = document.getElementById('userNameDisplay');
        if (userNameDisplay) {
            userNameDisplay.innerHTML = `<strong>${user.nick}</strong> / ${fullName}`;
        }

        // Actualizar foto de perfil con lazy loading y fallback
        const profilePictureImg = document.getElementById('profilePictureImg');
        if (profilePictureImg) {
            profilePictureImg.loading = 'lazy';
            const photoUrl = AuthService.getProfilePictureUrl();
            
            // Añadir versión para evitar cache agresivo tras cambios
            if (photoUrl) {
                const version = window.__profileImageVersion || Date.now();
                const sep = photoUrl.includes('?') ? '&' : '?';
                profilePictureImg.src = `${photoUrl}${sep}v=${version}`;
            }

            // Fallback si la imagen falla (404 o red)
            profilePictureImg.onerror = () => {
                profilePictureImg.src = '/frontend/src/assets/img/default-avatar.png';
                profilePictureImg.onerror = null; // Evitar bucle infinito
            };
        }

        // Actualizar información del usuario
        const nickDisplay = document.getElementById('nickDisplay');
        if (nickDisplay) {
            nickDisplay.textContent = user.nick || 'N/A';
        }

        const emailDisplay = document.getElementById('emailDisplay');
        if (emailDisplay) {
            emailDisplay.textContent = user.email || 'N/A';
        }

        const fullNameDisplay = document.getElementById('fullNameDisplay');
        if (fullNameDisplay) {
            fullNameDisplay.textContent = fullName || 'N/A';
        }

        const roleDisplay = document.getElementById('roleDisplay');
        if (roleDisplay) {
            const role = AuthService.getRole();
            roleDisplay.textContent = role;
            
            // Cambiar color según rol
            roleDisplay.className = 'badge';
            if (role === 'ADMIN') {
                roleDisplay.classList.add('bg-danger');
            } else if (role === 'MODERATOR' || role === 'PREMIUM') {
                roleDisplay.classList.add('bg-warning');
                if (role === 'PREMIUM') roleDisplay.classList.add('text-dark');
            } else {
                roleDisplay.classList.add('bg-info');
            }

            // Mostrar/ocultar enlaces de ADMIN y PREMIUM en Quick Links
            const adminUsersLink = document.getElementById('adminUsersLink');
            const adminCustomersLink = document.getElementById('adminCustomersLink');
            const isAdmin = role === 'ADMIN';
            const isPremium = role === 'PREMIUM';
            
            if (adminUsersLink) {
                adminUsersLink.style.display = (isAdmin || isPremium) ? 'block' : 'none';
            }
            if (adminCustomersLink) {
                adminCustomersLink.style.display = (isAdmin || isPremium) ? 'block' : 'none';
            }

            // Ajustar tamaño de tarjetas según rol
            const quickLinksRow = document.getElementById('quickLinksRow');
            if (quickLinksRow) {
                const columns = quickLinksRow.querySelectorAll('[class*="col-"]');
                columns.forEach(col => {
                    // Limpiar estilos previos
                    col.style.flex = '';
                    col.classList.remove('col-md-3', 'col-md-4');

                    if (isAdmin || isPremium) {
                        // ADMIN y PREMIUM: 5 tarjetas en una línea (20% cada una)
                        col.style.flex = '0 0 20%';
                        col.classList.add('col-md-3');
                    } else {
                        // USER: Tarjetas más grandes (3 por línea = 33.33%)
                        col.style.flex = '0 0 33.333%';
                        col.classList.add('col-md-4');
                    }
                });
            }

            // Guardar rol en sessionStorage para usar en otros componentes
            sessionStorage.setItem('userRole', role);
        }
    }

    /**
     * Adjunta eventos a los elementos de la página (SIN duplicados)
     */
    attachEventListeners() {
        const logoutBtn = document.getElementById('logoutBtn');
        if (!logoutBtn || logoutBtn.dataset.listenersAttached === 'true') {
            return;
        }
        logoutBtn.dataset.listenersAttached = 'true';
        
        logoutBtn.addEventListener('click', (e) => this.handleLogout(e));
    }

    /**
     * Maneja el logout del usuario
     * @param {Event} e - Evento del click
     */
    handleLogout(e) {
        e.preventDefault();

        if (typeof NavBar !== 'undefined' && NavBar && typeof NavBar.showLogoutModal === 'function') {
            NavBar.showLogoutModal();
            return;
        }

        const logoutModal = document.getElementById('logoutConfirmModal');
        if (logoutModal) {
            const modal = new bootstrap.Modal(logoutModal);
            modal.show();
        }
    }
}

// Exponer globalmente para la verificación de carga
window.DashboardComponent = DashboardComponent;

// Registrar que este script se ha cargado
if (typeof AppScripts !== 'undefined') AppScripts.register('dashboard');


