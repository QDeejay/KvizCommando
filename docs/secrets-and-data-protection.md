# Titkos kulcsok és személyes adatok a Kviz Commandóban

Ez a leírás a Kviz Commando jelenlegi működését foglalja össze. A célja az, hogy egyértelmű legyen, melyik adat hol található, melyik kulcs mire szolgál, és mit kell megőrizni vagy lecserélni.

## A működő rendszer röviden

Az ASP.NET Core Identity kezeli a fiók működéséhez szükséges adatokat. Az e-mail-cím az `AspNetUsers` táblában marad, mert a beépített Identity erre építi a regisztrációt, az egyediségvizsgálatot, a bejelentkezést, a jelszó-visszaállítást és a Facebook-fiók egyeztetését.

A külön `UserPii` tábla nem másolja le az e-mail-címet. Kizárólag az Identityn kívüli, fokozottabban védendő kapcsolattartási és számlázási adatokat tárolja:

```text
UserId
PhoneEncrypted + PhoneNonce + PhoneTag
BillingNameEncrypted + BillingNameNonce + BillingNameTag
BillingAddressEncrypted + BillingAddressNonce + BillingAddressTag
CreatedUtc
UpdatedUtc
```

A `UserId` olvasható marad, mert ez kapcsolja a PII-rekordot az `AspNetUsers.Id` értékéhez. Valódi idegen kulcs védi a kapcsolatot, és az Identity-fiók fizikai törlése a hozzá tartozó `UserPii` rekordot is törli. A titkosítás célja nem az adatbázis teljes anonimizálása, hanem az, hogy az adatbázisfájl megszerzése önmagában ne tegye olvashatóvá a telefonszámot, a számlázási nevet és a számlázási címet.

## Mi nincs a rendszerben?

Nincs külön e-mail-hash és nincs e-mail pepper. Ezekre nincs szükség, mert az e-mailt továbbra is az ASP.NET Core Identity kezeli és keresi az `AspNetUsers` táblában.

Nincs külön telefonszám-hash sem. A program jelenleg nem keres felhasználót telefonszám alapján.

A korábbi `EmailLookup`, `IEmailLookup`, `EmailHashPepper` és `DummyEncryptionProvider` csak egy félkész, a működő Identityvel párhuzamos irány részei voltak. Ezeket a rendszer már nem használja.

## A `secrets.json` szerepe

A `KvizCommando.Server/secrets.json` helyi konfigurációs fájl. A szerver az általános `appsettings` fájlok után tölti be, ezért az itt megadott titkos értékek felülírják a repóban található üres helykitöltőket.

A fájl a `.gitignore` listáján szerepel, így Git nem követi. Ez azonban nem akadályozza meg, hogy kézi ZIP-készítéskor bekerüljön a csomagba. A bírálói csomagba kizárólag lecserélhető fejlesztői titkok kerülhetnek, és maga a csomag csak egyéni jogosultságú helyen adható át.

A jelenlegi szerkezet:

```json
{
  "Authentication": {
    "Facebook": {
      "AppId": "FEJLESZTOI_ERTEK",
      "AppSecret": "FEJLESZTOI_ERTEK"
    }
  },
  "AuditHash": {
    "Secret": "32_VELETLEN_BAJT_BASE64_FORMABAN"
  },
  "PiiEncryption": {
    "Key": "MASIK_32_VELETLEN_BAJT_BASE64_FORMABAN"
  },
  "Database": {
    "Provider": "Sqlite",
    "EnableRetryOnFailure": false
  },
  "ConnectionStrings": {
    "SqlServerApplication": "",
    "SqlServerGame": ""
  }
}
```

Az auditkulcs és a PII-kulcs két külön érték. Nem cserélhetők fel, mert eltérő adatot és eltérő műveletet védenek. Az SQL Server connection string szintén helyi vagy éles titok lehet, mert hitelesítési adatot tartalmazhat. SQLite használatakor a két SQL Server-érték maradhat üres.

## Facebook App ID és App Secret

Az `AppId` azonosítja a Facebook fejlesztői alkalmazást. Az `AppSecret` igazolja a Facebook felé, hogy a szerver ehhez az alkalmazáshoz tartozik.

Az App Secret akkor is titok, ha a felhasználói adatbázis üres. Fejlesztői módban, egyéni jogosultságú bírálói átadásnál használható lecserélhető tesztérték. Élesítéskor új éles hitelesítő adat szükséges, a fejlesztői értéket pedig vissza kell vonni vagy le kell cserélni.

## `AuditHash:Secret`

Az auditnapló nem tárol nyers IP-címet. Bekapcsolt `Audit:IncludeIpHash` esetén a szerver az IP-címből és az `AuditHash:Secret` értékből HMAC-SHA256 lenyomatot készít.

```text
IP-cím + AuditHash:Secret -> ipHash
```

Ugyanezt a kulcsot használja a feltételek elfogadásakor mentett IP- és User-Agent-lenyomat. A hash nem visszafejthető titkosítás.

Hiányzó vagy hibás Base64-kulcs esetén az auditálás folytatódik, de az IP-hash kimarad. A kulcs cseréje után ugyanaz az IP-cím más hasht ad, ezért a csere előtti és utáni lenyomatok nem hasonlíthatók össze közvetlenül.

## `PiiEncryption:Key`

Ez a kulcs védi a `UserPii` tábla telefonszám-, számlázásinév- és számlázásicím-mezőit.

A kulcs követelménye:

- pontosan 32 véletlen bájt;
- Base64-formátumban kerül a konfigurációba;
- nem kerül az adatbázisba vagy Gitbe;
- nem lehet azonos az auditkulccsal.

PowerShellben fejlesztői kulcs készíthető így:

```powershell
$bytes = New-Object byte[] 32
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
[Convert]::ToBase64String($bytes)
$rng.Dispose()
```

A program induláskor ellenőrzi a kulcs formátumát és hosszát. Hiányzó vagy hibás kulccsal nem indul el, mert ilyen állapotban a PII-adatok védelmét nem lehetne garantálni.

## Hogyan működik a PII-titkosítás?

A program AES-256-GCM hitelesített titkosítást használ. Minden mező minden titkosításakor új, véletlen 12 bájtos nonce készül. A titkosított tartalom mellett egy 16 bájtos hitelesítési tag is elmentésre kerül.

```text
számlázási cím + PII-kulcs + új nonce
    -> titkosított tartalom + nonce + hitelesítési tag
```

A nonce nem titok, ezért az adatbázisban tárolható. A hitelesítési tag gondoskodik arról, hogy a szerver észrevegye a titkosított tartalom módosítását vagy sérülését. Hibás tartalom, nonce, tag vagy kulcs esetén a visszafejtés nem ad vissza részleges vagy találgatott szöveget, hanem hibával leáll.

A titkosítás hitelesítési kontextusa a `UserId`-t és a mező nevét is tartalmazza. Emiatt egy érvényes titkosított telefonszám vagy számlázási cím nem másolható át észrevétlenül másik felhasználóhoz vagy másik mezőbe.

.NET 8 alatt a szolgáltatás a kötelező tagméretet megadó `AesGcm` konstruktort használja. A tagméret nélküli konstruktor .NET 8-tól elavult. Lásd: [Microsoft SYSLIB0053](https://learn.microsoft.com/en-us/dotnet/fundamentals/syslib-diagnostics/syslib0053).

Az AES-GCM a .NET 8 része, ezért ehhez nem szükséges külön NuGet-csomag.

## Mi történik a PII-kulcs elvesztésekor vagy cseréjekor?

Ha a kulcs elveszik, a már elmentett PII-adatok nem fejthetők vissza. Az adatbázis mentése önmagában nem elég: a titkos kulcsról külön, védett mentés szükséges.

A kulcs egyszerű lecserélése után a régi adatok szintén nem fejthetők vissza az új kulccsal. Kulcscsere előtt a meglévő adatokat a régi kulccsal vissza kell fejteni, majd az új kulccsal újra kell titkosítani. A jelenlegi egyszerű rendszer nem végez automatikus kulcsrotációt, ezért a kulcsot nem szabad kézzel lecserélni meglévő PII-adatok mellett.

Éles környezetben a kulcsot nem repóbeli JSON-fájlban, hanem a választott üzemeltetési környezet titoktárában kell tárolni. A Microsoft fejlesztéshez Secret Managert vagy helyi titkos konfigurációt, production környezetben pedig például felügyelt titoktárat javasol. Lásd: [Microsoft alkalmazástitok-kezelési útmutató](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/secure-net-microservices-web-applications/developer-app-secrets-storage).

## Data Protection XML-kulcsok

A `DataProtection-Keys` mappa XML-fájljait az ASP.NET Core hozza létre. Ezek védik többek között:

- a bejelentkezési cookie-kat;
- az e-mail-megerősítő tokeneket;
- a jelszó-visszaállító tokeneket;
- az Identity opaque access és refresh tokenjeit;
- a külső bejelentkezés rövid életű cookie-ját.

Ezek nem PII-titkosítási kulcsok, nem auditkulcsok és nem Facebook-titkok. Nem kerülnek a `secrets.json` fájlba vagy Gitbe.

A fejlesztői mappa törlése nem törli az adatbázist, de érvénytelenítheti a korábban kiadott cookie-kat, aktiváló linkeket, reset linkeket és opaque tokeneket. Éles környezetben a Data Protection-kulcskészletnek telepítéseken át megmaradó, hozzáférés-védett és biztonságosan mentett hely szükséges.

## Mit jelent itt a GDPR-megfelelőség?

A GDPR nem ír elő konkrét AES-algoritmust, peppert vagy kötelező mezőszintű titkosítást minden személyes adatra. A kockázathoz igazodó technikai és szervezési intézkedéseket követel. A titkosítás ennek fontos része lehet, de önmagában nem teszi a teljes rendszert megfelelővé.

A teljes folyamat része többek között:

- az adatkezelés céljának és jogalapjának meghatározása;
- csak a ténylegesen szükséges adatok gyűjtése;
- adatkezelési tájékoztató;
- hozzáférések korlátozása;
- HTTPS és biztonságos hitelesítés;
- megőrzési és törlési szabályok;
- adatexport, helyesbítés és törlés kezelése;
- a kötelezően megőrzendő számlázási adatok elkülönítése;
- védett biztonsági mentések;
- audit- és incidensnyilvántartás;
- a biztonsági intézkedések rendszeres ellenőrzése.

A GDPR 25. cikke az alapértelmezett adatvédelemről és adattakarékosságról, a 32. cikke pedig a kockázatarányos biztonságról rendelkezik. Lásd: [GDPR](https://eur-lex.europa.eu/eli/reg/2016/679/2016-05-04) és [EDPB biztonsági útmutató](https://www.edpb.europa.eu/sme/be-compliant/secure-personal-data_hu).

## A fejlesztői csomag átadása

A bírálói csomagban:

- csak lecserélhető fejlesztői kulcs lehet;
- éles kulcs nem lehet;
- a csomag nem kerülhet nyilvános letöltési helyre;
- a `GameUser.db` és a fejlesztői e-mail-fájlok átadás előtt ellenőrzendők;
- a Data Protection fejlesztői XML-kulcsait nem szükséges átadni;
- élesítés előtt a Facebook-, audit- és PII-kulcsot le kell cserélni.

## Adatbázis újrainicializálása

Az SQLite- és SQL Server-migrációk mostantól külön láncot alkotnak. A `GameUser.db` és az exportált kérdésadatok miatt már nélkülözhető `Game.db` is tisztán újra létrehozható. A pontos parancsok és a visszaimportálás sorrendje a `docs/database-providers-and-migrations.md` fájlban található.
