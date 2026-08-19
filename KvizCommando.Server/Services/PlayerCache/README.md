# Játékos-gyorsítótár

A PlayerCache egy szolgáltatás, de többféle fájlt használ. Az alapmappában ezért csak az interfész és a fő PlayerCacheService marad.

- A Models mappában vannak a gyorsítótár saját állapot- és eredménytípusai.
- A Persistence mappában van minden, ami a cache tartalmának adatbázisba mentésével és a háttérben futó ürítéssel foglalkozik.

Az almappák ellenére ezek a típusok továbbra is a KvizCommando.Server.Services.PlayerCache namespace részei. Nem két új szolgáltatást hoztunk létre, csak szétválasztottuk a cache állapotát a mentési folyamattól. Emiatt a meglévő kódnak nem kell új namespace-eket ismernie.

A korábbi PlayerCahceService.cs fájlnév elírás volt; a fájl neve most PlayerCacheService.cs. Az osztály neve és a működése eddig is helyes volt.

A QuestionStats nem cache-típus, hanem a kérdésadatbázis mentési eredménye. Ezért a Services/Db mappába került. A nyilvános tulajdonságneveket ebben a rendezésben nem változtattuk meg.
