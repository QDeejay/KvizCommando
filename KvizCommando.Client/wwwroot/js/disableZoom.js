(function () {
    function resetZoom() {
        document.body.style.zoom = "100%";
    }

    // Billentyűparancs figyelés (Ctrl + / Ctrl - / Ctrl 0)
    document.addEventListener("keydown", function (e) {
        if (e.ctrlKey && (e.key === "+" || e.key === "-" || e.key === "0")) {
            e.preventDefault();
            resetZoom();
        }
    });

    // Egérgörgős nagyítás (Ctrl + görgetés)
    document.addEventListener("wheel", function (e) {
        if (e.ctrlKey) {
            e.preventDefault();
            resetZoom();
        }
    }, { passive: false });

    // Biztos ami biztos: ha a böngésző mégis zoomolna, visszaállítjuk
    window.addEventListener("resize", resetZoom);
})();
