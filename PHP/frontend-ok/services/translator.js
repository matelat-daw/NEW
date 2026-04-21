// Google Translate Service
(function() {
    'use strict';
    
    /**
     * Limpiar cookies de traducción
     */
    function clearTranslationCookies() {
        document.cookie = 'googtrans=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
        document.cookie = 'googtrans=; path=/; domain=' + window.location.hostname + '; expires=Thu, 01 Jan 1970 00:00:00 GMT';
    }
    
    /**
     * Obtener el idioma actual
     * @returns {string} 'es' o 'en'
     */
    function getCurrentLanguage() {
        const hash = window.location.hash;
        const cookie = document.cookie;
        
        if (hash.includes('/en') || cookie.includes('googtrans=/es/en')) {
            return 'en';
        }
        return 'es';
    }
    
    /**
     * Inicializar widget de Google Translate
     */
    window.googleTranslateElementInit = function() {
        new google.translate.TranslateElement({
            pageLanguage: 'es',
            includedLanguages: 'es,en',
            layout: google.translate.TranslateElement.InlineLayout.SIMPLE,
            autoDisplay: false
        }, 'google_translate_element');
        
        // Actualizar UI después de inicializar
        setTimeout(updateLanguageButton, 500);
    };
    
    /**
     * Cambiar idioma
     */
    window.changeLanguage = function() {
        const currentLang = getCurrentLanguage();
        const targetLang = currentLang === 'es' ? 'en' : 'es';
        
        // Guardar la página actual antes de cambiar idioma
        const currentPage = window.location.hash || '#home';
        sessionStorage.setItem('lastPage', currentPage);
        
        if (targetLang === 'es') {
            // Volver al español
            clearTranslationCookies();
            // NO borrar el hash - mantener la página actual
            window.location.reload();
        } else {
            // Cambiar a inglés
            const select = document.querySelector('.goog-te-combo');
            if (select) {
                select.value = 'en';
                select.dispatchEvent(new Event('change'));
                setTimeout(updateLanguageButton, 500);
            } else {
                document.cookie = 'googtrans=/es/en; path=/';
                document.cookie = 'googtrans=/es/en; path=/; domain=' + window.location.hostname;
                window.location.reload();
            }
        }
    };
    
    // Flag SVGs for cross-browser compatibility
    const flags = {
        en: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 60 30" width="24" height="12"><path d="M0,0 v30 h60 v-30 z" fill="#012169"/><path d="M0,0 L60,30 M60,0 L0,30" stroke="#fff" stroke-width="6"/><path d="M0,0 L60,30 M60,0 L0,30" stroke="#C8102E" stroke-width="4"/><path d="M30,0 v30 M0,15 h60" stroke="#fff" stroke-width="10"/><path d="M30,0 v30 M0,15 h60" stroke="#C8102E" stroke-width="6"/></svg>',
        es: '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 750 500" width="24" height="16"><rect width="750" height="500" fill="#c60b1e"/><rect width="750" height="250" y="125" fill="#ffc400"/></svg>'
    };

    /**
     * Actualizar el botón de idioma
     */
    function updateLanguageButton() {
        const currentLang = getCurrentLanguage();
        const buttons = document.querySelectorAll('.language-btn');
        
        if (buttons.length === 0) return;
        
        buttons.forEach(button => {
            const flagElement = button.querySelector('.flag-icon') || button.querySelector('.flag');
            const textElement = button.querySelector('.lang-text');
            
            if (currentLang === 'es') {
                // App en español, mostrar opción para cambiar a inglés (Bandera UK)
                if (flagElement) flagElement.innerHTML = flags.en;
                if (textElement) textElement.textContent = 'English';
                button.setAttribute('title', 'Switch to English');
                button.setAttribute('data-target-lang', 'en');
                
                // Si el botón no tiene estructura interna, actualizar todo el contenido
                if (!flagElement && !textElement) {
                    button.innerHTML = '<span class="flag-icon">' + flags.en + '</span>';
                }
            } else {
                // App en inglés, mostrar opción para cambiar a español (Bandera España)
                if (flagElement) flagElement.innerHTML = flags.es;
                if (textElement) textElement.textContent = 'Español';
                button.setAttribute('title', 'Cambiar a Español');
                button.setAttribute('data-target-lang', 'es');
                
                // Si el botón no tiene estructura interna, actualizar todo el contenido
                if (!flagElement && !textElement) {
                    button.innerHTML = '<span class="flag-icon">' + flags.es + '</span>';
                }
            }
        });
    }
    
    /**
     * Ocultar elementos de Google Translate de forma eficiente
     */
    function hideGoogleTranslateElements() {
        const selectors = [
            '.goog-te-banner-frame',
            '.skiptranslate iframe',
            'body > .skiptranslate',
            '#goog-gt-tt',
            '.goog-te-balloon-frame',
            '.goog-te-gadget',
            '.goog-te-gadget-simple',
            '.goog-te-menu-value',
            '.goog-te-gadget-icon',
            '.goog-logo-link',
            '.goog-te-menu-frame',
            '[id^="goog-"]',
            '[class*="goog-te"]',
            '[class*="goog-"]',
            '#google_translate_element *',
            'img[src*="translate.gstatic.com"]',
            'img[src*="translate.google.com"]'
        ];
        
        // Usar un solo selector gigante para reducir consultas al DOM
        const fullSelector = selectors.join(', ');
        const elements = document.querySelectorAll(fullSelector);
        
        elements.forEach(el => {
            if (el && el.style && el.style.display !== 'none') {
                el.style.display = 'none';
                el.style.visibility = 'hidden';
                el.style.opacity = '0';
                el.style.position = 'absolute';
                el.style.top = '-9999px';
                el.style.left = '-9999px';
                el.style.width = '0';
                el.style.height = '0';
                el.style.pointerEvents = 'none';
            }
        });
        
        // Limpiar elementos directos del body que no sean estructurales
        const structuralIds = ['app', 'google_translate_element'];
        const structuralTags = ['script', 'noscript', 'link', 'style'];
        
        Array.from(document.body.children).forEach(child => {
            const tagName = child.tagName.toLowerCase();
            const id = child.id;
            
            if (!structuralIds.includes(id) && !structuralTags.includes(tagName) && child.style.display !== 'none') {
                child.style.display = 'none';
                child.style.visibility = 'hidden';
                child.style.position = 'absolute';
                child.style.top = '-9999px';
            }
        });
    }
    
    /**
     * Inicialización
     */
    function init() {
        // Restaurar la página anterior si existe
        const lastPage = sessionStorage.getItem('lastPage');
        if (lastPage && lastPage !== window.location.hash) {
            window.location.hash = lastPage;
            sessionStorage.removeItem('lastPage');
        }
        
        // Actualizar botón de idioma
        updateLanguageButton();
        
        // Ocultar elementos iniciales
        hideGoogleTranslateElements();
        
        // Configurar observador de mutaciones para ocultar elementos dinámicos
        const observer = new MutationObserver(hideGoogleTranslateElements);
        observer.observe(document.body, { childList: true, subtree: true });
    }
    
    // Inicializar cuando el DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
    
    // Ejecutar ocultación periódica menos frecuente (seguridad)
    setInterval(hideGoogleTranslateElements, 500);
    setInterval(updateLanguageButton, 2000);
    
    // Ejecutar inmediatamente
    hideGoogleTranslateElements();
})();
