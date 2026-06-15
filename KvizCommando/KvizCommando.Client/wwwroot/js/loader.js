(function () {
    let isVisible = false;
    let showTimer = null;
    let hideTimer = null;
    let lastShownAt = 0;
    console.log(">>> loader.js loaded");

    function show() {
        console.log("[JS LOADER] Called");

        if (isVisible) {
            // Már látszik → újraindítjuk a minimum 1s időzítőt
            lastShownAt = Date.now();
            console.log("[JS LOADER] already visible, reset timer");
            return;
        }

        if (showTimer) {
            return; // Már folyamatban van egy 500ms késleltetés
        }

        console.log("[JS LOADER] Started delay");
        showTimer = setTimeout(() => {
            const overlay = document.getElementById("loader-overlay");
            if (!overlay) {
                console.warn("Loader overlay not found");
                return;
            }
            overlay.classList.add("active");
            isVisible = true;
            lastShownAt = Date.now();
            showTimer = null;
            console.log("[JS LOADER] now visible");
        }, 500);
    }

    function hide() {
        console.log(">>> HIDE called");

        if (showTimer) {
            clearTimeout(showTimer);
            showTimer = null;
            return; // még nem látszott, nem kell semmi
        }

        if (!isVisible) return;

        const elapsed = Date.now() - lastShownAt;
        const overlay = document.getElementById("loader-overlay");
        if (!overlay) return;

        const doHide = () => {
            overlay.classList.remove("active");
            isVisible = false;
            hideTimer = null;
            console.log("[JS LOADER] hidden");
        };

        if (elapsed < 1000) {
            // Még nincs meg az 1s → várunk
            if (hideTimer) clearTimeout(hideTimer);
            hideTimer = setTimeout(doHide, 1000 - elapsed);
        } else {
            doHide();
        }
    }

    function isActive() {
        return isVisible;
    }

    // Globális elérés
    window.kcLoader = {
        show,
        hide,
        isActive
    };
})();
