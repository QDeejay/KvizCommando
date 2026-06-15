window.screenWatcher = {
    init: function (dotNetRef) {
        function notify() {
            dotNetRef.invokeMethodAsync('UpdateScreenState', window.innerWidth, window.innerHeight);
        }

        notify(); // elsõ betöltéskor is
        window.addEventListener('resize', notify);

        window._screenWatcherCleanup = () => {
            window.removeEventListener('resize', notify);
        };
    },
    cleanup: function () {
        if (window._screenWatcherCleanup) window._screenWatcherCleanup();
    }
};
