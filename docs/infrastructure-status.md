# Átmeneti infrastruktúrák állapota

Ez a dokumentum megmutatja, mely helyi infrastruktúrák működnek már a fejlesztői és bírálói környezetben, és mit kell még production üzem előtt lecserélni vagy megerősíteni. Az átmeneti megoldások külön interfészek mögött vannak, ezért később az üzleti folyamatok átírása nélkül cserélhetők.

## Indítási szerkezet

A `Program.cs` közvetlen környezetéhez tartozó kód a `KvizCommando.Server/Startup` alatt található:

- `DependencyInjection`: `IServiceCollection`-regisztrációk;
- `Pipeline`: a HTTP-feldolgozási lánc extensionjei;
- `Endpoints`: a Programból meghívott végpontleképezések és endpoint-konvenciók.

Az `Identity` mappa az Identity modelljeit és szabályait, az `Infrastructure` pedig a konkrét technikai adaptereket tartalmazza. Az Identity-végpontokat a `MapKvizCommandoIdentityEndpoints` fogja össze; ez nem üzleti logikát rejt el, hanem az összetartozó végpont-regisztrációkat tartja egy helyen.

## Adatbázis-szolgáltató

A fejlesztési és bírálói környezet alapértelmezetten SQLite adatbázist használ, mert nem igényel külön adatbázis-szervert, és másik gépen is elindítható. A `Program.cs` elején lévő `USE_SQL_SERVER` kapcsoló `false` értéke SQLite-ot, `true` értéke SQL Servert választ; mindkét `DbContext` mindig ugyanazt a providert használja.

Az SQL Server-kapcsolati karakterláncok üresen maradnak a repóban. Csak a helyi `secrets.json` vagy az éles környezet titkos konfigurációja töltheti ki őket. SQLite kiválasztásakor az SQL Server kontextusai nem kerülnek a futó alkalmazásba, és a program nem próbál SQL Serverhez kapcsolódni.

A négy külön migrációs lánc és az adatbázisok frissítési menete a `docs/database-providers-and-migrations.md` fájlban található.

## E-mail-kézbesítés

Az Identity e-mail-küldője előállítja a lokalizált levél tárgyát, szöveges törzsét, HTML-törzsét és célhivatkozását, majd egy `IEmailDelivery` adapternek adja át. A levél összeállítása ezért nem függ a kézbesítés módjától.

Jelenlegi adapter:

```text
IEmailDelivery -> FileEmailDelivery
```

A `FileEmailDelivery` minden levélből azonos nevű `.eml`, `.html` és `.txt` fájlt készít. Az `.eml` szabályos `multipart/alternative` üzenet, a `.html` pedig közvetlenül megnyitható a böngészőben.

A fájlok címzettet és egyszer használatos megerősítő vagy visszaállító linket tartalmazhatnak. Kizárólag tesztadatokkal használhatók, nem kerülhetnek verziókezelésbe, és a teszt lezárása után törlendők.

Az alapértelmezett gyökér a szerver projektkönyvtárához viszonyított `App/Email`. A levelek típus és UTC-dátum szerint kerülnek almappába:

```text
KvizCommando.Server/App/Email/
├── Registration/yyyy-MM-dd/
├── PasswordReset/yyyy-MM-dd/
├── EmailChange/yyyy-MM-dd/
├── PasswordChanged/yyyy-MM-dd/
└── AccountDeleted/yyyy-MM-dd/
```

Ha az `Email:OutputRoot` abszolút útvonal, a rendszer azt használja. Relatív értéknél a szerver tartalomgyökeréből indul ki.

Az `Email:Service` jelenlegi értéke `File`. A `Mail` érték a későbbi SMTP- vagy API-alapú adapter cserepontja; ilyen adapter a diplomamunka részeként nem készül. Ha valaki idő előtt `Mail` értéket állít be, az alkalmazás érthető konfigurációs hibával leáll.

Az e-mailben szereplő link gyökere az `Email:ActiveBaseUrl` kulccsal választható:

- `Localhost`: `https://localhost:7229`;
- `LocalNetwork`: `https://192.168.0.220:7229`;
- `PublicTunnel`: a fejlesztési publikus cím.

A link hostját az engedélyezett Base URL-ekből képzett lista ellenőrzi. Új host felvételekor a konfigurációt kell bővíteni, nem a levélsablont vagy a C#-kódot.

Development környezetben az `api/auth/options` válasz a regisztrációs és a jelszó-visszaállítási típusmappát adja vissza, például `KvizCommando.Server/App/Email/Registration`. A felület ezt a sikeres művelet után megmutatja a bírálónak. A napi almappát nem teszi hozzá, ezért a legfrissebb dátumú könyvtárban kell keresni a levelet. Production környezetben helyi fájlútvonal nem kerül a klienshez.

## Auditnapló

A `FileAuditLogger` alapértelmezetten a `KvizCommando.Server/App/Audit` könyvtárba ír napi `audit-yyyy-MM-dd.jsonl` fájlokat. Egyetlen alkalmazáspéldányon belül singleton élettartam és közös `SemaphoreSlim` rendezi sorba az írásokat. A `RetentionDays` értéknél régebbi, az auditkönyvtárban található napi fájlokat a logger eltávolítja.

Egy bejegyzés tartalma:

- UTC időpont;
- stabil eseménynév;
- `Accepted`, `Succeeded`, `Failed` vagy `Denied` eredmény;
- `ActorId`: a művelet végrehajtójának belső azonosítója, ha ténylegesen ismert;
- `SubjectId`: az érintett fiók belső azonosítója, ha biztonságosan azonosítható;
- opcionális, kulcsolt IP-hash;
- technikai kérésazonosító;
- kizárólag engedélyezett `ChangedFields` vagy `DocumentVersion` részletek.

Nem kerülhet auditba e-mail-cím, jelszó, megerősítő token, jelszó-visszaállító kód, cookie, Authorization fejléc vagy request body.

Az elfelejtett jelszó művelet anonim `Identity.PasswordResetRequested` esemény. Az audit nem jelzi, hogy a megadott címhez tartozott-e fiók. A sikeres és sikertelen jelszó-visszaállítás külön esemény; az érintett belső azonosítóját a szerver az Identity-adattárból határozza meg anélkül, hogy az e-mail-címet naplózná.

A működő folyamatok a regisztrációt, a helyi és külső bejelentkezést, a zárolást, a kijelentkezést, a sessioncserét és -visszavonást, a jelszómódosítást, az e-mail-változás megerősítését, a külső login kapcsolását és eltávolítását, valamint az ÁSZF elfogadását auditálják. Az ÁSZF-esemény `DocumentVersion` részlete a `TermsConsents` táblába mentett verzióval azonos.

A személyesadat-export `Privacy.DataExport`, a fióktörlés pedig `Privacy.Erasure` eseményt ír. A helyesbítéshez, adatkezelési korlátozáshoz és tiltakozáshoz tartozó eseménynevek egyelőre csak fenntartott konstansok; ezeket jelenleg nem írja végpont az auditnaplóba.

Az `IncludeIpHash` jelenleg be van kapcsolva. Csak érvényes Base64-formátumú `AuditHash:Secret` mellett készül HMAC-SHA256 hash. Az IP-cím a hash előtt normalizálásra kerül, így az IPv4 és annak IPv4-be ágyazott IPv6 alakja azonos bemenetet ad. A hash-elt IP továbbra is személyhez kapcsolható adat lehet, ezért használata külön célt és megőrzési szabályt igényel.

A helyi fájl nem változtathatatlan, nem kezel több alkalmazáspéldányt és nem biztosít központi jogosultságkezelést. Production környezetben központi, hozzáférés-szabályozott audittároló szükséges. A GDPR szempontjából az audit célhoz kötöttségét, adattakarékosságát, megőrzését és védelmét együtt kell meghatározni; a kiinduló elvek a GDPR 5. és 32. cikkében találhatók.

## Személyes adatok mezőszintű védelme

A `UserPiiService` kizárólag az Identityn kívüli telefonszám- és számlázási mezők adatbázisba írását és visszaolvasását végzi. Az e-mail-cím az ASP.NET Core Identity `AspNetUsers` táblájában marad; a `UserPii` nem tárol e-mail-másolatot vagy keresési hasht.

Az `AesGcmEncryptionProvider` AES-256-GCM hitelesített titkosítást használ. Minden mezőhöz új 12 bájtos nonce és 16 bájtos hitelesítési tag készül. A `PiiEncryption:Key` beállításnak 32 véletlen bájt Base64-alakját kell tartalmaznia. A program hiányzó vagy hibás kulccsal nem indul el.

A kulcs fejlesztésben a Gitből kizárt `secrets.json` fájlban tárolható. Production környezetben a választott platform titoktára szükséges. A jelenlegi egyszerű szolgáltatás nem végez automatikus kulcsrotációt: meglévő adatok mellett a kulcs csak a mezők ellenőrzött visszafejtése és újratitkosítása után cserélhető.

Az ASP.NET Data Protection nem azonos a mezőszintű PII-titkosítással. Előbbi a framework cookie- és tokenfolyamatait védi, utóbbi a számlázási és kapcsolattartási mezőket. A részletes működést és az adatbázis újrainicializálását a `docs/secrets-and-data-protection.md` tartalmazza.

## GDPR-folyamatok

A profil adatvédelmi felületéről két működő, jelszavas újrahitelesítést kérő művelet érhető el:

- a személyesadat-export ZIP-fájlt készít a fiók-, Identity-, játékos-, játék- és kérdésadatokból; export előtt a memóriában módosult játékosadatokat is kiírja;
- a fióktörlés eltávolítja a játékoshoz, kérdésekhez és fiókhoz kapcsolódó adatokat, az Identity-felhasználót is törli, majd megkísérli elküldeni a törlési értesítést.

Mindkét végpont kéréskorlátozást használ és auditbejegyzést ír. A `PersonalDataOptions` jelenleg egyik folyamatot sem vezérli; megmaradt egy későbbi, kiegészítő adatvédelmi kulcskezelés konfigurációs cserepontjaként.

Production előtt továbbra is külön üzemi szabály kell a jogszabály alapján kötelezően megőrzendő adatokhoz, a biztonsági mentések életciklusához, valamint a helyesbítési, korlátozási és tiltakozási kérelmek dokumentált kezeléséhez.

## Hitelesítési diagnosztika

A `Diagnostics:EnableAuthenticationDebugLogging` kapcsoló csak Development környezetben aktiválható. A diagnosztika a cookie- és bearer-hitelesítés sikerességét, a technikai útvonalat és a válasz státuszát naplózhatja, de tokent, cookie-t, teljes Authorization fejlécet vagy felhasználói személyes adatot nem.

Production környezetben a kapcsoló értékétől függetlenül nem indul el a diagnosztikai middleware.

## Háttérfolyamatok

A regisztrált háttérszolgáltatások a lejárt tokenek és a játékoscache tartósításának feladatait végzik. Új háttérfeladat csak konkrét élettartam-, hibakezelési és leállítási szabályokkal kerülhet a szolgáltatások közé.
