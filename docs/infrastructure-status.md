# Félkész infrastruktúrák állapota

Ez a dokumentum azokat a technikai elemeket foglalja össze, amelyeknek már van helyük a kódban, de még nem tekinthetők végleges infrastruktúrának. A felsorolt komponensek jelenlegi állapota tudatos átmeneti állapot; éles üzem előtt külön lezárást igényelnek.

## Adatbázis-szolgáltató

A fejlesztési és bírálói környezet SQLite adatbázist használ, mert nem igényel külön adatbázis-szervert, és a projekt másik gépen is azonnal futtatható. Az SQL Serverhez szükséges EF Core csomag és a kapcsolódó konfigurációs minták megmaradtak, de a szolgáltató kiválasztása még nincs központi kapcsoló mögé rendezve.

A végleges szolgáltatóváltás feladatai:

- a szolgáltató kiválasztása konfigurációból;
- mindkét `DbContext` azonos döntés alapján történő regisztrálása;
- szolgáltatónként megfelelő kapcsolati karakterlánc használata;
- a szolgáltatófüggő indexek és migrációk ellenőrzése;
- SQLite és SQL Server indítási próba ugyanazzal az alkalmazáskóddal.

## E-mail-kézbesítés

A `WhitelistedEmailSender` jelenleg fájlba írja a regisztrációs és jelszó-visszaállítási leveleket. Ez fejlesztési segédmegoldás: nem kézbesít valódi levelet, és a kimeneti könyvtár még helyi fejlesztői útvonalhoz kötött.

Éles üzem előtt a `FileEmailDelivery` helyére szolgáltatói implementáció szükséges. A callback URL ellenőrzése és az engedélyezett hostok korlátozása a szolgáltató cseréje után is megtartandó biztonsági követelmény.

## Auditnapló

Az `AuditLogger` soronként JSON-formátumú bejegyzéseket ír helyi fájlba. A folyamaton belüli írásokat saját lock rendezi sorba, de ez nem biztosít több alkalmazáspéldány közötti szinkronizációt, naplórotációt vagy változtathatatlan tárolást.

Éles környezetben központi, hozzáférés-szabályozott naplógyűjtés szükséges. Meg kell határozni a megőrzési időt, a lekérdezési jogosultságokat és az incidensvizsgálathoz szükséges eseménykört is.

## Személyes adatok és GDPR

A `PersonalDataOptions` a későbbi export- és törlési folyamat bővítési pontja. Jelenleg nem végez adatexportot, törlést vagy kulcskezelést.

A `DummyEncryptionProvider` és a `DummyUserPiiService` nem éles adatvédelem. A kód csak az interfészek és az adatmodell kipróbálását szolgálja; a Base64-kódolás nem titkosítás. Valódi személyes adat csak hitelesített titkosítás, megfelelő kulcskezelés és dokumentált kulcsrotáció bevezetése után tárolható ezen az útvonalon.

A végleges GDPR-folyamatnak legalább az alábbiakat kell lefednie:

- felhasználóhoz kapcsolódó adatok összegyűjtése és hordozható exportja;
- törlési vagy anonimizálási szabályok;
- jogszabály vagy elszámolás miatt megőrzendő adatok elkülönítése;
- műveletek auditálása;
- a biztonsági mentésekre vonatkozó megőrzési eljárás.

## Hitelesítési diagnosztika

A `Diagnostics:EnableAuthenticationDebugLogging` kapcsoló a cookie- és bearer-hitelesítés hibáinak vizsgálatára szolgál mobil és asztali kliens tesztelésekor. Alapértéke `false`. A diagnosztika nem naplózhat tokent, teljes Authorization fejlécet vagy felhasználói azonosítót.

A `/signin-facebook` callback-diagnosztika ugyanehhez a kapcsolóhoz kötött, és kizárólag a külső hitelesítés sikerességét naplózza. A külső principalt és a hitelesítési tulajdonságok értékeit nem írja ki.

## Háttérfolyamatok

A regisztrált háttérszolgáltatások jelenleg a lejárt tokenek és a játékoscache tartósításának feladatait végzik. Az inaktív felhasználók értesítése csak korábbi elképzelés volt, ezért nincs kikommentelt szolgáltatásregisztrációként fenntartva. Új háttérfeladat csak konkrét élettartam-, hibakezelési és leállítási szabályokkal kerüljön a szolgáltatások közé.
