/**
 * customer.service.js - Servicio para gestionar customers de myikea
 */

class CustomerService {
    /**
     * Obtiene todos los customers con paginación
     * @param {number} page - Número de página (default 0)
     * @param {number} size - Tamaño de página (default 10)
     * @returns {Promise<Object>}
     */
    static async getCustomers(page = 0, size = 10) {
        try {
            const url = `${API_CONFIG.BASE_URL}${API_CONFIG.ENDPOINTS.CUSTOMERS}?page=${page}&size=${size}`;
            const response = await Utils.makeRequest('GET', url);
            return response;
        } catch (error) {
            throw error;
        }
    }

    /**
     * Obtiene un customer por ID
     * @param {number} id - ID del customer
     * @returns {Promise<Object>}
     */
    static async getCustomerById(id) {
        try {
            const url = API_CONFIG.BASE_URL + API_CONFIG.ENDPOINTS.CUSTOMER_BY_ID.replace(':id', id);
            const response = await Utils.makeRequest('GET', url);
            return response;
        } catch (error) {
            throw error;
        }
    }

    /**
     * Busca customers por nombre
     * @param {string} firstName - Nombre a buscar
     * @returns {Promise<Object>}
     */
    static async searchByFirstName(firstName) {
        try {
            const url = `${API_CONFIG.BASE_URL}${API_CONFIG.ENDPOINTS.CUSTOMER_SEARCH_FIRSTNAME.replace(':firstName', encodeURIComponent(firstName))}`;
            const response = await Utils.makeRequest('GET', url);
            return response;
        } catch (error) {
            throw error;
        }
    }

    /**
     * Busca customers por apellido
     * @param {string} lastName - Apellido a buscar
     * @returns {Promise<Object>}
     */
    static async searchByLastName(lastName) {
        try {
            const url = `${API_CONFIG.BASE_URL}${API_CONFIG.ENDPOINTS.CUSTOMER_SEARCH_LASTNAME.replace(':lastName', encodeURIComponent(lastName))}`;
            const response = await Utils.makeRequest('GET', url);
            return response;
        } catch (error) {
            throw error;
        }
    }

    /**
     * Elimina un customer
     * @param {number} id - ID del customer
     * @returns {Promise<Object>}
     */
    static async deleteCustomer(id) {
        try {
            const url = API_CONFIG.BASE_URL + API_CONFIG.ENDPOINTS.DELETE_CUSTOMER.replace(':id', id);
            const response = await Utils.makeRequest('DELETE', url);
            return response;
        } catch (error) {
            throw error;
        }
    }

    /**
     * Actualiza un customer
     * @param {number} id - ID del customer
     * @param {Object} customer - Objeto customer con datos actualizados
     * @returns {Promise<Object>}
     */
    static async updateCustomer(id, customer) {
        try {
            const url = API_CONFIG.BASE_URL + API_CONFIG.ENDPOINTS.UPDATE_CUSTOMER.replace(':id', id);
            const response = await Utils.makeRequest('PUT', url, customer);
            return response;
        } catch (error) {
            throw error;
        }
    }
}

// Exponer globalmente
window.CustomerService = CustomerService;

// Registrar que este script se ha cargado
if (typeof AppScripts !== 'undefined') AppScripts.register('customer.service');
