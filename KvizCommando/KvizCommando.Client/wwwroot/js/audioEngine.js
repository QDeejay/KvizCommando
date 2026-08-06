window.audioEngine = (() => {

    let musicPlayer = null;
    const activeSfx = new Set();

    let currentMusicPath = null;

    let masterMuted = true;
    let musicEnabled = true;
    let sfxEnabled = true;

    let musicVolume = 1.0;
    let sfxVolume = 1.0;

    let initialized = false;

    let fadeOperationId = 0;

    function initialize() {

        if (initialized)
            return;

        initialized = true;

        const unlockAudio = () => {

            const audio = new Audio();

            audio.play().catch(() => { });

            document.removeEventListener("click", unlockAudio);
            document.removeEventListener("keydown", unlockAudio);
        };

        document.addEventListener("click", unlockAudio);
        document.addEventListener("keydown", unlockAudio);
    }

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

    function setEnabled(enabled) {

        masterMuted = !enabled;

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

    function playSfx(path) {

        const sfx = new Audio(path);

        sfx.muted = masterMuted || !sfxEnabled;
        sfx.volume = sfxVolume;
        activeSfx.add(sfx);

        const release = () => activeSfx.delete(sfx);
        sfx.addEventListener("ended", release, { once: true });
        sfx.addEventListener("error", release, { once: true });

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

        initialize,

        playMusic,
        stopMusic,

        setEnabled,

        setMusicEnabled,
        setMusicVolume,

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
 * folytatódik a hang.
 */
