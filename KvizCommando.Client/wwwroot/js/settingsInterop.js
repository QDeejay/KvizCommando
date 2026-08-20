window.settingsInterop = (() => {
    function setTheme(theme) {
        document.documentElement.dataset.theme = theme;
    }

    async function tryEnterFullscreen() {
        if (document.fullscreenElement !== null)
            return;

        try {
            await document.documentElement.requestFullscreen();
        }
        catch {
            // A böngésző megtagadhatja, ha a kéréshez nincs aktív felhasználói gesztus.
        }
    }

    async function exitFullscreen() {
        if (document.fullscreenElement === null)
            return;

        try {
            await document.exitFullscreen();
        }
        catch {
            // Kilépés közben megszűnhet a fullscreen állapot; ez nem logout hiba.
        }
    }

    return {
        setTheme,
        tryEnterFullscreen,
        exitFullscreen
    };
})();
