# Szerveroldali szolgáltatásregisztrációk

Ebben a mappában áll össze, hogy induláskor milyen szolgáltatásokat kap meg a szerver. Korábban minden regisztrációs fájl egymás mellett volt, ezért egy idő után nehéz volt megmondani, melyik beállítás melyik területhez tartozik.

Az almappák egyszerűen a keresést segítik:

- Identity: bejelentkezés, Identity és az alkalmazás hitelesítési beállításai;
- Security: Data Protection, korlátozások és a biztonsági szolgáltatások;
- Persistence: adatbázisok és háttérben futó mentések;
- Gameplay: a játék működéséhez tartozó szolgáltatások;
- Web: CORS, lokalizáció, hibaválaszok és a webes kiszolgálás.

A fájlok továbbra is a KvizCommando.Server.Startup namespace-ben vannak. Ez tudatos: az almappák nem külön indítási rétegek, csak rendet tartanak a fájlok között. Emiatt a Program.cs hívásai sem változtak meg.

Új namespace akkor indokolt, ha valamelyik csoport saját, kívülről is elkülönülő indítási API-t kap. Addig a közös namespace egyszerűbb, és nem termel felesleges using sorokat.
