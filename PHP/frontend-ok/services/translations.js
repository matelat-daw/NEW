// Sistema de Traducción Local - Español e Inglés
(function() {
    'use strict';

    // Diccionario de traducciones
    const translations = {
        es: {
            // Header
            'home': 'Inicio',
            'about': 'Acerca de',
            'calculator': 'Calculadora',
            'results': 'Resultados',
            'contact': 'Contacto',
            'profile': 'Perfil',
            'logout': 'Salir',
            'login': 'Iniciar Sesión',
            'register': 'Registrarse',
            'theme_dark': 'Cambiar a tema oscuro',
            'theme_light': 'Cambiar a tema claro',
            'language_english': 'Switch to English',
            'language_spanish': 'Cambiar a Español',
            
            // Botones comunes
            'save': 'Guardar',
            'cancel': 'Cancelar',
            'delete': 'Eliminar',
            'edit': 'Editar',
            'back': 'Atrás',
            'next': 'Siguiente',
            'submit': 'Enviar',
            'loading': 'Cargando...',
            'error': 'Error',
            'success': 'Éxito',
            'warning': 'Advertencia',
            'info': 'Información',
            
            // Mensajes de validación
            'required_field': 'Este campo es requerido',
            'invalid_email': 'Correo electrónico inválido',
            'password_mismatch': 'Las contraseñas no coinciden',
            'password_too_short': 'La contraseña debe tener al menos 8 caracteres',
            
            // Autenticación
            'login_title': 'Iniciar Sesión',
            'register_title': 'Registrarse',
            'email': 'Correo Electrónico',
            'password': 'Contraseña',
            'confirm_password': 'Confirmar Contraseña',
            'remember_me': 'Recuérdame',
            'forgot_password': '¿Olvidaste tu contraseña?',
            'no_account': '¿No tienes cuenta?',
            'have_account': '¿Ya tienes cuenta?',
            'login_success': 'Sesión iniciada correctamente',
            'logout_success': 'Sesión cerrada correctamente',
            'login_error': 'Error al iniciar sesión',
            
            // Página de Inicio
            'welcome': 'Bienvenido a Energy App',
            'compare_providers': 'Compara Proveedores de Energía',
            'save_up_to': 'Ahorra hasta un 30% en tu factura de luz',
            'start_now': 'Comenzar Ahora',
            'how_it_works': 'Cómo Funciona',
            'our_services': 'Nuestros Servicios',
            'best_deals': 'Las Mejores Ofertas',
            'renewable_energy': 'Energía 100% Renovable',
            'no_commitment': 'Sin Permanencia',
            'quick_process': 'Proceso Rápido y Fácil',
            
            // Calculadora
            'calculator_title': 'Calculadora de Ahorro',
            'monthly_consumption': 'Consumo Mensual (kWh)',
            'current_provider': 'Proveedor Actual',
            'current_price': 'Precio Actual (€/kWh)',
            'calculate': 'Calcular Ahorro',
            'estimated_savings': 'Ahorro Estimado',
            'per_month': 'por mes',
            'per_year': 'por año',
            
            // Perfil
            'my_profile': 'Mi Perfil',
            'personal_info': 'Información Personal',
            'account_settings': 'Configuración de Cuenta',
            'change_password': 'Cambiar Contraseña',
            'delete_account': 'Eliminar Cuenta',
            'privacy_policy': 'Política de Privacidad',
            'terms_of_service': 'Términos de Servicio',
            'current_password': 'Contraseña Actual',
            'new_password': 'Nueva Contraseña',
            'confirm_new_password': 'Confirmar Nueva Contraseña',
            
            // Footer
            'about_us': 'Acerca de Nosotros',
            'contact_us': 'Contacta con Nosotros',
            'privacy': 'Privacidad',
            'terms': 'Términos',
            'cookies': 'Cookies',
            'follow_us': 'Síguenos',
            'all_rights_reserved': 'Todos los derechos reservados',
            'company': 'Energy App',
            
            // Errores comunes
            'error_404': 'Página no encontrada',
            'error_500': 'Error del servidor',
            'connection_error': 'Error de conexión',
            'try_again': 'Intentar de nuevo',
            'back_home': 'Volver al Inicio'
        },
        en: {
            // Header
            'home': 'Home',
            'about': 'About',
            'calculator': 'Calculator',
            'results': 'Results',
            'contact': 'Contact',
            'profile': 'Profile',
            'logout': 'Logout',
            'login': 'Sign In',
            'register': 'Sign Up',
            'theme_dark': 'Switch to dark mode',
            'theme_light': 'Switch to light mode',
            'language_english': 'English',
            'language_spanish': 'Cambiar a Español',
            
            // Common buttons
            'save': 'Save',
            'cancel': 'Cancel',
            'delete': 'Delete',
            'edit': 'Edit',
            'back': 'Back',
            'next': 'Next',
            'submit': 'Submit',
            'loading': 'Loading...',
            'error': 'Error',
            'success': 'Success',
            'warning': 'Warning',
            'info': 'Information',
            
            // Validation messages
            'required_field': 'This field is required',
            'invalid_email': 'Invalid email address',
            'password_mismatch': 'Passwords do not match',
            'password_too_short': 'Password must be at least 8 characters',
            
            // Authentication
            'login_title': 'Sign In',
            'register_title': 'Sign Up',
            'email': 'Email',
            'password': 'Password',
            'confirm_password': 'Confirm Password',
            'remember_me': 'Remember me',
            'forgot_password': 'Forgot password?',
            'no_account': 'Don\'t have an account?',
            'have_account': 'Already have an account?',
            'login_success': 'Signed in successfully',
            'logout_success': 'Signed out successfully',
            'login_error': 'Sign in failed',
            
            // Home page
            'welcome': 'Welcome to Energy App',
            'compare_providers': 'Compare Energy Providers',
            'save_up_to': 'Save up to 30% on your electricity bill',
            'start_now': 'Start Now',
            'how_it_works': 'How it Works',
            'our_services': 'Our Services',
            'best_deals': 'Best Deals',
            'renewable_energy': '100% Renewable Energy',
            'no_commitment': 'No Long-term Commitment',
            'quick_process': 'Quick and Easy Process',
            
            // Calculator
            'calculator_title': 'Savings Calculator',
            'monthly_consumption': 'Monthly Consumption (kWh)',
            'current_provider': 'Current Provider',
            'current_price': 'Current Price (€/kWh)',
            'calculate': 'Calculate Savings',
            'estimated_savings': 'Estimated Savings',
            'per_month': 'per month',
            'per_year': 'per year',
            
            // Profile
            'my_profile': 'My Profile',
            'personal_info': 'Personal Information',
            'account_settings': 'Account Settings',
            'change_password': 'Change Password',
            'delete_account': 'Delete Account',
            'privacy_policy': 'Privacy Policy',
            'terms_of_service': 'Terms of Service',
            'current_password': 'Current Password',
            'new_password': 'New Password',
            'confirm_new_password': 'Confirm New Password',
            
            // Footer
            'about_us': 'About Us',
            'contact_us': 'Contact Us',
            'privacy': 'Privacy',
            'terms': 'Terms',
            'cookies': 'Cookies',
            'follow_us': 'Follow Us',
            'all_rights_reserved': 'All rights reserved',
            'company': 'Energy App',
            
            // Common errors
            'error_404': 'Page not found',
            'error_500': 'Server error',
            'connection_error': 'Connection error',
            'try_again': 'Try again',
            'back_home': 'Back to Home'
        }
    };

    /**
     * Servicio de traducción
     */
    window.TranslationService = {
        /**
         * Obtener idioma actual del localStorage o por defecto español
         */
        getCurrentLanguage: function() {
            const saved = localStorage.getItem('appLanguage');
            if (saved && (saved === 'es' || saved === 'en')) {
                return saved;
            }
            
            // Detectar idioma del navegador
            const browserLang = navigator.language || navigator.userLanguage;
            if (browserLang.startsWith('en')) {
                return 'en';
            }
            return 'es';
        },

        /**
         * Establecer idioma actual
         */
        setLanguage: function(lang) {
            if (lang === 'es' || lang === 'en') {
                localStorage.setItem('appLanguage', lang);
                return true;
            }
            return false;
        },

        /**
         * Traducir una clave
         */
        translate: function(key, lang) {
            lang = lang || this.getCurrentLanguage();
            
            if (translations[lang] && translations[lang][key]) {
                return translations[lang][key];
            }
            
            // Fallback a español si no existe la traducción
            if (translations.es[key]) {
                return translations.es[key];
            }
            
            // Si ni existe en español, retornar la clave
            return key;
        },

        /**
         * Traducir múltiples claves
         */
        translateBatch: function(keys, lang) {
            const result = {};
            keys.forEach(key => {
                result[key] = this.translate(key, lang);
            });
            return result;
        },

        /**
         * Obtener todas las traducciones del idioma actual
         */
        getAllTranslations: function(lang) {
            lang = lang || this.getCurrentLanguage();
            return translations[lang] || translations.es;
        },

        /**
         * Traducir elementos del DOM con data-i18n
         */
        translateDOM: function(container) {
            container = container || document;
            
            const elements = container.querySelectorAll('[data-i18n]');
            const lang = this.getCurrentLanguage();
            
            elements.forEach(el => {
                const key = el.getAttribute('data-i18n');
                const translation = this.translate(key, lang);
                
                // Si tiene data-i18n-attr, traducir atributo
                const attrName = el.getAttribute('data-i18n-attr');
                if (attrName) {
                    el.setAttribute(attrName, translation);
                } else {
                    // Si es un input, traducir placeholder
                    if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') {
                        el.placeholder = translation;
                    } else {
                        // Si no es input, traducir el contenido de texto
                        el.textContent = translation;
                    }
                }
            });
        },

        /**
         * Crear una función t() para uso en templates
         */
        getTranslateFunction: function() {
            const self = this;
            return function(key) {
                return self.translate(key);
            };
        }
    };

    /**
     * Función global t() para traducir
     */
    window.t = function(key) {
        return window.TranslationService.translate(key);
    };

})();
