window.kcHowler = (() => {
    const sfxByPath = new Map();
    let music = null;
    let musicPath = null;
    let musicVolume = 1;
    let sfxVolume = 1;

    function clampVolume(value) {
        const numericValue = Number(value);

        if (!Number.isFinite(numericValue))
            return 0;

        return Math.min(1, Math.max(0, numericValue));
    }

    function getSfx(path) {
        let sound = sfxByPath.get(path);

        if (sound !== undefined)
            return sound;

        sound = new Howl({
            src: [path],
            preload: true,
            volume: sfxVolume
        });
        sfxByPath.set(path, sound);

        return sound;
    }

    function setMuted(muted) {
        Howler.mute(Boolean(muted));
    }

    function playMusic(path) {
        if (music !== null && musicPath === path) {
            if (!music.playing())
                music.play();

            return;
        }

        if (music !== null)
            music.unload();

        musicPath = path;
        music = new Howl({
            src: [path],
            loop: true,
            preload: true,
            volume: musicVolume
        });
        music.play();
    }

    function stopMusic() {
        if (music === null)
            return;

        music.unload();
        music = null;
        musicPath = null;
    }

    function setMusicVolume(volume) {
        musicVolume = clampVolume(volume);

        if (music !== null)
            music.volume(musicVolume);
    }

    function preloadSfx(paths) {
        for (const path of paths)
            getSfx(path);
    }

    function playSfx(path) {
        getSfx(path).play();
    }

    function setSfxVolume(volume) {
        sfxVolume = clampVolume(volume);

        for (const sound of sfxByPath.values())
            sound.volume(sfxVolume);
    }

    function stopAll() {
        stopMusic();

        for (const sound of sfxByPath.values())
            sound.stop();
    }

    return {
        setMuted,
        playMusic,
        stopMusic,
        setMusicVolume,
        preloadSfx,
        playSfx,
        setSfxVolume,
        stopAll
    };
})();
