/**
 * customer-details.component.js - Componente para ver detalles de un cliente MyIkea
 */
class CustomerDetailsComponent {
    constructor() {
        this.selector = '#router-outlet';
        this.customerId = null;
        this.customer = null;
    }

    async init(customerId) {
        try {
            this.customerId = customerId;
            
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
            await this.loadCustomer();
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

    async loadCustomer() {
        const res = await CustomerService.getCustomerById(this.customerId);
        this.customer = res && res.customer ? res.customer : res.data;
        if (!this.customer) throw new Error('Cliente no encontrado');
    }

    render() {
        const currentUser = AuthService.getUserSession();
        const currentUserName = currentUser ? `${currentUser.name} ${currentUser.surname1}` : 'Usuario';
        const userRole = AuthService.getRole();
        const isAdmin = userRole === 'ADMIN';

        const formattedDate = this.customer.fechaDeNacimiento 
            ? new Date(this.customer.fechaDeNacimiento).toLocaleDateString('es-ES')
            : 'No especificada';

        return `
<div class="container py-5">
    <!-- Header y Botón Volver -->
    <div class="row mb-4">
        <div class="col-12 d-flex justify-content-between align-items-center">
            <div>
                <h1 class="display-5 fw-bold mb-0">
                    <i class="fas fa-building me-3 text-success"></i>Detalles del Cliente
                </h1>
                <p class="text-muted mt-2">Cliente de MyIkea: <strong>${this.customer.firstName} ${this.customer.lastName}</strong></p>
            </div>
            <a href="/customers" class="btn btn-outline-secondary">
                <i class="fas fa-arrow-left me-2"></i>Volver a la lista
            </a>
        </div>
    </div>

    <!-- Contenido Principal -->
    <div class="row">
        <!-- Columna Izquierda: Datos Principales -->
        <div class="col-lg-4 mb-4">
            <div class="card shadow-sm text-center">
                <div class="card-body py-5">
                    <div class="mb-4">
                        <div class="bg-success rounded-circle d-inline-flex align-items-center justify-content-center shadow" style="width: 150px; height: 150px; border: 5px solid #fff;">
                            <i class="fas fa-user-tie fa-5x text-white"></i>
                        </div>
                    </div>
                    <h3 class="fw-bold mb-1">${this.customer.firstName}</h3>
                    <p class="fw-bold text-muted">${this.customer.lastName}</p>
                    <span class="badge bg-success fs-6 px-3 py-2">Cliente Activo</span>
                    <hr class="my-4">
                    <div class="d-grid">
                        ${isAdmin ? `
                            <button type="button" id="deleteBtn" class="btn btn-danger">
                                <i class="fas fa-trash-alt me-2"></i>Eliminar Cliente
                            </button>
                        ` : `
                            <button type="button" class="btn btn-secondary" disabled title="Solo lectura">
                                <i class="fas fa-lock me-2"></i>Solo Lectura
                            </button>
                        `}
                    </div>
                </div>
            </div>
        </div>

        <!-- Columna Derecha: Datos Detallados -->
        <div class="col-lg-8">
            <div class="card shadow-sm">
                <div class="card-header bg-success text-white">
                    <h5 class="mb-0"><i class="fas fa-info-circle me-2"></i>Información Completa</h5>
                </div>
                <div class="card-body">
                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label class="form-label text-muted">ID del Cliente</label>
                            <p class="fw-bold">${this.customer.customerId}</p>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label text-muted">Nombre Completo</label>
                            <p class="fw-bold">${this.customer.firstName} ${this.customer.lastName}</p>
                        </div>
                    </div>

                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label class="form-label text-muted">Email</label>
                            <p class="fw-bold"><a href="mailto:${this.customer.email}">${this.customer.email}</a></p>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label text-muted">Teléfono</label>
                            <p class="fw-bold">${this.customer.telefono || 'No especificado'}</p>
                        </div>
                    </div>

                    <div class="row mb-3">
                        <div class="col-md-6">
                            <label class="form-label text-muted">Fecha de Nacimiento</label>
                            <p class="fw-bold">${formattedDate}</p>
                        </div>
                    </div>

                    <hr>

                    <div class="alert alert-info" role="alert">
                        <i class="fas fa-info-circle me-2"></i>
                        <strong>Administrador:</strong> ${currentUserName}
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- Modal: Confirmar eliminación -->
<div class="modal fade" id="deleteCustomerModal" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content border-danger">
            <div class="modal-header bg-danger text-white">
                <h5 class="modal-title">
                    <i class="fas fa-trash me-2"></i>Confirmar Eliminación
                </h5>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
                <p><strong>¿Estás seguro de que deseas eliminar este cliente?</strong></p>
                <p class="text-danger"><i class="fas fa-warning me-2"></i>Esta acción no se puede deshacer</p>
                <div class="bg-light p-3 rounded">
                    <p class="mb-1"><strong>Cliente:</strong> ${this.customer.firstName} ${this.customer.lastName}</p>
                    <p class="mb-0"><strong>Email:</strong> ${this.customer.email}</p>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                <button type="button" class="btn btn-danger" id="confirmDeleteBtn">
                    <i class="fas fa-trash me-2"></i>Sí, eliminar cliente
                </button>
            </div>
        </div>
    </div>
</div>
        `;
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
        const modal = new bootstrap.Modal(document.getElementById('deleteCustomerModal'));
        
        const confirmBtn = document.getElementById('confirmDeleteBtn');
        if (confirmBtn) {
            confirmBtn.addEventListener('click', async () => {
                await this.executeDelete();
            }, { once: true });
        }
        
        modal.show();
    }

    async executeDelete() {
        try {
            const response = await CustomerService.deleteCustomer(this.customerId);
            if (response.success) {
                Utils.showMessage('Éxito', 'Cliente eliminado correctamente', 'success');
                
                // Cerrar modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('deleteCustomerModal'));
                modal.hide();
                
                // Redirigir a la lista después de 1.5 segundos
                setTimeout(() => {
                    App.getInstance().navigateTo('/customers');
                }, 1500);
            } else {
                Utils.showMessage('Error', response.message || 'No se pudo eliminar el cliente', 'error');
            }
        } catch (error) {
            console.error('Error eliminando customer:', error);
            Utils.showMessage('Error', 'Ocurrió un error al eliminar el cliente', 'error');
        }
    }
}

// Registrar el componente globalmente
window.CustomerDetailsComponent = CustomerDetailsComponent;

// Registrar que este script se ha cargado
if (typeof AppScripts !== 'undefined') AppScripts.register('customer-details');
