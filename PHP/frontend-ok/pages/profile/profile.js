// Profile Page
var profilePage = {
    currentUser: null,
    _deleteOpener: null,
    
    init: function() {
        // Verificar si está logueado
        if (!window.authService || !window.authService.isLoggedIn()) {
            window.app.loadPage('login');
            return;
        }
        
        this.currentUser = window.authService.getUser();
        this._adminContractsLoaded = false; // Resetear flag para permitir recarga fresca
        this.loadUserData();
        // Solo cargar datos de contrato si no es admin
        if (!this.currentUser.roles || !this.currentUser.roles.includes('admin')) {
            this.loadContractData();
        }
        this.setupForms();
        this._setupModalKeyboard();
        this._setupTabKeyboard();
    },

    /* Accesibilidad: Escape cierra el modal */
    _setupModalKeyboard: function() {
        document.addEventListener('keydown', function(e) {
            var modal = document.getElementById('delete-modal');
            if (!modal || modal.style.display === 'none') return;
            if (e.key === 'Escape') {
                profilePage.closeDeleteModal();
            }
            // Trampa de foco dentro del modal
            if (e.key === 'Tab') {
                var focusable = modal.querySelectorAll('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
                if (!focusable.length) return;
                var first = focusable[0];
                var last = focusable[focusable.length - 1];
                if (e.shiftKey) {
                    if (document.activeElement === first) { e.preventDefault(); last.focus(); }
                } else {
                    if (document.activeElement === last) { e.preventDefault(); first.focus(); }
                }
            }
        });
    },

    /* Accesibilidad: navegación con flechas del teclado en la barra de tabs */
    _setupTabKeyboard: function() {
        var tabList = document.querySelector('[role="tablist"]');
        if (!tabList) return;
        tabList.addEventListener('keydown', function(e) {
            var tabs = Array.from(tabList.querySelectorAll('[role="tab"]:not([style*="display: none"])'));
            var idx = tabs.indexOf(document.activeElement);
            if (idx < 0) return;
            if (e.key === 'ArrowRight') {
                e.preventDefault();
                tabs[(idx + 1) % tabs.length].focus();
            } else if (e.key === 'ArrowLeft') {
                e.preventDefault();
                tabs[(idx - 1 + tabs.length) % tabs.length].focus();
            } else if (e.key === 'Home') {
                e.preventDefault();
                tabs[0].focus();
            } else if (e.key === 'End') {
                e.preventDefault();
                tabs[tabs.length - 1].focus();
            }
        });
    },
    
    loadUserData: function() {
        if (!this.currentUser) return;
        
        // Llenar formulario con datos del usuario
        document.getElementById('profile-first-name').value = this.currentUser.first_name || '';
        document.getElementById('profile-last-name').value = this.currentUser.last_name || '';
        document.getElementById('profile-second-last-name').value = this.currentUser.second_last_name || '';
        document.getElementById('profile-username').value = this.currentUser.username || '';
        document.getElementById('profile-email').value = this.currentUser.email || '';
        document.getElementById('profile-phone').value = this.currentUser.phone || '';
        
        // Mostrar rol (emoji aria-hidden para lectores de pantalla)
        var roleBadge = document.getElementById('user-role-badge');
        var roleText = this.currentUser.roles && this.currentUser.roles[0] || 'usuario';
        var roleEmoji = roleText === 'admin' ? '👑' : roleText === 'seller' ? '💼' : '👤';
        var roleLabel = roleText.charAt(0).toUpperCase() + roleText.slice(1);
        roleBadge.innerHTML = '<span aria-hidden="true">' + roleEmoji + '</span> ' + roleLabel;
        
        // Mostrar pestaña de gestión solo para admin
        if (this.currentUser.roles && this.currentUser.roles.includes('admin')) {
            var managementTab = document.getElementById('tab-management-btn');
            if (managementTab) {
                managementTab.style.display = 'inline-block';
            }
            var adminContractsTab = document.getElementById('tab-admin-contracts-btn');
            if (adminContractsTab) {
                adminContractsTab.style.display = 'inline-block';
            }
        }
        
        // Configurar pestañas según el rol del usuario
        var contractTab = document.getElementById('tab-contract-btn');
        var addSaleTab = document.getElementById('tab-add-sale-btn');
        
        if (this.currentUser.roles && this.currentUser.roles.includes('seller')) {
            // Para vendedores: ocultar Mi Contrato, mostrar Agregar Venta
            if (contractTab) contractTab.style.display = 'none';
            if (addSaleTab) addSaleTab.style.display = 'inline-block';
        } else if (this.currentUser.roles && this.currentUser.roles.includes('admin')) {
            // Para admin: ocultar Mi Contrato y Agregar Venta
            if (contractTab) contractTab.style.display = 'none';
            if (addSaleTab) addSaleTab.style.display = 'none';
        } else {
            // Para usuarios normales: mostrar Mi Contrato, ocultar Agregar Venta
            if (contractTab) contractTab.style.display = 'inline-block';
            if (addSaleTab) addSaleTab.style.display = 'none';
        }
    },
    
    loadContractData: function() {
        // Cargar contratos reales desde la API
        if (!this.currentUser) return;
        
        var self = this;
        
        // Obtener contratos del usuario
        window.apiService.get('/contracts/my')
            .then(function(response) {
                
                if (response.data && response.data.length > 0) {
                    // Tomar el primer contrato activo o el más reciente
                    var contract = response.data.find(c => c.status === 'active') || response.data[0];
                    self.displayContractData(contract);
                } else {
                    // No hay contratos, mostrar datos por defecto o de calculadora
                    self.displayNoContract();
                }
            })
            .catch(function(error) {
                // Si hay error, intentar cargar desde sessionStorage
                self.loadContractFromStorage();
            });
    },
    
    displayContractData: function(contract) {
        // Actualizar elementos con datos del contrato real
        var contractNumberEl = document.getElementById('contract-number');
        var previousCompanyEl = document.getElementById('previous-company');
        var providerNameEl = document.getElementById('provider-name');
        var monthlySavingsEl = document.getElementById('monthly-savings');
        var statusEl = document.querySelector('.status-active');
        
        // Información del contrato
        if (contractNumberEl) {
            contractNumberEl.textContent = 'CT-' + contract.id.toString().padStart(6, '0');
        }
        
        // Compañía anterior (por ahora desde sessionStorage o por defecto)
        var calculatorData = sessionStorage.getItem('calculatorData');
        var previousCompany = 'Anterior proveedor';
        if (calculatorData) {
            var data = JSON.parse(calculatorData);
            previousCompany = data.companyName || previousCompany;
        }
        if (previousCompanyEl) {
            previousCompanyEl.textContent = previousCompany;
        }
        
        // Nombre del proveedor
        var providerName = contract.provider_name || 'Proveedor';
        if (providerNameEl) {
            providerNameEl.textContent = providerName;
        }
        
        // Actualizar todas las referencias al proveedor
        var providerNameElements = [
            document.getElementById('provider-name-text'),
            document.getElementById('provider-name-text2'),
            document.getElementById('provider-name-text3'),
            document.getElementById('provider-name-text4')
        ];
        providerNameElements.forEach(function(el) {
            if (el) el.textContent = providerName;
        });
        
        // Ahorro estimado (por ahora usar dato de sessionStorage o por defecto)
        var savings = '30%';
        if (calculatorData) {
            var data = JSON.parse(calculatorData);
            savings = data.savings || savings;
        }
        if (monthlySavingsEl) {
            monthlySavingsEl.textContent = savings;
        }
        
        // Estado del contrato
        if (statusEl) {
            var statusMap = {
                'active': '✓ Activo',
                'pending': '⏳ Pendiente',
                'cancelled': '✗ Cancelado',
                'completed': '✓ Completado'
            };
            statusEl.textContent = statusMap[contract.status] || contract.status;
            statusEl.className = 'detail-value status-' + contract.status;
        }
        
        // Guardar contrato actual para referencia
        this.currentContract = contract;
    },
    
    displayNoContract: function() {
        // No hay contratos, intentar cargar desde sessionStorage
        this.loadContractFromStorage();
    },
    
    loadContractFromStorage: function() {
        // Cargar datos de cotización desde sessionStorage
        var calculatorData = sessionStorage.getItem('calculatorData');
        var contractNumber = 'No disponible';
        var previousCompany = 'No disponible';
        var providerName = 'Proveedor';
        
        if (calculatorData) {
            var data = JSON.parse(calculatorData);
            previousCompany = data.companyName || previousCompany;
            providerName = data.selectedProvider || providerName;
        }
        
        // Actualizar elementos
        var contractNumberEl = document.getElementById('contract-number');
        var previousCompanyEl = document.getElementById('previous-company');
        var providerNameEl = document.getElementById('provider-name');
        
        if (contractNumberEl) {
            contractNumberEl.textContent = contractNumber;
        }
        if (previousCompanyEl) {
            previousCompanyEl.textContent = previousCompany;
        }
        if (providerNameEl) {
            providerNameEl.textContent = providerName;
        }
        
        // Actualizar todas las referencias al proveedor
        var providerNameElements = [
            document.getElementById('provider-name-text'),
            document.getElementById('provider-name-text2'),
            document.getElementById('provider-name-text3'),
            document.getElementById('provider-name-text4')
        ];
        providerNameElements.forEach(function(el) {
            if (el) el.textContent = providerName;
        });
    },
    
    setupForms: function() {
        this.setupProfileForm();
        this.setupPasswordForm();
        this.setupAddSellerForm();
        this.setupSellerCalculator();
        this.setupSellerRegister();
    },
    
    setupProfileForm: function() {
        var form = document.getElementById('profile-form');
        var messageDiv = document.getElementById('profile-message');
        var saveBtn = document.getElementById('save-btn');
        
        if (!form) return;
        
        var self = this;
        
        form.onsubmit = function(e) {
            e.preventDefault();
            
            var formData = {
                first_name: document.getElementById('profile-first-name').value.trim(),
                last_name: document.getElementById('profile-last-name').value.trim(),
                second_last_name: document.getElementById('profile-second-last-name').value.trim() || null,
                username: document.getElementById('profile-username').value.trim(),
                email: document.getElementById('profile-email').value.trim(),
                phone: document.getElementById('profile-phone').value.trim() || null
            };
            
            messageDiv.style.display = 'none';
            saveBtn.classList.add('loading');
            saveBtn.disabled = true;
            
            // Actualizar perfil
            window.apiService.put('/auth/profile', formData)
                .then(function(response) {
                    
                    // Actualizar datos en authService y localStorage
                    if (response.data) {
                        window.authService.user = response.data;
                        localStorage.setItem('user', JSON.stringify(response.data));
                        self.currentUser = response.data;
                    }
                    
                    // Mostrar éxito
                    messageDiv.className = 'message success';
                    messageDiv.textContent = '✓ Perfil actualizado exitosamente';
                    messageDiv.style.display = 'block';
                    
                    // Actualizar header
                    if (window.headerComponent) {
                        window.headerComponent.updateUserMenu();
                    }
                    
                    saveBtn.classList.remove('loading');
                    saveBtn.disabled = false;
                    
                    // Ocultar mensaje después de 3 segundos
                    setTimeout(function() {
                        messageDiv.style.display = 'none';
                    }, 3000);
                })
                .catch(function(error) {
                    
                    messageDiv.className = 'message error';
                    messageDiv.textContent = error.message || 'Error al actualizar el perfil';
                    messageDiv.style.display = 'block';
                    
                    saveBtn.classList.remove('loading');
                    saveBtn.disabled = false;
                });
        };
    },
    
    setupPasswordForm: function() {
        var form = document.getElementById('password-form');
        var messageDiv = document.getElementById('password-message');
        var passwordBtn = document.getElementById('password-btn');
        
        if (!form) return;
        
        form.onsubmit = function(e) {
            e.preventDefault();
            
            var currentPassword = document.getElementById('current-password').value;
            var newPassword = document.getElementById('new-password').value;
            var confirmPassword = document.getElementById('confirm-password').value;
            
            // Validar que las contraseñas coincidan
            if (newPassword !== confirmPassword) {
                messageDiv.className = 'message error';
                messageDiv.textContent = 'Las contraseñas no coinciden';
                messageDiv.style.display = 'block';
                return;
            }
            
            if (newPassword.length < 6) {
                messageDiv.className = 'message error';
                messageDiv.textContent = 'La contraseña debe tener al menos 6 caracteres';
                messageDiv.style.display = 'block';
                return;
            }
            
            messageDiv.style.display = 'none';
            passwordBtn.classList.add('loading');
            passwordBtn.disabled = true;
            
            // Cambiar contraseña
            var userId = self.currentUser.id;
            window.apiService.put('/users/' + userId + '/password', {
                current_password: currentPassword,
                new_password: newPassword
            })
                .then(function(response) {
                    
                    messageDiv.className = 'message success';
                    messageDiv.textContent = '✓ Contraseña actualizada exitosamente';
                    messageDiv.style.display = 'block';
                    
                    // Limpiar formulario
                    form.reset();
                    
                    passwordBtn.classList.remove('loading');
                    passwordBtn.disabled = false;
                    
                    setTimeout(function() {
                        messageDiv.style.display = 'none';
                    }, 3000);
                })
                .catch(function(error) {
                    
                    messageDiv.className = 'message error';
                    messageDiv.textContent = error.message || 'Error al cambiar la contraseña';
                    messageDiv.style.display = 'block';
                    
                    passwordBtn.classList.remove('loading');
                    passwordBtn.disabled = false;
                });
        };
    },
    
    switchTab: function(tabName) {
        // Desactivar todos los tabs y sus paneles (ARIA tablist pattern)
        var tabBtns = document.querySelectorAll('.tab-btn[role="tab"]');
        var tabContents = document.querySelectorAll('.tab-content[role="tabpanel"]');
        
        tabBtns.forEach(function(btn) {
            btn.classList.remove('active');
            btn.setAttribute('aria-selected', 'false');
            btn.setAttribute('tabindex', '-1');
        });
        tabContents.forEach(function(content) {
            content.classList.remove('active');
        });
        
        // Activar tab seleccionado - buscar el botón por id convencional
        var activeBtn = document.getElementById('tab-' + tabName + '-btn');
        if (activeBtn) {
            activeBtn.classList.add('active');
            activeBtn.setAttribute('aria-selected', 'true');
            activeBtn.setAttribute('tabindex', '0');
        } else {
            // Fallback: buscar por onclick
            tabBtns.forEach(function(btn) {
                if (btn.onclick && btn.onclick.toString().includes("'" + tabName + "'")) {
                    btn.classList.add('active');
                    btn.setAttribute('aria-selected', 'true');
                    btn.setAttribute('tabindex', '0');
                }
            });
        }
        
        var tabContent = document.getElementById('tab-' + tabName);
        if (tabContent) {
            tabContent.classList.add('active');
        }
    },
    
    confirmDelete: function() {
        var modal = document.getElementById('delete-modal');
        modal.style.display = 'flex';
        // Mover el foco al modal (accesibilidad)
        var firstBtn = modal.querySelector('button');
        if (firstBtn) firstBtn.focus();
        // Guardar referencia del elemento que abrió el modal
        this._deleteOpener = document.activeElement;
    },
    
    closeDeleteModal: function() {
        document.getElementById('delete-modal').style.display = 'none';
        // Devolver el foco al botón que abrió el modal
        if (this._deleteOpener) {
            this._deleteOpener.focus();
        }
    },
    
    deleteAccount: function() {
        var userId = this.currentUser.id;
        
        window.apiService.delete('/users/' + userId)
            .then(function(response) {
                
                // Cerrar sesión
                window.authService.logout();
                
                // Redirigir a home
                window.app.loadPage('home');
                
                alert('Tu cuenta ha sido eliminada exitosamente');
            })
            .catch(function(error) {
                alert('Error al eliminar la cuenta: ' + (error.message || 'Error desconocido'));
            });
    },
    
    setupAddSellerForm: function() {
        var form = document.getElementById('add-seller-form');
        var messageDiv = document.getElementById('seller-message');
        var addSellerBtn = document.getElementById('add-seller-btn');
        
        if (!form) return;
        
        var self = this;
        
        form.onsubmit = function(e) {
            e.preventDefault();
            
            // Verificar que el usuario sea admin
            if (!self.currentUser.roles || !self.currentUser.roles.includes('admin')) {
                messageDiv.className = 'message error';
                messageDiv.textContent = 'No tienes permisos para realizar esta acción';
                messageDiv.style.display = 'block';
                return;
            }
            
            var password = document.getElementById('seller-password').value;
            var passwordConfirm = document.getElementById('seller-password-confirm').value;
            
            // Validar contraseña
            if (password.length < 6) {
                messageDiv.className = 'message error';
                messageDiv.textContent = 'La contraseña debe tener al menos 6 caracteres';
                messageDiv.style.display = 'block';
                return;
            }
            
            // Validar que las contraseñas coincidan
            if (password !== passwordConfirm) {
                messageDiv.className = 'message error';
                messageDiv.textContent = 'Las contraseñas no coinciden';
                messageDiv.style.display = 'block';
                return;
            }
            
            var formData = {
                first_name: document.getElementById('seller-first-name').value.trim(),
                last_name: document.getElementById('seller-last-name').value.trim(),
                second_last_name: document.getElementById('seller-second-last-name').value.trim() || null,
                email: document.getElementById('seller-email').value.trim(),
                username: document.getElementById('seller-username').value.trim(),
                password: password,
                phone: document.getElementById('seller-phone').value.trim() || null,
                role: 'seller',
                is_active: true // Vendedor activo inmediatamente
            };
            
            messageDiv.style.display = 'none';
            addSellerBtn.classList.add('loading');
            addSellerBtn.disabled = true;
            
            // Crear vendedor
            window.apiService.post('/admin/create-seller', formData)
                .then(function(response) {
                    
                    messageDiv.className = 'message success';
                    messageDiv.textContent = '✓ Vendedor creado exitosamente';
                    messageDiv.style.display = 'block';
                    
                    // Limpiar formulario
                    form.reset();
                    
                    addSellerBtn.classList.remove('loading');
                    addSellerBtn.disabled = false;
                    
                    setTimeout(function() {
                        messageDiv.style.display = 'none';
                    }, 3000);
                })
                .catch(function(error) {
                    
                    messageDiv.className = 'message error';
                    messageDiv.textContent = error.message || 'Error al crear vendedor';
                    messageDiv.style.display = 'block';
                    
                    addSellerBtn.classList.remove('loading');
                    addSellerBtn.disabled = false;
                });
        };
    },
    
    setupSellerCalculator: function() {
        var form = document.getElementById('seller-calculator-form');
        var errorDiv = document.getElementById('seller-calc-error');
        var calculateBtn = document.getElementById('seller-calculate-btn');
        
        if (!form) return;
        
        var self = this;
        
        form.onsubmit = function(e) {
            e.preventDefault();
            
            var holderName = document.getElementById('seller-calc-holder-name').value.trim();
            var contractNumber = document.getElementById('seller-calc-contract-number').value.trim();
            var companyName = document.getElementById('seller-calc-company-name').value.trim();
            var consumption = parseFloat(document.getElementById('seller-calc-consumption').value);
            var monthlyAmount = parseFloat(document.getElementById('seller-calc-monthly-amount').value);
            
            // Validaciones
            if (!holderName || !contractNumber || !companyName) {
                errorDiv.className = 'message error';
                errorDiv.textContent = 'Por favor, completa todos los campos obligatorios';
                errorDiv.style.display = 'block';
                return;
            }
            
            if (consumption <= 0 || monthlyAmount <= 0) {
                errorDiv.className = 'message error';
                errorDiv.textContent = 'El consumo y el importe deben ser mayores que cero';
                errorDiv.style.display = 'block';
                return;
            }
            
            // Ocultar error
            errorDiv.style.display = 'none';
            
            // Calcular ahorro (30%)
            var savings = monthlyAmount * 0.30;
            var newAmount = monthlyAmount - savings;
            
            // Guardar datos temporalmente en el objeto profilePage
            self.clientCalculatorData = {
                holderName: holderName,
                contractNumber: contractNumber,
                companyName: companyName,
                consumption: consumption,
                monthlyAmount: monthlyAmount,
                savings: savings,
                newAmount: newAmount
            };
            
            // Mostrar resultados
            document.getElementById('seller-result-current').textContent = '€' + monthlyAmount.toFixed(2);
            document.getElementById('seller-result-savings').textContent = '€' + savings.toFixed(2);
            document.getElementById('seller-result-new').textContent = '€' + newAmount.toFixed(2);
            
            // Ocultar calculadora, mostrar resultados y formulario de registro
            document.getElementById('seller-calculator-section').style.display = 'none';
            document.getElementById('seller-results-section').style.display = 'block';
        };
        
        // Botón para volver a la calculadora
        var backBtn = document.getElementById('seller-back-btn');
        if (backBtn) {
            backBtn.onclick = function() {
                document.getElementById('seller-calculator-section').style.display = 'block';
                document.getElementById('seller-results-section').style.display = 'none';
                // Limpiar mensajes
                var messageDiv = document.getElementById('seller-reg-message');
                if (messageDiv) {
                    messageDiv.style.display = 'none';
                }
            };
        }
    },
    
    loadAdminContracts: function() {
        // Evitar recargar si ya se cargó
        if (this._adminContractsLoaded) return;
        this._adminContractsLoaded = true;

        var self = this;
        var statusLabels = {
            'active':    { text: 'Activo',    cls: 'status-badge status-active' },
            'pending':   { text: 'Pendiente', cls: 'status-badge status-pending' },
            'cancelled': { text: 'Cancelado', cls: 'status-badge status-cancelled' },
            'completed': { text: 'Completado',cls: 'status-badge status-completed' }
        };

        function statusBadge(status) {
            var s = statusLabels[status] || { text: status || '-', cls: 'status-badge' };
            return '<span class="' + s.cls + '">' + s.text + '</span>';
        }

        function formatDate(dateStr) {
            if (!dateStr) return '-';
            var d = new Date(dateStr);
            return isNaN(d) ? dateStr : d.toLocaleDateString('es-ES');
        }

        window.apiService.get('/admin/contracts')
            .then(function(response) {
                var data = response.data || {};

                // --- Tabla vendedores ---
                var sellerContracts = data.seller_contracts || [];
                var sellerLoading = document.getElementById('seller-contracts-loading');
                var sellerEmpty   = document.getElementById('seller-contracts-empty');
                var sellerWrap    = document.getElementById('seller-contracts-table-wrap');
                var sellerTbody   = document.getElementById('seller-contracts-tbody');

                if (sellerLoading) sellerLoading.style.display = 'none';

                if (sellerContracts.length === 0) {
                    if (sellerEmpty) sellerEmpty.style.display = 'block';
                } else {
                    if (sellerWrap) sellerWrap.style.display = 'block';
                    sellerTbody.innerHTML = sellerContracts.map(function(c) {
                        var clientName = (c.client_first_name + ' ' + c.client_last_name).trim() || 'N/A';
                        var sellerName = (c.seller_first_name + ' ' + c.seller_last_name).trim()
                                         || c.seller_username || '-';
                        var commission = c.commission_amount ? '\u20ac' + parseFloat(c.commission_amount).toFixed(2) : '-';
                        return '<tr>' +
                            '<td>' + 'CT-' + String(c.id).padStart(6, '0') + '</td>' +
                            '<td>' + clientName + '</td>' +
                            '<td>' + (c.client_email || '-') + '</td>' +
                            '<td>' + sellerName + '</td>' +
                            '<td>' + (c.plan_name || '-') + '</td>' +
                            '<td>' + (c.provider_name || '-') + '</td>' +
                            '<td>' + statusBadge(c.status) + '</td>' +
                            '<td>' + commission + '</td>' +
                            '<td>' + formatDate(c.created_at) + '</td>' +
                        '</tr>';
                    }).join('');
                }

                // --- Tabla directos ---
                var directContracts = data.direct_contracts || [];
                var directLoading = document.getElementById('direct-contracts-loading');
                var directEmpty   = document.getElementById('direct-contracts-empty');
                var directWrap    = document.getElementById('direct-contracts-table-wrap');
                var directTbody   = document.getElementById('direct-contracts-tbody');

                if (directLoading) directLoading.style.display = 'none';

                if (directContracts.length === 0) {
                    if (directEmpty) directEmpty.style.display = 'block';
                } else {
                    if (directWrap) directWrap.style.display = 'block';
                    directTbody.innerHTML = directContracts.map(function(c) {
                        var clientName = (c.client_first_name + ' ' + c.client_last_name).trim() || 'N/A';
                        var amount = c.total_amount ? '\u20ac' + parseFloat(c.total_amount).toFixed(2) : '-';
                        return '<tr>' +
                            '<td>' + 'CT-' + String(c.id).padStart(6, '0') + '</td>' +
                            '<td>' + clientName + '</td>' +
                            '<td>' + (c.client_email || '-') + '</td>' +
                            '<td>' + (c.plan_name || '-') + '</td>' +
                            '<td>' + (c.provider_name || '-') + '</td>' +
                            '<td>' + statusBadge(c.status) + '</td>' +
                            '<td>' + amount + '</td>' +
                            '<td>' + formatDate(c.created_at) + '</td>' +
                        '</tr>';
                    }).join('');
                }
            })
            .catch(function(error) {
                ['seller-contracts-loading', 'direct-contracts-loading'].forEach(function(id) {
                    var el = document.getElementById(id);
                    if (el) {
                        el.innerHTML = '<span style="color:#ef4444">⚠️ Error al cargar los contratos. Intenta de nuevo.</span>';
                    }
                });
            });
    },

    setupSellerRegister: function() {
        var form = document.getElementById('seller-register-form');
        var messageDiv = document.getElementById('seller-reg-message');
        var registerBtn = document.getElementById('seller-register-btn');
        
        if (!form) return;
        
        var self = this;
        
        form.onsubmit = function(e) {
            e.preventDefault();
            
            // Verificar que el usuario sea vendedor
            if (!self.currentUser.roles || !self.currentUser.roles.includes('seller')) {
                messageDiv.className = 'message error';
                messageDiv.textContent = 'No tienes permisos para realizar esta acción';
                messageDiv.style.display = 'block';
                return;
            }
            
            // Verificar que hay datos de la calculadora
            if (!self.clientCalculatorData) {
                messageDiv.className = 'message error';
                messageDiv.textContent = 'Debes calcular el ahorro primero';
                messageDiv.style.display = 'block';
                return;
            }
            
            var password = document.getElementById('seller-reg-password').value;
            var passwordConfirm = document.getElementById('seller-reg-password-confirm').value;
            
            // Validar contraseña
            if (password.length < 6) {
                messageDiv.className = 'message error';
                messageDiv.textContent = 'La contraseña debe tener al menos 6 caracteres';
                messageDiv.style.display = 'block';
                return;
            }
            
            // Validar que las contraseñas coincidan
            if (password !== passwordConfirm) {
                messageDiv.className = 'message error';
                messageDiv.textContent = 'Las contraseñas no coinciden';
                messageDiv.style.display = 'block';
                return;
            }
            
            var formData = {
                first_name: document.getElementById('seller-reg-first-name').value.trim(),
                last_name: document.getElementById('seller-reg-last-name').value.trim(),
                second_last_name: document.getElementById('seller-reg-second-last-name').value.trim() || null,
                email: document.getElementById('seller-reg-email').value.trim(),
                username: document.getElementById('seller-reg-username').value.trim(),
                password: password,
                phone: document.getElementById('seller-reg-phone').value.trim() || null,
                contract_number: self.clientCalculatorData.contractNumber,
                current_company: self.clientCalculatorData.companyName,
                provider: 'Naturgy',
                role: 'user',
                seller_id: self.currentUser.id
            };
            
            messageDiv.style.display = 'none';
            registerBtn.classList.add('loading');
            registerBtn.disabled = true;
            
            // Registrar cliente y crear contrato
            window.authService.register(formData)
                .then(function(response) {
                    
                    messageDiv.className = 'message success';
                    messageDiv.textContent = '✓ Cliente registrado exitosamente con su contrato';
                    messageDiv.style.display = 'block';
                    
                    // Limpiar formularios
                    form.reset();
                    document.getElementById('seller-calculator-form').reset();
                    
                    // Limpiar datos temporales
                    self.clientCalculatorData = null;
                    
                    registerBtn.classList.remove('loading');
                    registerBtn.disabled = false;
                    
                    // Después de 2 segundos, volver a la calculadora
                    setTimeout(function() {
                        messageDiv.style.display = 'none';
                        document.getElementById('seller-calculator-section').style.display = 'block';
                        document.getElementById('seller-results-section').style.display = 'none';
                    }, 2000);
                })
                .catch(function(error) {
                    
                    messageDiv.className = 'message error';
                    messageDiv.textContent = error.message || 'Error al registrar cliente';
                    messageDiv.style.display = 'block';
                    
                    registerBtn.classList.remove('loading');
                    registerBtn.disabled = false;
                });
        };
    }
};

window.profilePage = profilePage;
