// Login Page
var loginPage = {
    init: function() {
        this.setupForm();
        this.setupPasswordToggle();
        
        // Si ya está logueado, redirigir a perfil
        if (window.authService && window.authService.isLoggedIn()) {
            window.app.loadPage('profile');
        }
    },
    
    setupPasswordToggle: function() {
        var toggleBtn = document.getElementById('toggle-password');
        var passwordInput = document.getElementById('password');
        
        if (!toggleBtn || !passwordInput) {
            return;
        }
        
        toggleBtn.onclick = function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            var type = passwordInput.type === 'password' ? 'text' : 'password';
            passwordInput.type = type;
            
            // Cambiar icono
            var eyeIcon = toggleBtn.querySelector('.eye-icon');
            if (type === 'text') {
                // Ojo tachado (contraseña visible)
                eyeIcon.innerHTML = '<path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path><line x1="1" y1="1" x2="23" y2="23"></line>';
            } else {
                // Ojo normal (contraseña oculta)
                eyeIcon.innerHTML = '<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle>';
            }
        };
    },
    
    setupForm: function() {
        var form = document.getElementById('login-form');
        var errorDiv = document.getElementById('login-error');
        var loginBtn = document.getElementById('login-btn');
        
        if (!form) return;
        
        form.onsubmit = function(e) {
            e.preventDefault();
            
            var email = document.getElementById('email').value;
            var password = document.getElementById('password').value;
            
            // Ocultar error previo
            errorDiv.style.display = 'none';
            
            // Loading state
            loginBtn.classList.add('loading');
            loginBtn.disabled = true;
            
            // Llamar al servicio de autenticación
            window.authService.login(email, password)
                .then(function(response) {
                    
                    // Mostrar mensaje de éxito
                    errorDiv.className = 'success-message';
                    errorDiv.textContent = '✓ ¡Bienvenido! Redirigiendo a tu perfil...';
                    errorDiv.style.display = 'block';
                    
                    // Redirigir al perfil después de 1 segundo
                    setTimeout(function() {
                        window.app.loadPage('profile');
                    }, 1000);
                })
                .catch(function(error) {
                    // Mostrar error
                    errorDiv.className = 'error-message';
                    errorDiv.textContent = error.message || 'Error al iniciar sesión. Verifica tus credenciales.';
                    errorDiv.style.display = 'block';
                    
                    // Remover loading state
                    loginBtn.classList.remove('loading');
                    loginBtn.disabled = false;
                });
        };
    }
};

window.loginPage = loginPage;
