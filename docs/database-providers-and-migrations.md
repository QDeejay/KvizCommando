# Adatbázis-provider és migrációk

Ez a leírás azt foglalja össze, hogyan használja a Kviz Commando ugyanazt az adatmodellt SQLite és SQL Server alatt anélkül, hogy a két adatbázismotor migrációi összekeverednének.

## Melyik adatbázis mikor fut?

Az alapértelmezett provider SQLite:

```json
"Database": {
  "Provider": "Sqlite",
  "EnableRetryOnFailure": false
}
```

Ebben az állapotban az alkalmazás a következő két kapcsolatot használja:

```json
"ConnectionStrings": {
  "SqliteApplication": "Data Source=GameUser.db",
  "SqliteGame": "Data Source=Game.db"
}
```

Az SQL Server kódja és NuGet-csomagja ettől még a projekt része, de SQLite kiválasztásakor nem jön létre SQL Server-kapcsolat.

## SQL Server 2022 helyi beállítása

Fejlesztéshez a SQL Server 2022 Developer kiadás megfelelő. Az SSMS csak kezelőprogram: a meglévő SSMS 21.6.17 használható hozzá, nem kell emiatt SSMS 22-re frissíteni.

Az SSMS telepítése önmagában nem jelenti azt, hogy az SQL Server motor is telepítve van. Kapcsolódás után ezzel lehet ellenőrizni a tényleges szerververziót:

```sql
SELECT
    SERVERPROPERTY('ProductVersion') AS ProductVersion,
    SERVERPROPERTY('Edition') AS Edition;
```

SQL Server 2022 esetén a termékverzió főszáma `16`.

Az SQL Serveren két külön adatbázist használunk:

```text
KvizCommandoApplication
KvizCommandoGame
```

Windows-hitelesítés és alapértelmezett helyi SQL Server-példány esetén a `secrets.json` vonatkozó része:

```json
{
  "Database": {
    "Provider": "SqlServer",
    "EnableRetryOnFailure": true
  },
  "ConnectionStrings": {
    "SqlServerApplication": "Server=localhost;Database=KvizCommandoApplication;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True",
    "SqlServerGame": "Server=localhost;Database=KvizCommandoGame;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

Ha a telepített példány neve például `SQLEXPRESS`, a szerver neve:

```text
Server=localhost\SQLEXPRESS
```

SQL Server-felhasználónév és jelszó használatakor azok kizárólag a `secrets.json` fájlba vagy az éles környezet titoktárába kerülhetnek. Repóbeli `appsettings` fájlba nem írhatók.

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

A `secrets.json` SQL Server-beállításai mellett:

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

Az átadható alapkonfiguráció továbbra is `Provider: Sqlite`. Az ellenőrzés menete:

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
