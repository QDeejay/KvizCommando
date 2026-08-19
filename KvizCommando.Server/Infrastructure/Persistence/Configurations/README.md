# Entity Framework konfigurációk

Itt találhatók az adatbázis-táblák leképezései. Az almappák azt mutatják meg, melyik adatkörhöz tartozik egy konfiguráció:

- Account: felhasználói fiók, hozzájárulások, PII és fizetési adatok;
- Players: játékos, karakterek, felszerelés és statisztikák;
- Questions: saját és ellenőrzésre váró kérdések.

Ez kizárólag fizikai rendezés. Minden osztály megtartotta a KvizCommando.Server.Infrastructure.Persistence.Configurations namespace-t. Az ApplicationDbContext továbbra is assembly alapján találja meg a konfigurációkat, a GameDbContext pedig ugyanazokat a kérdéskonfigurációkat példányosítja, mint eddig.

Az almappa tehát nem módosít táblát, kulcsot, kapcsolatot vagy migrációt. Ha később az account- és a játékadatok külön projektbe vagy külön perzisztencia-modulba kerülnének, akkor az önálló namespace már valódi határt fejezne ki. Jelenleg csak zavarná a hivatkozásokat.
