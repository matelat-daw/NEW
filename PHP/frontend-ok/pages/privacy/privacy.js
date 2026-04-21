// Privacy Page
var privacyPage = {
    init: function() {
        this.scrollToTop();
    },
    
    scrollToTop: function() {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
};

window.privacyPage = privacyPage;
