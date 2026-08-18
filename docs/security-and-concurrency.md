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

A helyi auditfájl írását folyamaton belüli lock védi. Ez kizárólag az egy alkalmazáspéldányból érkező sorok összecsúszását akadályozza meg; több példány, fájlrotáció és manipuláció elleni védelem nincs megoldva. Az IP-cím hash-elése önmagában nem jelent anonimizálást, ezért a megőrzési és hozzáférési szabályokat az éles naplózási megoldásban külön kell meghatározni.
