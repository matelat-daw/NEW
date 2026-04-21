// Contact Page
var contactPage = {
    init: function() {
        this.scrollToTop();
        this.setupForm();
    },
    
    scrollToTop: function() {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    },
    
    setupForm: function() {
        var form = document.getElementById('contact-form');
        var responseDiv = document.getElementById('contact-response');
        var contactBtn = document.getElementById('contact-btn');
        
        if (!form) return;
        
        form.onsubmit = function(e) {
            e.preventDefault();
            
            var formData = {
                name: document.getElementById('contact-name').value.trim(),
                email: document.getElementById('contact-email').value.trim(),
                phone: document.getElementById('contact-phone').value.trim() || null,
                subject: document.getElementById('contact-subject').value,
                message: document.getElementById('contact-message').value.trim()
            };
            
            // Validación básica
            if (!formData.name || !formData.email || !formData.subject || !formData.message) {
                responseDiv.className = 'response-message error-message';
                responseDiv.textContent = 'Por favor completa todos los campos obligatorios';
                responseDiv.style.display = 'block';
                return;
            }
            
            // Loading state
            contactBtn.classList.add('loading');
            contactBtn.disabled = true;
            responseDiv.style.display = 'none';
            
            // Simular envío (aquí deberías hacer la llamada al API)
            setTimeout(function() {
                
                // Mostrar mensaje de éxito
                responseDiv.className = 'response-message success-message';
                responseDiv.textContent = '✓ ¡Mensaje enviado exitosamente! Te responderemos pronto.';
                responseDiv.style.display = 'block';
                
                // Limpiar formulario
                form.reset();
                
                // Remover loading state
                contactBtn.classList.remove('loading');
                contactBtn.disabled = false;
                
                // Scroll al mensaje de éxito
                responseDiv.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }, 1000);
            
            // TODO: Implementar llamada real al API cuando esté disponible
            // window.apiService.post('/contact', formData)
            //     .then(function(response) {
            //         responseDiv.className = 'response-message success-message';
            //         responseDiv.textContent = '✓ ¡Mensaje enviado exitosamente!';
            //         responseDiv.style.display = 'block';
            //         form.reset();
            //     })
            //     .catch(function(error) {
            //         responseDiv.className = 'response-message error-message';
            //         responseDiv.textContent = error.message || 'Error al enviar el mensaje';
            //         responseDiv.style.display = 'block';
            //     })
            //     .finally(function() {
            //         contactBtn.classList.remove('loading');
            //         contactBtn.disabled = false;
            //     });
        };
    }
};

window.contactPage = contactPage;
