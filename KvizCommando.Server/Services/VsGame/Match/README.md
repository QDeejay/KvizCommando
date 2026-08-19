# VS meccs működése

A meccskezelés már korábban is feladat szerint volt szétbontva. Ezen most nem változtattunk; ez a leírás azért került ide, hogy a mappák neve mögött ne kelljen találgatni.

- Core: a meccs alapfolyamata és közös működése;
- Preparation: a kezdés előtti állapot összeállítása;
- Questions: kérdésválasztás és a kérdéskör kezelése;
- Gameplay: a játék közbeni műveletek;
- Rewards: jutalmak és eredmények feldolgozása;
- State: a meccs állapotának kezelése.

Mindegyik rész a KvizCommando.Server.Services.VsGame.Match namespace-ben marad. Ezek ugyanannak a meccsszolgáltatásnak a részfájljai, nem egymástól független almodulok. A közös namespace ezt pontosabban fejezi ki, mint a könyvtárszerkezet mechanikus lemásolása.

Ha valamelyik terület később önálló szolgáltatássá válik saját publikus interfésszel és külön életciklussal, akkor érdemes lesz külön namespace-t adni neki. Addig az almappák feladata csak az, hogy gyorsan megtaláljuk, hol történik egy adott meccslépés.
