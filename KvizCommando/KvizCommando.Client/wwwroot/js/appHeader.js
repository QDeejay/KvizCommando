console.log('Bootstrap Dropdown API:', !!(window.bootstrap && bootstrap.Dropdown));

function clearLanguageCache() {
    console.log("[Lang] Selective cache clear (lang*)");
    const keysToDelete = [];
    for (let i = 0; i < sessionStorage.length; i++) {
        const key = sessionStorage.key(i);
        if (key && key.startsWith("lang")) {
            keysToDelete.push(key);
        }
    }
    keysToDelete.forEach(k => {
        sessionStorage.removeItem(k);
        console.log("[Lang] Removed", k);
    });
}

window.appHeader = {
    setUser: function (isAuthenticated, displayName) {
        const lang = document.getElementById('langSelector');
        if (!lang) return;

        if (isAuthenticated) {
            lang.classList.add('d-none');
        } else {
            lang.classList.remove('d-none');
        }
    },

    setLang: function (culture) {
        const current = sessionStorage.getItem("userLang");
        if (current === culture) {
            console.log("[Lang] Culture already set to", culture, "- no reload");
            return;
        }

        clearLanguageCache();
        sessionStorage.setItem("userLang", culture);
        console.log("[Lang] Culture changed to", culture, "- reloading...");
        location.reload();
    }
};
