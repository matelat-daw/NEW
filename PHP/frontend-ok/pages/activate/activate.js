// Activate Page
var activatePage = {
    init: function() {
        // Obtener token de la URL
        var urlParams = new URLSearchParams(window.location.hash.split('?')[1] || '');
        var token = urlParams.get('token');
        
        if (!token) {
            this.showError('No se proporcionó un token de activación válido.');
            return;
        }
        
        // Activar cuenta
        this.activateAccount(token);
    },
    
    activateAccount: function(token) {
        window.apiService.post('/activate', { token: token })
            .then(function(response) {
                activatePage.showSuccess(response.message || 'Cuenta activada exitosamente');
            })
            .catch(function(error) {
                activatePage.showError(error.message || 'Error al activar la cuenta');
            });
    },
    
    showSuccess: function(message) {
        var icon = document.getElementById('activate-icon');
        var title = document.getElementById('activate-title');
        var messageEl = document.getElementById('activate-message');
        var content = document.getElementById('activate-content');
        var footer = document.getElementById('activate-footer');
        
        icon.textContent = '✅';
        icon.className = 'activate-icon success';
        
        title.textContent = '¡Cuenta Activada!';
        title.style.color = 'var(--success-color)';
        
        messageEl.textContent = message;
        
        content.innerHTML = '<div class="success-message">Tu cuenta ha sido verificada correctamente. Ya puedes iniciar sesión.</div>';
        
        footer.style.display = 'block';
    },
    
    showError: function(message) {
        var icon = document.getElementById('activate-icon');
        var title = document.getElementById('activate-title');
        var messageEl = document.getElementById('activate-message');
        var content = document.getElementById('activate-content');
        var footer = document.getElementById('activate-footer');
        
        icon.textContent = '❌';
        icon.className = 'activate-icon error';
        
        title.textContent = 'Error de Activación';
        title.style.color = 'var(--danger-color)';
        
        messageEl.textContent = message;
        
        content.innerHTML = '<div class="error-message">' + message + '</div>';
        
        footer.style.display = 'block';
    }
};

window.activatePage = activatePage;
