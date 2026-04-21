// Calculator Page
var calculatorPage = {
    init: function() {
        this.setupForm();
    },
    
    setupForm: function() {
        var form = document.getElementById('calculator-form');
        var errorDiv = document.getElementById('calculator-error');
        var calculateBtn = document.getElementById('calculate-btn');
        
        if (!form) return;
        
        form.onsubmit = function(e) {
            e.preventDefault();
            
            var holderName = document.getElementById('calc-holder-name').value.trim();
            var contractNumber = document.getElementById('calc-contract-number').value.trim();
            var companyName = document.getElementById('calc-company-name').value.trim();
            var consumption = parseFloat(document.getElementById('calc-consumption').value);
            var monthlyAmount = parseFloat(document.getElementById('calc-monthly-amount').value);
            
            // Validaciones
            if (!holderName || !contractNumber || !companyName) {
                errorDiv.textContent = 'Por favor, completa todos los campos obligatorios';
                errorDiv.style.display = 'block';
                return;
            }
            
            if (consumption <= 0 || monthlyAmount <= 0) {
                errorDiv.textContent = 'El consumo y el importe deben ser mayores que cero';
                errorDiv.style.display = 'block';
                return;
            }
            
            // Ocultar error
            errorDiv.style.display = 'none';
            
            // Calcular ahorro (30%)
            var savings = monthlyAmount * 0.30;
            var newAmount = monthlyAmount - savings;
            
            // Guardar datos en sessionStorage para usarlos después
            var calculatorData = {
                holderName: holderName,
                contractNumber: contractNumber,
                companyName: companyName,
                consumption: consumption,
                monthlyAmount: monthlyAmount,
                savings: savings,
                newAmount: newAmount
            };
            
            sessionStorage.setItem('calculatorData', JSON.stringify(calculatorData));
            
            // Redirigir a página de resultados
            window.app.navigate('results');
        };
    }
};

window.calculatorPage = calculatorPage;
