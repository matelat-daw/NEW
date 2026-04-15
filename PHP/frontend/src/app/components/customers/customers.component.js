/**
 * CustomersComponent.js - Componente de gestión de customers
 */

class CustomersComponent {
    constructor() {
        this.selector = '#router-outlet';
        this.currentPage = 0;
        this.pageSize = 10;
        this.customers = [];
        this.totalItems = 0;
        this.totalPages = 0;
        this.customerToDelete = null;
        this.isSearching = false;
        this.searchFirstName = '';
        this.searchLastName = '';
    }

    async init() {
        try {

            // Verificar si el usuario está autenticado
            if (!AuthService.isAuthenticated()) {
                App.getInstance().navigateTo('/login');
                return;
            }

            // Obtener el rol del usuario
            const userRole = AuthService.getRole();
            const isAdmin = userRole === 'ADMIN';
            const isPremium = userRole === 'PREMIUM';

            if (!isAdmin && !isPremium) {
                this.renderAccessDenied(userRole);
                return;
            }

            // Guardar el rol en la instancia para usarlo en other methods
            this.userRole = userRole;
            this.isAdmin = isAdmin;
            this.isPremium = isPremium;

            // Mostrar spinner mientras se cargan los datos
            const container = document.querySelector(this.selector);
            if (container) {
                container.innerHTML = `
                    <div class="text-center py-5">
                        <div class="spinner-border text-primary" role="status">
                            <span class="visually-hidden">Cargando customers...</span>
                        </div>
                        <p class="mt-3">Cargando lista de customers...</p>
                    </div>
                `;
            }

            // Cargar customers
            await this.loadCustomers();

            // Renderizar tabla
            this.renderAdminView();

            // Adjuntar event listeners
            this.attachEventListeners();
        } catch (error) {
            const container = document.querySelector(this.selector);
            if (container) {
                container.innerHTML = `
                    <div class="alert alert-danger m-5">
                        <h4>Error al cargar customers</h4>
                        <p>${error.message || 'No se pudo cargar la lista de customers'}</p>
                        <button class="btn btn-primary" onclick="location.reload()">Recargar</button>
                    </div>
                `;
            }
        }
    }

    async loadCustomers() {
        try {
            const response = await CustomerService.getCustomers(this.currentPage, this.pageSize);
            
            if (response.success) {
                // La API devuelve los clientes en response.customers
                this.customers = response.customers || [];
                const pagination = response.pagination || {};
                
                this.totalItems = pagination.totalItems || response.totalItems || 0;
                this.totalPages = pagination.totalPages || response.totalPages || 0;
                this.currentPage = pagination.currentPage || response.currentPage || 0;
                
                this.isSearching = false;
                this.searchFirstName = '';
                this.searchLastName = '';
                return;
            } else {
                throw new Error(response.message || 'Error al obtener customers');
            }
        } catch (error) {
            throw error;
        }
    }

    renderAdminView() {
        const html = this.getAdminViewHTML();
        const container = document.querySelector(this.selector);
        if (container) {
            container.innerHTML = html;
        }
    }

    getAdminViewHTML() {
        const modeText = this.isAdmin ? 'Administración' : 'Visualización';
        return `
            <div class="container-fluid py-5">
                <!-- Saludo Usuario -->
                <div class="row mb-4">
                    <div class="col-12">
                        <div class="alert alert-info alert-dismissible fade show" role="alert">
                            <i class="fas fa-user-check me-2"></i>
                            <strong>¡Bienvenido ${this.userRole}! ${modeText} de Customers de MyIkea</strong>
                            ${!this.isAdmin ? '<span class="ms-2 badge bg-warning text-dark"><i class="fas fa-eye me-1"></i>Solo lectura</span>' : ''}
                            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                        </div>
                    </div>
                </div>

                <!-- Header -->
                <div class="row mb-4">
                    <div class="col-12">
                        <h1 class="display-5 fw-bold mb-2">
                            <i class="fas fa-building me-3"></i>Customers MyIkea
                        </h1>
                        <p class="text-muted">Total de customers: <strong>${this.totalItems}</strong></p>
                    </div>
                </div>

                <!-- Barra de búsqueda -->
                <div class="row mb-4">
                    <div class="col-12">
                        <div class="card shadow-sm">
                            <div class="card-body">
                                <div class="row g-3">
                                    <div class="col-md-4">
                                        <input type="text" class="form-control" id="searchFirstName" placeholder="Buscar por nombre...">
                                    </div>
                                    <div class="col-md-4">
                                        <input type="text" class="form-control" id="searchLastName" placeholder="Buscar por apellido...">
                                    </div>
                                    <div class="col-md-4">
                                        <button class="btn btn-primary w-100" id="searchBtn">
                                            <i class="fas fa-search me-2"></i>Buscar
                                        </button>
                                        <button class="btn btn-secondary w-100 mt-2" id="resetBtn">
                                            <i class="fas fa-redo me-2"></i>Limpiar
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Tabla de customers -->
                <div class="row">
                    <div class="col-12">
                        <div class="card shadow-sm">
                            <div class="card-body">
                                ${this.customers.length > 0 ? this.renderTable() : this.renderEmpty()}
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Paginación -->
                ${this.totalPages > 1 ? this.renderPagination() : ''}
            </div>

            <!-- Modal: Confirmar eliminación -->
            <div class="modal fade" id="deleteConfirmModal" tabindex="-1" data-bs-backdrop="static" data-bs-keyboard="false">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header bg-danger bg-opacity-10 border-danger">
                            <h5 class="modal-title fw-bold text-danger">
                                <i class="fas fa-exclamation-triangle me-2"></i>Confirmar eliminación
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <p><strong>¿Estás seguro de que deseas eliminar este customer?</strong></p>
                            <p id="deleteCustomerInfo" class="text-muted"></p>
                            <div class="alert alert-warning" role="alert">
                                <i class="fas fa-warning me-2"></i>
                                <strong>Advertencia:</strong> Esta acción no se puede deshacer.
                            </div>
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
    }

    renderTable() {
        const rows = this.customers.map(customer => `
            <tr>
                <td>
                    <strong>${customer.firstName || 'N/A'}</strong>
                </td>
                <td>
                    <strong>${customer.lastName || 'N/A'}</strong>
                </td>
                <td>${customer.email || '-'}</td>
                <td>${customer.telefono || '-'}</td>
                <td>
                    <small class="text-muted">${customer.fechaDeNacimiento ? new Date(customer.fechaDeNacimiento).toLocaleDateString() : '-'}</small>
                </td>
                <td>
                    <div class="btn-group btn-group-sm" role="group">
                        <button class="btn btn-outline-primary btn-sm" onclick="CustomersComponent.viewCustomer(${customer.customerId})">
                            <i class="fas fa-eye"></i> Ver
                        </button>
                        ${this.isAdmin ? `
                            <button class="btn btn-outline-danger btn-sm" onclick="CustomersComponent.confirmDelete(${customer.customerId}, '${customer.firstName} ${customer.lastName}')">
                                <i class="fas fa-trash"></i> Eliminar
                            </button>
                        ` : `
                            <button class="btn btn-outline-secondary btn-sm" disabled title="Solo lectura">
                                <i class="fas fa-lock"></i> Solo lectura
                            </button>
                        `}
                    </div>
                </td>
            </tr>
        `).join('');

        return `
            <div class="table-responsive">
                <table class="table table-hover mb-0">
                    <thead class="table-light">
                        <tr>
                            <th>Nombre</th>
                            <th>Apellido</th>
                            <th>Email</th>
                            <th>Teléfono</th>
                            <th>Nacimiento</th>
                            <th style="width: 150px;">Acciones</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${rows}
                    </tbody>
                </table>
            </div>
        `;
    }

    renderEmpty() {
        let message = 'No hay customers registrados';
        if (this.isSearching) {
            message = 'No se encontraron customers con esos criterios de búsqueda';
        }
        return `
            <div class="text-center py-5">
                <i class="fas fa-inbox display-1 text-secondary mb-3"></i>
                <h4>${message}</h4>
                <p class="text-muted">Intenta con otros términos de búsqueda.</p>
            </div>
        `;
    }

    renderPagination() {
        if (this.totalPages <= 1) return '';

        const pages = [];
        
        // Botón anterior
        pages.push(`
            <li class="page-item ${this.currentPage === 0 ? 'disabled' : ''}">
                <a class="page-link" href="javascript:void(0)" onclick="CustomersComponent.previousPage()" data-page="${this.currentPage - 1}">
                    Anterior
                </a>
            </li>
        `);

        // Números de página (mostrar máximo 5 páginas)
        let startPage = Math.max(0, this.currentPage - 2);
        let endPage = Math.min(this.totalPages - 1, this.currentPage + 2);

        if (startPage > 0) {
            pages.push(`
                <li class="page-item">
                    <a class="page-link" href="javascript:void(0)" onclick="CustomersComponent.goToPage(0)">1</a>
                </li>
            `);
            if (startPage > 1) {
                pages.push(`<li class="page-item disabled"><span class="page-link">...</span></li>`);
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            pages.push(`
                <li class="page-item ${i === this.currentPage ? 'active' : ''}">
                    <a class="page-link" href="javascript:void(0)" onclick="CustomersComponent.goToPage(${i})" data-page="${i}">
                        ${i + 1}
                    </a>
                </li>
            `);
        }

        if (endPage < this.totalPages - 1) {
            if (endPage < this.totalPages - 2) {
                pages.push(`<li class="page-item disabled"><span class="page-link">...</span></li>`);
            }
            pages.push(`
                <li class="page-item">
                    <a class="page-link" href="javascript:void(0)" onclick="CustomersComponent.goToPage(${this.totalPages - 1})">${this.totalPages}</a>
                </li>
            `);
        }

        // Botón siguiente
        pages.push(`
            <li class="page-item ${this.currentPage === this.totalPages - 1 ? 'disabled' : ''}">
                <a class="page-link" href="javascript:void(0)" onclick="CustomersComponent.nextPage()" data-page="${this.currentPage + 1}">
                    Siguiente
                </a>
            </li>
        `);

        return `
            <div class="row mt-5">
                <div class="col-12 d-flex justify-content-center">
                    <nav>
                        <ul class="pagination">
                            ${pages.join('')}
                        </ul>
                    </nav>
                </div>
            </div>
        `;
    }

    renderAccessDenied(userRole) {
        const html = `
            <div class="container py-5">
                <div class="row mb-5">
                    <div class="col-12">
                        <h1 class="display-5 fw-bold mb-2"><i class="fas fa-building me-3"></i>Customers MyIkea</h1>
                        <p class="text-muted">Administración de customers</p>
                    </div>
                </div>
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
                                <div class="alert alert-info" role="alert">
                                    <i class="fas fa-user-circle me-2"></i>
                                    <strong>Tu rol actual:</strong> <span class="badge bg-info ms-2">${userRole || 'No especificado'}</span>
                                </div>
                                <a href="/dashboard" class="btn btn-primary mt-3">
                                    <i class="fas fa-arrow-left me-2"></i>Volver al Dashboard
                                </a>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        const container = document.querySelector(this.selector);
        if (container) {
            container.innerHTML = html;
        }
    }

    attachEventListeners() {
        // Botón de búsqueda
        const searchBtn = document.getElementById('searchBtn');
        if (searchBtn) {
            searchBtn.addEventListener('click', () => this.handleSearch());
        }

        // Botón de reset
        const resetBtn = document.getElementById('resetBtn');
        if (resetBtn) {
            resetBtn.addEventListener('click', () => this.handleReset());
        }

        // Enter en campos de búsqueda
        const searchFirstName = document.getElementById('searchFirstName');
        const searchLastName = document.getElementById('searchLastName');
        
        if (searchFirstName) {
            searchFirstName.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') this.handleSearch();
            });
        }
        if (searchLastName) {
            searchLastName.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') this.handleSearch();
            });
        }

        // Modal de confirmación de eliminación
        const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');
        if (confirmDeleteBtn) {
            confirmDeleteBtn.addEventListener('click', () => this.executeDelete());
        }
    }

    async handleSearch() {
        const firstName = document.getElementById('searchFirstName')?.value || '';
        const lastName = document.getElementById('searchLastName')?.value || '';

        if (!firstName && !lastName) {
            Utils.showMessage('Búsqueda', 'Por favor ingresa al menos un criterio de búsqueda', 'warning');
            return;
        }

        try {
            this.isSearching = true;
            this.searchFirstName = firstName;
            this.searchLastName = lastName;

            let results = [];

            if (firstName) {
                const response = await CustomerService.searchByFirstName(firstName);
                if (response.success) {
                    results = response.customers || [];
                }
            }

            if (lastName && results.length === 0) {
                const response = await CustomerService.searchByLastName(lastName);
                if (response.success) {
                    results = response.customers || [];
                }
            }

            this.customers = results;
            this.totalItems = results.length;
            this.totalPages = 1;
            this.currentPage = 0;

            this.renderAdminView();
            this.attachEventListeners();
            Utils.showMessage('Búsqueda completada', `Se encontraron ${results.length} customer(s)`, 'success');
        } catch (error) {
            Utils.showMessage('Error', 'Error durante la búsqueda', 'error');
        }
    }

    async handleReset() {
        this.currentPage = 0;
        this.isSearching = false;
        this.searchFirstName = '';
        this.searchLastName = '';
        
        const searchFirstName = document.getElementById('searchFirstName');
        const searchLastName = document.getElementById('searchLastName');
        
        if (searchFirstName) searchFirstName.value = '';
        if (searchLastName) searchLastName.value = '';

        await this.loadCustomers();
        this.renderAdminView();
        this.attachEventListeners();
    }

    // Métodos estáticos para paginación
    static async goToPage(pageNumber) {
        const component = this.getInstance();
        component.currentPage = pageNumber;
        await component.loadCustomers();
        component.renderAdminView();
        component.attachEventListeners();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }

    static async nextPage() {
        const component = this.getInstance();
        if (component.currentPage < component.totalPages - 1) {
            component.currentPage++;
            await component.loadCustomers();
            component.renderAdminView();
            component.attachEventListeners();
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    }

    static async previousPage() {
        const component = this.getInstance();
        if (component.currentPage > 0) {
            component.currentPage--;
            await component.loadCustomers();
            component.renderAdminView();
            component.attachEventListeners();
            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    }

    static confirmDelete(customerId, customerName) {
        const component = this.getInstance();
        component.customerToDelete = customerId;
        
        const modal = new bootstrap.Modal(document.getElementById('deleteConfirmModal'));
        document.getElementById('deleteCustomerInfo').textContent = `Customer: ${customerName} (ID: ${customerId})`;
        modal.show();
    }

    static async viewCustomer(customerId) {
        App.getInstance().navigateTo(`/customers/${customerId}`);
    }

    async executeDelete() {
        if (!this.customerToDelete) return;

        try {
            const response = await CustomerService.deleteCustomer(this.customerToDelete);
            if (response.success) {
                Utils.showMessage('Éxito', 'El customer fue eliminado correctamente', 'success');
                
                // Cerrar modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('deleteConfirmModal'));
                modal.hide();

                // Recargar lista
                this.customerToDelete = null;
                await this.loadCustomers();
                this.renderAdminView();
                this.attachEventListeners();
            } else {
                Utils.showMessage('Error', response.message || 'Error al eliminar el customer', 'error');
            }
        } catch (error) {
            Utils.showMessage('Error', 'Error al eliminar el customer', 'error');
        }
    }

    static getInstance() {
        if (!window._customersComponent) {
            window._customersComponent = new CustomersComponent();
        }
        return window._customersComponent;
    }
}

// Exponer globalmente
window.CustomersComponent = CustomersComponent;

// Registrar que este script se ha cargado
if (typeof AppScripts !== 'undefined') AppScripts.register('customers');
