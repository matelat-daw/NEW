// Results Page
var resultsPage = {
    init: function() {
        this.loadData();
        this.setupButtons();
    },
    
    loadData: function() {
        // Obtener datos del sessionStorage
        var dataJson = sessionStorage.getItem('calculatorData');
        
        if (!dataJson) {
            // Si no hay datos, redirigir a la calculadora
            window.app.navigate('calculator');
            return;
        }
        
        var data = JSON.parse(dataJson);
        
        // Mostrar los datos en la página
        document.getElementById('current-amount').textContent = '€' + data.monthlyAmount.toFixed(2);
        document.getElementById('savings-amount').textContent = '€' + data.savings.toFixed(2);
        document.getElementById('new-amount').textContent = '€' + data.newAmount.toFixed(2);
        
        document.getElementById('holder-name').textContent = data.holderName;
        document.getElementById('company-name').textContent = data.companyName;
        document.getElementById('consumption').textContent = data.consumption + ' kWh';
        document.getElementById('contract-number').textContent = data.contractNumber;
    },
    
    setupButtons: function() {
        var contractBtn = document.getElementById('contract-btn');
        var recalculateBtn = document.getElementById('recalculate-btn');
        
        if (contractBtn) {
            contractBtn.onclick = function() {
                // Redirigir al formulario de registro
                window.app.navigate('register');
            };
        }
        
        if (recalculateBtn) {
            recalculateBtn.onclick = function() {
                // Volver a la calculadora
                window.app.navigate('calculator');
            };
        }
    }
};

window.resultsPage = resultsPage;
