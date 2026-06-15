window.audioEngine = (() => {

    let musicPlayer = null;

    let currentMusicPath = null;

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

    function setMusicEnabled(enabled) {

        musicEnabled = enabled;

        if (musicPlayer !== null) {

            musicPlayer.muted = !enabled;
        }
    }

    function setMusicVolume(volume) {

        musicVolume = volume;

        if (musicPlayer !== null) {

            musicPlayer.volume = volume;
        }
    }

    function playSfx(path) {

        if (!sfxEnabled)
            return;

        const sfx = new Audio(path);

        sfx.volume = sfxVolume;

        sfx.play().catch(error => {
            console.error("SFX play failed:", error);
        });
    }

    function setSfxEnabled(enabled) {

        sfxEnabled = enabled;
    }

    function setSfxVolume(volume) {

        sfxVolume = volume;
    }

    return {

        initialize,

        playMusic,
        stopMusic,

        setMusicEnabled,
        setMusicVolume,

        playSfx,

        setSfxEnabled,
        setSfxVolume,

        stopAll
    };
})();