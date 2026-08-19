# Entity Framework konfigurációk

Itt találhatók az adatbázis-táblák leképezései. Az almappák azt mutatják meg, melyik adatkörhöz tartozik egy konfiguráció:

- Account: felhasználói fiók, hozzájárulások, PII és fizetési adatok;
- Players: játékos, karakterek, felszerelés és statisztikák;
- Questions: saját és ellenőrzésre váró kérdések.

Ez kizárólag fizikai rendezés. Minden osztály megtartotta a KvizCommando.Server.Infrastructure.Persistence.Configurations namespace-t. Az ApplicationDbContext név szerint alkalmazza az Account és Players konfigurációkat. A GameDbContext ugyanígy közvetlenül alkalmazza a két kérdéskonfigurációt, ezért a kérdéstáblák nem kerülhetnek véletlenül a fiókadatbázis modelljébe.

A `SqliteModelConfiguration` és `SqlServerModelConfiguration` csak a tényleges adatbázis-provider különbségeket tartalmazza: a JSON-ellenőrzéseket, számított oszlopokat, hashhossz-korlátokat és a rowversion viselkedését. A közös kulcsok, indexek és kapcsolatok továbbra is az entitások saját konfigurációs fájljaiban maradnak.

A providerbontás ott változtatja meg a fizikai sémát, ahol SQLite és SQL Server eltérő SQL-t igényel, de nem hoz létre második üzleti adatmodellt.
