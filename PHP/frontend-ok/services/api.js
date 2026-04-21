// API Service
var apiService = {
    baseURL: '/api',
    
    request: function(endpoint, options) {
        options = options || {};
        var url = this.baseURL + endpoint;
        
        var config = {
            method: options.method || 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include' // Importante: envía cookies automáticamente
        };
        
        // Agregar body si existe
        if (options.body) {
            config.body = JSON.stringify(options.body);
        }
        
        return fetch(url, config)
            .then(function(response) {
                return response.json().then(function(data) {
                    if (!response.ok) {
                        throw data;
                    }
                    return data;
                });
            });
    },
    
    get: function(endpoint) {
        return this.request(endpoint, { method: 'GET' });
    },
    
    post: function(endpoint, data) {
        return this.request(endpoint, { method: 'POST', body: data });
    },
    
    put: function(endpoint, data) {
        return this.request(endpoint, { method: 'PUT', body: data });
    },
    
    delete: function(endpoint) {
        return this.request(endpoint, { method: 'DELETE' });
    }
};

window.apiService = apiService;
