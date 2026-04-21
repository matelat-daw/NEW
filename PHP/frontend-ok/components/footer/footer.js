// Footer Component
var footerComponent = {
    init: function() {
        this.loadFooterHTML();
    },
    
    loadFooterHTML: function() {
        var container = document.getElementById('footer-component');
        if (!container) return;
        
        var footer_url = '/frontend/components/footer/footer.html?v=' + new Date().getTime();
        fetch(footer_url)
            .then(function(response) { return response.text(); })
            .then(function(html) {
                container.innerHTML = html;
                
                // Cargar CSS del footer (con cache buster)
                var link = document.createElement('link');
                link.rel = 'stylesheet';
                link.href = '/frontend/components/footer/footer.css?v=' + new Date().getTime();
                document.head.appendChild(link);
            })
            .catch(function(error) {});
    }
};

window.footerComponent = footerComponent;
