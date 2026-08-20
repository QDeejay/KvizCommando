window.audioEngine = (() => {

    let musicPlayer = null;
    const activeSfx = new Set();
    const forcedUiSfx = new Set();
    const preloadedSfx = new Map();

    const clickSfxPath = "audio/sfx/Click.webm?v=3";
    const uiTouchSfxPath = "audio/sfx/UiTouch.webm?v=3";

    let currentMusicPath = null;

    let masterMuted = true;
    let musicEnabled = true;
    let sfxEnabled = true;

    let musicVolume = 1.0;
    let sfxVolume = 1.0;

    let fadeOperationId = 0;

    function registerAudioUnlock() {
        const unlockAudio = () => {
            const audio = new Audio(clickSfxPath);
            audio.volume = 0;
            audio.play()
                .then(() => {
                    audio.pause();
                    audio.currentTime = 0;
                })
                .catch(() => { });

            document.removeEventListener("click", unlockAudio);
            document.removeEventListener("keydown", unlockAudio);
        };

        document.addEventListener("click", unlockAudio);
        document.addEventListener("keydown", unlockAudio);
    }

    registerAudioUnlock();

    function registerUiClickDelegation() {
        document.addEventListener("click", event => {
            const source = event.target instanceof Element
                ? event.target
                : null;

            if (source === null)
                return;

            const helpControl = source.closest(".kc-help-window button, .kc-help-window [role='button']");

            if (helpControl !== null) {
                if (!isDisabledControl(helpControl))
                    playSfxInternal(uiTouchSfxPath, false);

                return;
            }

            const uiControl = source.closest(
                ".kc-lcd-surface button, button.kc-lcd-surface, " +
                ".button-on-lcd, .button-on-lcd--text, .navigation-button, " +
                ".app-header .dropdown-item, .htp-tab, .kc-back-button, " +
                ".kc-profile-window button, .kc-profile-window [role='button']");

            if (uiControl === null || isDisabledControl(uiControl))
                return;

            const isMasterSoundControl = uiControl.matches(".lcd-sound-control");
            playSfxInternal(uiTouchSfxPath, isMasterSoundControl);
        }, true);
    }

    function isDisabledControl(control) {
        return control.matches(":disabled, .disabled, [aria-disabled='true']");
    }

    registerUiClickDelegation();

    async function fadeVolume(audio, start, end, duration, operationId) {

        const steps = 30;

        const interval = duration / steps;

        const delta = (end - start) / steps;

        audio.volume = start;

        for (let i = 0; i < steps; i++) {

            if (operationId !== fadeOperationId)
                return;

            audio.volume += delta;

            await delay(interval);
        }

        audio.volume = end;
    }

    function delay(ms) {

        return new Promise(resolve => setTimeout(resolve, ms));
    }

    // =========================================
    // PUBLIC API (NON-BLOCKING)
    // =========================================

    function playMusic(path) {

        playMusicInternal(path);
    }

    function stopMusic() {

        stopMusicInternal();
    }

    function stopAll() {

        stopMusicInternal();
        stopSfx();
    }

    // =========================================
    // INTERNAL ASYNC OPERATIONS
    // =========================================

    async function playMusicInternal(path) {

        if (!musicEnabled)
            return;

        // Same track already playing
        if (currentMusicPath === path &&
            musicPlayer !== null &&
            !musicPlayer.paused) {

            return;
        }

        fadeOperationId++;

        const operationId = fadeOperationId;

        // Fade out current
        if (musicPlayer !== null) {

            await fadeVolume(
                musicPlayer,
                musicPlayer.volume,
                0,
                500,
                operationId);

            if (operationId !== fadeOperationId)
                return;

            musicPlayer.pause();
            musicPlayer.currentTime = 0;
        }

        currentMusicPath = path;

        musicPlayer = new Audio(path);

        musicPlayer.loop = true;
        musicPlayer.muted = masterMuted;
        musicPlayer.volume = 0;

        try {

            await musicPlayer.play();

            await fadeVolume(
                musicPlayer,
                0,
                musicVolume,
                700,
                operationId);
        }
        catch (error) {

            console.error("Music play failed:", error);
        }
    }

    async function stopMusicInternal() {

        if (musicPlayer === null)
            return;

        fadeOperationId++;

        const operationId = fadeOperationId;

        await fadeVolume(
            musicPlayer,
            musicPlayer.volume,
            0,
            500,
            operationId);

        if (operationId !== fadeOperationId)
            return;

        musicPlayer.pause();
        musicPlayer.currentTime = 0;

        currentMusicPath = null;
    }

    // =========================================
    // SETTINGS
    // =========================================

    function setMuted(muted) {

        masterMuted = muted;

        if (musicPlayer !== null)
            musicPlayer.muted = masterMuted || !musicEnabled;

        for (const sfx of activeSfx)
            sfx.muted = masterMuted || !sfxEnabled;
    }

    function setMusicEnabled(enabled) {

        musicEnabled = enabled;

        if (musicPlayer !== null) {

            musicPlayer.muted = masterMuted || !enabled;
        }
    }

    function setMusicVolume(volume) {

        musicVolume = volume;

        if (musicPlayer !== null) {

            musicPlayer.volume = volume;
        }
    }

    function preloadSfx(paths) {

        for (const path of paths) {

            if (preloadedSfx.has(path))
                continue;

            const sfx = new Audio(path);
            sfx.preload = "auto";
            sfx.load();
            preloadedSfx.set(path, sfx);
        }
    }

    function playSfx(path) {

        playSfxInternal(path, false);
    }

    function playSfxInternal(path, forceAudible) {

        let sfx = preloadedSfx.get(path);

        if (sfx === undefined || !sfx.paused) {

            sfx = new Audio(path);
        }
        else {

            sfx.currentTime = 0;
        }

        sfx.muted = forceAudible
            ? false
            : masterMuted || !sfxEnabled;
        sfx.volume = sfxVolume;
        const collection = forceAudible
            ? forcedUiSfx
            : activeSfx;
        collection.add(sfx);

        const release = () => collection.delete(sfx);
        sfx.onended = release;
        sfx.onerror = release;

        sfx.play().catch(error => {
            release();
            console.error("SFX play failed:", error);
        });
    }

    function stopSfx() {

        for (const sfx of activeSfx) {

            sfx.pause();
            sfx.currentTime = 0;
        }

        activeSfx.clear();

        for (const sfx of forcedUiSfx) {

            sfx.pause();
            sfx.currentTime = 0;
        }

        forcedUiSfx.clear();
    }

    function setSfxEnabled(enabled) {

        sfxEnabled = enabled;

        for (const sfx of activeSfx)
            sfx.muted = masterMuted || !enabled;
    }

    function setSfxVolume(volume) {

        sfxVolume = volume;
    }

    return {
        playMusic,
        stopMusic,

        setMuted,

        setMusicEnabled,
        setMusicVolume,

        preloadSfx,
        playSfx,

        setSfxEnabled,
        setSfxVolume,

        stopAll
    };
})();

/**
 * MÓDOSÍTÁS: a motor közös master mute állapotot használ. Némításkor
 * a zene és az aktív effektek nem állnak le, csak elnémulnak; az új
 * lejátszások is némán futnak. Feloldáskor az aktuális idővonalon
 * folytatódik a hang. A publikus API a működésének megfelelő
 * setMuted nevet használja.
 * MÓDOSÍTÁS: az effektek külön lejátszás nélkül előtölthetők. Az első
 * kattintás is a már betöltött példányt használja; párhuzamos lejátszásnál
 * a motor csak az adott alkalomhoz készít új Audio példányt.
 */
