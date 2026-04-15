/**
 * constants.js - Constantes de la aplicación
 */

const APP_BASE_PATH = window.APP_BASE_PATH || (() => {
    const path = window.location.pathname;
    if (path.endsWith('/')) {
        return path;
    }

    const knownRoutes = ['login', 'register', 'dashboard', 'users', 'profile'];
    const parts = path.split('/').filter(Boolean);
    
    // Si estamos en una subruta conocida, el base path es el anterior
    if (parts.length > 0 && knownRoutes.includes(parts[parts.length - 1])) {
        return '/' + parts.slice(0, -1).join('/') + (parts.length > 1 ? '/' : '');
    }

    // Fallback: intentar detectar el directorio actual
    const lastSlash = path.lastIndexOf('/');
    return lastSlash >= 0 ? path.substring(0, lastSlash + 1) : '/';
})();

const API_CONFIG = {
    // API a través del backend local
    BASE_URL: 'http://localhost:7200/api',
    TIMEOUT: 5000,
    ENDPOINTS: {
        REGISTER: '/user/register',
        LOGIN: '/auth/login',
        PROFILE: '/profile',
        USERS: '/user',
        USER_BY_ID: '/user/:id',
        UPDATE_USER: '/user/:id',
        DELETE_USER: '/user/:id',
        // Endpoints de Customers (MyIkea)
        CUSTOMERS: '/myikea/customer',
        CUSTOMER_BY_ID: '/myikea/customer/:id',
        CUSTOMER_SEARCH_FIRSTNAME: '/myikea/customer/search/firstName/:firstName',
        CUSTOMER_SEARCH_LASTNAME: '/myikea/customer/search/lastName/:lastName',
        DELETE_CUSTOMER: '/myikea/customer/:id',
        UPDATE_CUSTOMER: '/myikea/customer/:id'
    }
};

const ROUTES = {
    HOME: APP_BASE_PATH,
    REGISTER: `${APP_BASE_PATH}register`,
    LOGIN: `${APP_BASE_PATH}login`,
    USERS: `${APP_BASE_PATH}users`,
    DASHBOARD: `${APP_BASE_PATH}dashboard`,
    CUSTOMERS: `${APP_BASE_PATH}customers`
};

const MESSAGES = {
    SUCCESS: {
        REGISTER: '¡User registration successful! Your account has been created.',
        LOGIN: '¡Welcome! You have logged in successfully.',
        UPDATE: 'The user has been updated correctly.',
        DELETE: 'The user has been deleted correctly.'
    },
    ERROR: {
        REGISTER_FAILED: 'Error registering the account. Please try again.',
        LOGIN_FAILED: 'Invalid credentials. Please verify your username and password.',
        SERVER_ERROR: 'Server error. Please try later.',
        VALIDATION_ERROR: 'Please complete all fields correctly.',
        EMAIL_EXISTS: 'The email address is already registered.',
        NICK_EXISTS: 'The username (nick) is already taken.',
        CONNECTION_ERROR: 'Connection error. Please verify your internet connection.'
    }
};

const VALIDATION_PATTERNS = {
    EMAIL: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
    PHONE: /^[+]?[0-9]{7,15}$/,
    PASSWORD: /^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[@#$%^&+=!_.\-*])/, // Debe tener mayúscula, minúscula, número y carácter especial
    NAME: /^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]{2,255}$/,
    NICK: /^[a-zA-Z0-9_]{3,255}$/ // Alfanumérico y guión bajo, mínimo 3
};

// Exponer globalmente para la verificación de carga
window.API_CONFIG = API_CONFIG;
window.APP_BASE_PATH = APP_BASE_PATH;
window.ROUTES = ROUTES;
window.MESSAGES = MESSAGES;
window.VALIDATION_PATTERNS = VALIDATION_PATTERNS;

// Registrar que este script se ha cargado
if (typeof AppScripts !== 'undefined') AppScripts.register('constants');

