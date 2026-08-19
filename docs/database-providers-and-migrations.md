# Adatbázis-provider és migrációk

Ez a leírás azt foglalja össze, hogyan használja a Kviz Commando ugyanazt az adatmodellt SQLite és SQL Server alatt anélkül, hogy a két adatbázismotor migrációi összekeverednének.

## Melyik adatbázis mikor fut?

Az adatbázis-provider nem titok, ezért nem a `secrets.json` kapcsolja át. Egyetlen kapcsoló van a `Program.cs` fájl elején:

```csharp
const bool USE_SQL_SERVER = false;
```

- `false`: SQLite;
- `true`: SQL Server.

Ebben az állapotban az alkalmazás a következő két kapcsolatot használja:

```json
"ConnectionStrings": {
  "SqliteApplication": "Data Source=GameUser.db",
  "SqliteGame": "Data Source=Game.db"
}
```

Az SQL Server kódja és NuGet-csomagja ettől még a projekt része, de SQLite kiválasztásakor nem jön létre SQL Server-kapcsolat.

A váltáshoz csak ezt az egy `true`/`false` értéket kell átírni, mást nem. A `secrets.json` csak a helyi SQL Server-kapcsolati karakterláncokat tartalmazza. A migrációs parancsokban szereplő `--Database:Provider=...` csak az adott EF-parancs idejére választ providert; az sem ír át konfigurációs fájlt.

## SQL Server 2022 Express helyi beállítása

A Kviz Commando a már használt SQL Server 2022 Express kiadással fut. Nem kell mellé Developer kiadást telepíteni. Az Express ingyenesen használható fejlesztésre és kisebb éles rendszerhez is. Az SSMS csak kezelőprogram: a meglévő SSMS 21.6.17 használható hozzá, nem kell emiatt SSMS 22-re frissíteni.

### 1. Ellenőrzés: van-e már SQL Server motor?

Az SSMS megléte nem bizonyítja, hogy az adatbázismotor is telepítve van. Először próbálj csatlakozni az SSMS-ben:

- `Server type`: `Database Engine`;
- `Server name`: `localhost\SQLEXPRESS`;
- `Authentication`: `Windows Authentication`;
- `Encryption`: `Mandatory`;
- helyi fejlesztéshez jelöld be a `Trust server certificate` lehetőséget.

Ha nem kapcsolódik, a Windows Szolgáltatások között ellenőrizd, hogy fut-e a `SQL Server (SQLEXPRESS)` szolgáltatás. Ha ez megvan, a motor telepítve van, ezért a következő telepítési részt át kell ugrani.

### 2. SQL Server 2022 Express telepítése új gépen

Ezt csak olyan gépen kell elvégezni, amelyen még nincs `SQL Server (SQLEXPRESS)` szolgáltatás.

1. Nyisd meg a [Microsoft SQL Server letöltési oldalát](https://www.microsoft.com/en-us/sql-server/sql-server-downloads), és töltsd le a SQL Server 2022 Express kiadást.
2. Indítsd el a letöltött telepítőt rendszergazdaként.
3. Válaszd a `Basic` telepítést.
4. Fogadd el a licencfeltételeket; a telepítési hely maradhat az alapértelmezett.
5. Indítsd el a telepítést. Az elkészült named instance szokásos neve `SQLEXPRESS`.
6. A befejező képernyőn jegyezd fel az `Instance name` és a `Connection string` értékét.
7. Az SSMS-t nem kell újratelepíteni, ha a 21.6.17 már a gépen van.

A Microsoft telepítővarázslójának részletes háttere: [Install SQL Server from the Installation Wizard](https://learn.microsoft.com/en-us/sql/database-engine/install-windows/install-sql-server-from-the-installation-wizard-setup?view=sql-server-ver16).

### 3. Kapcsolódás SSMS 21.6.17-ből

Alapértelmezett instance esetén:

```text
Server type: Database Engine
Server name: localhost\SQLEXPRESS
Authentication: Windows Authentication
Encryption: Mandatory
Trust server certificate: bekapcsolva
```

Ha a telepítő befejező képernyője más instance-nevet mutatott, a `SQLEXPRESS` helyére azt kell írni.

Például `KVIZCOMMANDO` nevű instance esetén:

```text
localhost\KVIZCOMMANDO
```

A `Trust server certificate` itt csak a helyi fejlesztői, saját aláírású tanúsítvány miatt szerepel. Éles szervernél megbízható TLS-tanúsítványt kell használni.

Kapcsolódás után nyiss egy új lekérdezést, és futtasd:

```sql
SELECT
    SERVERPROPERTY('ProductVersion') AS ProductVersion,
    SERVERPROPERTY('Edition') AS Edition;
```

SQL Server 2022 esetén a `ProductVersion` főszáma `16`; az `Edition` eredményben az Express kiadásnak kell megjelennie.

### Express éles használata

Az Express élesben is használható. SQL Server 2022 alatt a fő korlátai adatbázisonként 10 GB tárhely, legfeljebb 4 processzormag és körülbelül 1,4 GB buffer pool memória; SQL Server Agentet nem tartalmaz. A két Kviz Commando-adatbázis külön-külön kapja a 10 GB-os határt.

Kezdeti éles használatra ez megfelelő. Ha a mért terhelés vagy valamelyik adatbázis mérete később kinövi, SQL Server Standardra lehet váltani. A program providerkapcsolója ekkor is SQL Server marad; csak a connection stringben szereplő szerverpéldány változik.

A Microsoft aktuális kiadás- és korlátlistája: [SQL Server 2022 editions and supported features](https://learn.microsoft.com/en-us/sql/sql-server/editions-and-components-of-sql-server-2022?view=sql-server-ver16).

### 4. A Kviz Commando helyi kapcsolatai

Az SQL Serveren két külön adatbázist használunk:

```text
KvizCommandoApplication
KvizCommandoGame
```

Windows-hitelesítés és alapértelmezett helyi SQL Server-instance esetén a `secrets.json` vonatkozó része kizárólag ez:

```json
{
  "ConnectionStrings": {
    "SqlServerApplication": "Server=localhost\\SQLEXPRESS;Database=KvizCommandoApplication;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True",
    "SqlServerGame": "Server=localhost\\SQLEXPRESS;Database=KvizCommandoGame;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

Ha a telepített instance neve nem `SQLEXPRESS`, mindkét connection stringben azt a nevet kell használni. Például:

```text
Server=localhost\KVIZCOMMANDO
```

Ezután állítsd a `Program.cs` elején lévő `USE_SQL_SERVER` kapcsolót `true` értékre. A géphez vagy hitelesítéshez kötött connection string a secretsben marad.

SQL Server-felhasználónév és jelszó használatakor azok kizárólag a `secrets.json` fájlba vagy az éles környezet titoktárába kerülhetnek. Repóbeli `appsettings` fájlba nem írhatók. Helyi fejlesztéshez egyszerűbb a fenti Windows-hitelesítés.

Az alkalmazás nem készít adatbázist és nem futtat migrációt automatikusan induláskor. A séma frissítése mindig külön, tudatos művelet.

## Miért van négy migrációs lánc?

Két adatbázismotor és két `DbContext` van:

```text
Data/Migrations/
├── Sqlite/
│   ├── Application/
│   └── Game/
└── SqlServer/
    ├── Application/
    └── Game/
```

Az Application adatbázis tartalmazza az Identity-, játékos-, hozzájárulási, PII- és fizetési táblákat. A Game adatbázis kizárólag a kérdéstáblákat tartalmazza.

A négy migrációs context csak az EF eszközeinek választási pontja:

- `SqliteApplicationDbContext`;
- `SqliteGameDbContext`;
- `SqlServerApplicationDbContext`;
- `SqlServerGameDbContext`.

A normál alkalmazáskód továbbra is az `ApplicationDbContext` és `GameDbContext` típusokat használja.

## Első migrációk létrehozása

A Visual Studio Package Manager Console-ban a Default project legyen `KvizCommando.Server`.

SQLite Application:

```powershell
Add-Migration InitialSqliteApplication -Context SqliteApplicationDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -OutputDir Data/Migrations/Sqlite/Application -Args "--Database:Provider=Sqlite"
```

SQLite Game:

```powershell
Add-Migration InitialSqliteGame -Context SqliteGameDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -OutputDir Data/Migrations/Sqlite/Game -Args "--Database:Provider=Sqlite"
```

SQL Server Application:

```powershell
Add-Migration InitialSqlServerApplication -Context SqlServerApplicationDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -OutputDir Data/Migrations/SqlServer/Application -Args "--Database:Provider=SqlServer"
```

SQL Server Game:

```powershell
Add-Migration InitialSqlServerGame -Context SqlServerGameDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -OutputDir Data/Migrations/SqlServer/Game -Args "--Database:Provider=SqlServer"
```

Az `Add-Migration` parancsot ebben a projektben nem futtatjuk `-Context` nélkül.

## SQLite felhasználói adatbázis létrehozása

Ha nincs megtartandó felhasználói adat, leállított szerver mellett törölhető:

```text
KvizCommando.Server/GameUser.db
KvizCommando.Server/GameUser.db-shm
KvizCommando.Server/GameUser.db-wal
```

Ezután:

```powershell
Update-Database -Context SqliteApplicationDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -Args "--Database:Provider=Sqlite"
```

## SQLite kérdésadatbázis létrehozása

A kérdések korábbi tartalma exportálva van, ezért a régi `Game.db` adatbázist nem kell baseline-olni. Leállított szerver mellett törölhető:

```text
KvizCommando.Server/Game.db
KvizCommando.Server/Game.db-shm
KvizCommando.Server/Game.db-wal
```

Ezután az új adatbázis tisztán létrehozható:

```powershell
Update-Database -Context SqliteGameDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -Args "--Database:Provider=Sqlite"
```

A kérdésadatok csak az új séma létrejötte után importálhatók vissza. Kézi `__EFMigrationsHistory`-módosításra, baseline-ra vagy más kerülőútra nincs szükség.

## SQL Server adatbázisok létrehozása

A `secrets.json` connection stringjei mellett nem szükséges kézzel létrehozni a két üres adatbázist. A migráció létrehozza őket, ha a Windows-felhasználód megfelelő jogosultságot kapott:

```powershell
Update-Database -Context SqlServerApplicationDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -Args "--Database:Provider=SqlServer"
```

```powershell
Update-Database -Context SqlServerGameDbContext -Project KvizCommando.Server -StartupProject KvizCommando.Server -Args "--Database:Provider=SqlServer"
```

A futtató Windows-felhasználónak adatbázis-létrehozási jogosultság kell. Ha ezt nem kapja meg, az üres adatbázisokat előre létre lehet hozni SSMS-ben, majd elegendő tulajdonosi jogosultságot adni a migrációkhoz.

## Későbbi modellváltozások

Ha egy Application-entitás változik, ugyanazzal a beszédes névvel készül egy SQLite- és egy SQL Server-migráció. Ha egy kérdésentitás változik, ugyanez a két Game contexttel történik.

Egy provider migrációját soha nem másoljuk át kézzel a másik provider mappájába. Mindkettőt a saját aktív providerével generáljuk, mert az adattípusok, JSON-ellenőrzések és számított oszlopok SQL-je eltér.

## Iskolai SQLite-ellenőrzés

Az átadható alapkonfigurációban a `USE_SQL_SERVER` értéke továbbra is `false`. Az ellenőrzés menete:

1. üres ideiglenes Application és Game SQLite-fájl;
2. a két SQLite migráció alkalmazása;
3. teljes solution build;
4. szerverindítás SQL Server nélkül;
5. regisztráció, belépés és check-in;
6. játékosadatok és kérdések lekérése;
7. annak ellenőrzése, hogy nem történt SQL Server-kapcsolódási kísérlet.

Az SQL Server kipróbálása ettől külön ellenőrzés, és nem feltétele az SQLite-os iskolai build elindításának.

## Hivatalos háttér

Az EF Core több provider esetén külön migrációkészletet javasol minden providerhez: [Migrations with Multiple Providers](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers).

A SQL Server 2022 általános támogatása 2028. január 11-ig, kiterjesztett támogatása 2033. január 11-ig tart: [SQL Server 2022 lifecycle](https://learn.microsoft.com/en-us/lifecycle/products/sql-server-2022).
