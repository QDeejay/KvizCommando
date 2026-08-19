# A Kviz Commando titkos beállításai

Ez a leírás azt foglalja össze, hogy a Kviz Commando fejlesztői környezetében melyik titkos érték mire való. Nem általános kriptográfiai dokumentáció: a jelenlegi program konkrét működését írja le.

## A `secrets.json` szerepe

A `KvizCommando.Server/secrets.json` olyan helyi beállításokat tartalmaz, amelyeket nem akarunk az `appsettings.json` fájlba és a Git repóba tenni. A szerver induláskor az általános beállítások után tölti be ezt a fájlt, ezért az itt megadott értékek felülírják az `appsettings` azonos nevű, üres vagy helykitöltő értékeit.

A fájl szándékosan szerepel a `.gitignore` listáján. Ettől egy kézzel készített ZIP-be még bekerülhet, ezért átadás előtt mindig tudni kell, hogy a csomag fejlesztői vagy éles titkokat tartalmaz-e.

A jelenlegi fejlesztői fájl szerkezete:

```json
{
  "Authentication": {
    "Facebook": {
      "AppId": "FEJLESZTOI_ERTEK",
      "AppSecret": "FEJLESZTOI_ERTEK"
    }
  },
  "AuditHash": {
    "Secret": "KULON_GENERALT_BASE64_KULCS"
  },
  "Security": {
    "EmailHashPepper": "MASIK_KULON_GENERALT_KULCS"
  }
}
```

A példában nincsenek valódi kulcsok. A Facebook titka, az auditkulcs és az e-mail pepper három külön érték.

## Facebook App ID és App Secret

Az `AppId` azonosítja a Facebook fejlesztői alkalmazást. Az `AppSecret` bizonyítja a Facebook felé, hogy a szerver ehhez az alkalmazáshoz tartozik. A Facebook-bejelentkezés ezek nélkül nem indul el.

Az App Secret nem a felhasználói adatbázist titkosítja. Akkor is titoknak számít, ha a `GameUser.db` üres. A jelenlegi bírálói csomagban fejlesztői érték használható, ha a csomag egyéni jogosultságú mappába kerül, a Facebook-alkalmazás továbbra is fejlesztői módban marad, és a bíráló nincs felvéve tesztelőnek. Ebben az esetben a bíráló látja a konfigurációt, de a saját Facebook-fiókjával nem tud belépni.

Élesítéskor új éles hitelesítő adatokat kell használni, a fejlesztői értékeket pedig vissza kell vonni vagy le kell cserélni.

## `AuditHash:Secret`

Az auditnapló nem tárol nyers IP-címet. Ha az `Audit:IncludeIpHash` be van kapcsolva, a szerver az IP-címből és az `AuditHash:Secret` kulcsból HMAC-SHA256 lenyomatot készít.

Egyszerűsítve:

```text
IP-cím + AuditHash:Secret -> ipHash
```

Az auditfájlba csak az `ipHash` kerül. A kulcs hiányában vagy hibás Base64-formátum esetén az auditálás nem áll le, de az `ipHash` értéke kimarad, és a szerver figyelmeztetést ír a normál naplóba.

Ugyanezt a kulcsot használja a feltételek elfogadásakor mentett IP- és User-Agent-lenyomat is. Ez nem PII-titkosítás, és a hashből nem lehet visszafejteni az eredeti IP-címet vagy User-Agentet.

A kulcsot működés közben nem célszerű lecserélni. Kulcscsere után ugyanaz az IP-cím más lenyomatot ad, ezért a csere előtti és utáni auditbejegyzések ezen az alapon már nem kapcsolhatók össze. A régi napló nem sérül meg, csak ez az összehasonlíthatóság vész el.

## `Security:EmailHashPepper`

Az e-mail pepper a későbbi PII-rendszer keresési hashéhez tartozik. Feladata, hogy az e-mail-címből ne egyszerű, előre kiszámítható SHA-256 hash készüljön.

Egyszerűsítve:

```text
normalizált e-mail + EmailHashPepper -> EmailNormHash
```

Az `EmailLookup` már beolvassa ezt a beállítást és képes elkészíteni a hasht. A teljes `UserPii` adatfolyam azonban még nincs élesítve. A jelenlegi regisztráció nem készít `UserPii` rekordot, és nem ír `EmailNormHash` értéket az adatbázisba. A pepper most konfigurálva és használatra kész, de önmagában nem kapcsolja be a PII-tárolást.

Ez a pepper nem titkosítja az e-mail-címet, és nem azonos a személyes adatokat később védő valódi titkosítási kulccsal. A jelenlegi `DummyEncryptionProvider` csak fejlesztői helykitöltő, nem valódi titkosítás. A teljes mezőszintű PII-védelem külön GDPR-fejlesztési csomag feladata.

Ha a peppert az `EmailNormHash` értékek használata után lecseréljük, ugyanaz az e-mail más hasht kap. Ilyenkor a korábbi rekordokat újra kellene számolni. Ezért az éles PII-rendszer bekapcsolása előtt végleges, tartós pepper szükséges.

## Miért külön kulcs az audit secret és az e-mail pepper?

A két kulcs eltérő célhoz tartozik:

- az `AuditHash:Secret` az IP- és User-Agent-lenyomatokat védi;
- az `EmailHashPepper` az e-mail keresési hasht védi.

Ha ugyanazt a kulcsot használnánk minden célra, az egyik rendszer kulcscseréje vagy kompromittálódása a másik rendszert is érintené. A külön kulcsokkal az audit és a PII-rendszer egymástól függetlenül kezelhető.

## Data Protection XML-kulcsok

A `DataProtection-Keys` mappában található XML-fájlokat nem mi írjuk kézzel. Az ASP.NET Core automatikusan hozza létre és kezeli őket.

Ezekkel védi többek között:

- a bejelentkezési cookie-kat;
- az e-mail-megerősítő tokeneket;
- a jelszó-visszaállító tokeneket;
- az ASP.NET Core Identity opaque access és refresh tokenjeit;
- a külső bejelentkezés rövid életű cookie-ját.

Ezek a kulcsok nem JWT-kulcsok, nem Facebook-titkok, és nem az adatbázis személyes mezőinek titkosító kulcsai.

A kulcskészlet azért marad meg a szerver újraindítása után, hogy a korábban kiadott cookie-k és tokenek továbbra is ellenőrizhetők legyenek. Ha a fejlesztői kulcsmappát töröljük, a szerver új kulcsot készít. Az adatbázis nem sérül, de a korábbi cookie-k, megerősítő linkek, jelszó-visszaállító linkek és opaque tokenek érvénytelenné válhatnak.

Az XML-fájlok nem kerülnek a `secrets.json` fájlba és nem kerülnek Gitbe. A `.gitignore` külön kizárja a `DataProtection-Keys` mappákat.

Fejlesztésben a helyi fájlos tárolás megfelelő. Éles környezetben a kulcskészletnek telepítéseken át megmaradó, hozzáférés-védett hely kell, és külön gondoskodni kell a fájlban lévő kulcsanyag nyugalmi állapotú védelméről. Ennek végleges módja az éles üzemeltetési környezettől függ, ezért a jelenlegi csomag nem változtatja meg a Data Protection beállítását.

## Fejlesztői csomag átadása

A jelenlegi bírálói csomag üres felhasználói adatbázissal és lecserélhető fejlesztői kulcsokkal készül. Egyéni hozzáférésű mappában a fejlesztői `secrets.json` is átadható, ha a bírálónak a konfigurációt is látnia kell.

Az átadás szabályai:

- a csomag ne kerüljön nyilvános letöltési helyre;
- éles kulcs soha ne kerüljön bele;
- élesítés előtt a fejlesztői Facebook-adatokat, auditkulcsot és e-mail peppert le kell cserélni;
- a Data Protection fejlesztői XML-fájljait nem szükséges átadni, mert a bíráló gépe saját kulcskészletet készít;
- a fejlesztői e-mail-mappában lévő leveleket átadás előtt ellenőrizni vagy törölni kell, mert egyszer használatos linkeket és címzettet tartalmazhatnak.

## Jelenlegi állapot röviden

- A böngészős bejelentkezés cookie-t használ.
- A külön tokenes útvonal ASP.NET Core Identity opaque tokent használ, nem JWT-t.
- Az audit IP-hash működik, ha érvényes `AuditHash:Secret` érkezik a helyi konfigurációból.
- Az e-mail pepper be van kötve az `EmailLookup` szolgáltatásba, de a UserPII adatfolyam még nincs bekapcsolva.
- A Data Protection fájlos kulcskészlete működik, de production környezetben külön végleges tárolási és kulcsvédelmi döntés szükséges.
