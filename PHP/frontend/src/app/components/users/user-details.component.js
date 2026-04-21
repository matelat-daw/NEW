// user-details.component.js
class UserDetailsComponent {
    constructor() {
this.selector = '#router-outlet';
        this.userId = null;
        this.user = null;
    }

    async init(userId) {
try {
            this.userId = userId;
            
            if (!AuthService.isAuthenticated()) {
App.getInstance().navigateTo('/login');
                return;
            }
            
            const userRole = AuthService.getRole();
            if (userRole !== 'ADMIN' && userRole !== 'PREMIUM') {
this.show('ERROR: No tienes permisos para ver esto');
                return;
            }
            
            this.show('Cargando...');
            await this.loadUser();
            this.show(this.render());
            this.setupListeners();
        } catch (e) {
this.show('ERROR: ' + e.message);
        }
    }

    show(html) {
        const el = document.querySelector(this.selector);
        if (el) el.innerHTML = html;
    }

    async loadUser() {
const res = await UserService.getUserById(this.userId);
        this.user = res && res.data ? res.data : res.user;
        if (!this.user) throw new Error('Usuario no encontrado');
}

    render() {
        const admin = AuthService.getUserSession();
        const adminName = admin ? `${admin.name} ${admin.surname1}` : 'Admin';
        const currentUserRole = AuthService.getRole();
        const isAdmin = currentUserRole === 'ADMIN';
        
        // Determinar imagen del usuario
        const userImg = this.user.profileImg ? 
            `/${this.user.profileImg}` :
            null;
        
        return `
<div class="container py-5">
    <!-- Header y Botón Volver -->
    <div class="row mb-4">
        <div class="col-12 d-flex justify-content-between align-items-center">
            <div>
                <h1 class="display-5 fw-bold mb-0">
                    <i class="fas fa-user-id me-3 text-primary"></i>Detalles de Usuario
                </h1>
                <p class="text-muted mt-2">Gestionando el perfil de <strong>${this.user.nick}</strong></p>
            </div>
            <a href="/users" class="btn btn-outline-secondary">
                <i class="fas fa-arrow-left me-2"></i>Volver a la lista
            </a>
        </div>
    </div>

    <!-- Contenido Principal -->
    <div class="row">
        <!-- Columna Izquierda: Foto y Resumen -->
        <div class="col-lg-4 mb-4">
            <div class="card shadow-sm text-center">
                <div class="card-body py-5">
                    <div class="mb-4">
                        ${userImg ? 
                            `<img src="${userImg}" alt="${this.user.nick}" class="img-thumbnail rounded-circle shadow" style="width: 150px; height: 150px; object-fit: cover; border: 5px solid #fff;">` :
                            `<div class="bg-light rounded-circle d-inline-flex align-items-center justify-content-center shadow" style="width: 150px; height: 150px; border: 5px solid #fff;">
                                <i class="fas fa-user fa-5x text-secondary"></i>
                            </div>`
                        }
                    </div>
                    <h3 class="fw-bold mb-1">${this.user.name} ${this.user.surname1}</h3>
                    <p class="text-muted mb-3">@${this.user.nick}</p>
                    <span class="badge ${this.getRoleBadgeClass(this.user.role)} fs-6 px-3 py-2">
                        ${this.user.role || 'USER'}
                    </span>
                    <hr class="my-4">
                    <div class="d-grid">
                        ${isAdmin ? `
                            <button type="button" id="deleteBtn" class="btn btn-danger">
                                <i class="fas fa-trash-alt me-2"></i>Eliminar Cuenta
                            </button>
                        ` : `
                            <button type="button" class="btn btn-secondary" disabled title="Solo lectora">
                                <i class="fas fa-lock me-2"></i>Solo Lectura
                            </button>
                        `}
                    </div>
                </div>
            </div>
        </div>

        <!-- Columna Derecha: Datos Detallados -->
        <div class="col-lg-8">
            <div class="card shadow-sm h-100">
                <div class="card-header bg-white py-3">
                    <h5 class="card-title mb-0 fw-bold">
                        <i class="fas fa-info-circle me-2 text-primary"></i>Información Completa
                    </h5>
                </div>
                <div class="card-body">
                    <div class="row g-4">
                        <div class="col-md-6">
                            <label class="text-muted small text-uppercase fw-bold">Email</label>
                            <p class="fs-5"><i class="fas fa-envelope me-2 text-muted"></i>${this.user.email}</p>
                        </div>
                        <div class="col-md-6">
                            <label class="text-muted small text-uppercase fw-bold">Teléfono</label>
                            <p class="fs-5"><i class="fas fa-phone me-2 text-muted"></i>${this.user.phone || 'No proporcionado'}</p>
                        </div>
                        <div class="col-md-6">
                            <label class="text-muted small text-uppercase fw-bold">Género</label>
                            <p class="fs-5">
                                <i class="fas fa-venus-mars me-2 text-muted"></i>${this.user.gender || 'No especificado'}
                            </p>
                        </div>
                        <div class="col-md-6">
                            <label class="text-muted small text-uppercase fw-bold">Fecha de Nacimiento</label>
                            <p class="fs-5">
                                <i class="fas fa-calendar-alt me-2 text-muted"></i>${this.user.bday ? new Date(this.user.bday).toLocaleDateString() : 'N/A'}
                            </p>
                        </div>
                        <div class="col-md-12"><hr></div>
                        <div class="col-md-6">
                            <label class="text-muted small text-uppercase fw-bold">Estado de Cuenta</label>
                            <p class="fs-5">
                                <span class="badge ${this.user.active ? 'bg-success' : 'bg-secondary'} me-2">
                                    ${this.user.active ? 'ACTIVO' : 'INACTIVO'}
                                </span>
                                <span class="badge ${this.user.emailVerified ? 'bg-primary' : 'bg-warning'}">
                                    ${this.user.emailVerified ? 'EMAIL VERIFICADO' : 'PENDIENTE VERIFICAR'}
                                </span>
                            </p>
                        </div>
                        <div class="col-md-6">
                            <label class="text-muted small text-uppercase fw-bold">Fecha de Registro</label>
                            <p class="fs-5"><i class="fas fa-clock me-2 text-muted"></i>${new Date(this.user.createdAt).toLocaleString()}</p>
                        </div>
                    </div>
                </div>
                <div class="card-footer bg-light py-3">
                    <small class="text-muted">
                        <i class="fas fa-history me-2"></i>Última actualización: ${new Date(this.user.updatedAt).toLocaleString()}
                    </small>
                </div>
            </div>
        </div>
    </div>
</div>
`;
    }

    getRoleBadgeClass(role) {
        const roleBadgeMap = {
            'ADMIN': 'bg-danger',
            'USER': 'bg-info',
            'PREMIUM': 'bg-success',
            'MODERATOR': 'bg-warning'
        };
        return roleBadgeMap[role] || 'bg-secondary';
    }

    setupListeners() {
        const btn = document.getElementById('deleteBtn');
        if (btn) {
            btn.addEventListener('click', () => {
                this.showDeleteConfirmModal();
            });
        }
    }

    showDeleteConfirmModal() {
        const modalHtml = `
            <div class="modal fade" id="deleteUserModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content border-danger">
                        <div class="modal-header bg-danger text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-trash me-2"></i>Confirmar Eliminación
                            </h5>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <div class="alert alert-warning mb-3" role="alert">
                                <strong>¡Advertencia!</strong> Esta acción es irreversible.
                            </div>
                            <p>¿Estás seguro de que deseas eliminar al usuario <strong>${this.user.nick}</strong>?</p>
                            <p class="text-muted small">Una vez eliminado, no se puede recuperar.</p>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                            <button type="button" class="btn btn-danger" id="confirmDeleteBtn">
                                <i class="fas fa-trash me-2"></i>Sí, eliminar
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Remover modal anterior si existe
        const existingModal = document.getElementById('deleteUserModal');
        if (existingModal) {
            existingModal.remove();
        }

        // Agregar modal al DOM
        document.body.insertAdjacentHTML('beforeend', modalHtml);

        // Mostrar modal
        const modal = new bootstrap.Modal(document.getElementById('deleteUserModal'));
        modal.show();

        // Adjuntar evento al botón de confirmación
        const confirmBtn = document.getElementById('confirmDeleteBtn');
        if (confirmBtn) {
            confirmBtn.addEventListener('click', async () => {
                modal.hide();
                await this.delete();
            });
        }
    }

    async delete() {
        try {
            const confirmBtn = document.getElementById('confirmDeleteBtn');
            if (confirmBtn) {
                confirmBtn.disabled = true;
                confirmBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Eliminando...';
            }

            const res = await UserService.deleteUser(this.user.id);
            if (res && res.success) {
                // Cerrar modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('deleteUserModal'));
                if (modal) {
                    modal.hide();
                }

                // Mostrar notificación de éxito
                this.showSuccessNotification('Usuario eliminado exitosamente');

                // Redirigir después de 1 segundo
                setTimeout(() => {
                    App.getInstance().navigateTo('/users');
                }, 1000);
            } else {
                // Mostrar error
                const errorMsg = res ? res.message : 'desconocido';
                this.showErrorNotification('Error al eliminar: ' + errorMsg);
                
                // Habilitar botón nuevamente
                if (confirmBtn) {
                    confirmBtn.disabled = false;
                    confirmBtn.innerHTML = '<i class="fas fa-trash me-2"></i>Sí, eliminar';
                }
            }
        } catch (e) {
            this.showErrorNotification('Error: ' + e.message);
            const confirmBtn = document.getElementById('confirmDeleteBtn');
            if (confirmBtn) {
                confirmBtn.disabled = false;
                confirmBtn.innerHTML = '<i class="fas fa-trash me-2"></i>Sí, eliminar';
            }
        }
    }

    showSuccessNotification(message) {
        const alertHtml = `
            <div class="alert alert-success alert-dismissible fade show position-fixed" role="alert" 
                 style="top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
                <i class="fas fa-check-circle me-2"></i>
                <strong>Éxito!</strong> ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;
        document.body.insertAdjacentHTML('beforeend', alertHtml);

        // Auto-hide después de 3 segundos
        setTimeout(() => {
            const alerts = document.querySelectorAll('.alert-success.position-fixed');
            alerts.forEach(alert => alert.remove());
        }, 3000);
    }

    showErrorNotification(message) {
        const alertHtml = `
            <div class="alert alert-danger alert-dismissible fade show position-fixed" role="alert" 
                 style="top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
                <i class="fas fa-exclamation-circle me-2"></i>
                <strong>Error!</strong> ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;
        document.body.insertAdjacentHTML('beforeend', alertHtml);

        // Auto-hide después de 5 segundos
        setTimeout(() => {
            const alerts = document.querySelectorAll('.alert-danger.position-fixed');
            alerts.forEach(alert => alert.remove());
        }, 5000);
    }
}
window.UserDetailsComponent = UserDetailsComponent;
if (typeof AppScripts !== 'undefined') {
AppScripts.register('user-details');
} else {
}


