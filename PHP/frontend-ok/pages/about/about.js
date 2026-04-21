// About Page
var aboutPage = {
    init: function() {
        this.scrollToTop();
    },
    
    scrollToTop: function() {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    }
};

window.aboutPage = aboutPage;
