# Biztonsági és konkurenciakezelési megjegyzések

Ez a dokumentum a módosítások során megőrzendő biztonsági és párhuzamossági feltételeket rögzíti. Nem helyettesíti a teszteket, de összefoglalja azokat az invariánsokat, amelyek megsértése nehezen reprodukálható hibát vagy adatvédelmi problémát okozhat.

## Hitelesítés és munkamenet

- A böngészős kliens cookie-, a mobil és asztali kliens bearer-hitelesítést használhat.
- A naplózás csak a hitelesítési mód sikerességét és a kérés technikai útvonalát rögzítheti. Token, jelszó-visszaállító kód és teljes Authorization fejléc nem kerülhet naplóba.
- A callback URL csak ellenőrzött hostra mutathat. Az ellenőrzés megkerülése nyílt átirányítást és tokenkiszivárgást okozhat.
- A Terms claim frissítése és a központi ÁSZF-verzió összevetése együtt biztosítja, hogy elavult elfogadás ne adjon hozzáférést.

## Játékoscache

Egy játékos módosítható állapotát a hozzá tartozó `CacheEntry.Lock` védi. A `LockedAsync` nevű műveletek ennek a játékoslocknak a birtokában olvasnak vagy írnak. Új módosítás nem kerülheti meg ezt az útvonalat, ha ugyanazt a cache-bejegyzést párhuzamos kérés is elérheti.

A lock alatt végzett callbacknek rövidnek és kiszámíthatónak kell maradnia. Más játékos lockjának megszerzése vagy tetszőleges külső hívás bevezetése holtpontot és indokolatlan várakozást okozhat. A dirty jelzők a memóriabeli változás és a későbbi adatbázis-mentés közötti szerződés részei; módosítás után a megfelelő szegmenst mindig meg kell jelölni.

## VS matchmaking és meccsállapot

- A várólista közös gyűjteményeit a szolgáltatás saját `_syncRoot` lockja védi.
- Egy lezárt meccs belső állapotát a `VsMatchSession.SyncRoot` védi.
- A két lock hatókörét nem szabad önkényesen egymásba ágyazni. A hosszú vagy aszinkron munka a lehető legrövidebb zárolt szakasz után fusson.
- A fázisváltás és az ahhoz tartozó snapshot ugyanabból a konzisztens állapotból készüljön.
- A szerver által hitelesített jutalommentés nem függhet az eredeti klienssession további érvényességétől.

## Egyéni játék

Az aktív egyéni játékok gyorsítótára játékazonosító és játékosazonosító alapján is kereshető. A létrehozás, lekérdezés és eltávolítás közötti kapcsolatot meg kell őrizni, hogy egy játékoshoz ne maradjon árva aktív munkamenet.

## Auditnapló

A fájlos auditlogger singleton élettartamú; egy közös `SemaphoreSlim` akadályozza meg, hogy az ugyanazon alkalmazáspéldányból érkező sorok összecsússzanak. A zárolás hatóköre kizárólag a könyvtárkarbantartásra és a fájlírásra terjed ki. Több alkalmazáspéldány közötti koordinációt és manipuláció elleni védelmet nem biztosít.

Az auditfájlok naponta elkülönülnek, a konfigurált megőrzési időnél régebbi fájlokat a logger eltávolítja. A takarítás kizárólag a beállított auditkönyvtár közvetlen `audit-*.jsonl` fájljait érintheti; általános könyvtártörlés nem vezethető be.

IP-cím csak bekapcsolt opció és külön titkos kulcs mellett, HMAC formában kerülhet a bejegyzésbe. A hash nem jelent automatikus anonimizálást. E-mail-cím, token, jelszó-visszaállító kód, cookie, Authorization fejléc és request body auditba írása tilos.
