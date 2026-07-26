# BWIN2 – standalone Blazor WebAssembly

A QuickBASIC játék .NET 8-as, kliensoldali átirata. A futó alkalmazás nem használ
szerveroldali játékkódot, adatbázist vagy API-t: a játékmag, a meccsszámítás és
a felület a böngészőben, WebAssemblyben fut. A statikus `game-data.json` az
eredeti csapat-, játékos-, sorsolás- és kommentáradatokat tartalmazza.

## Indítás Visual Studióból

1. Nyisd meg a `BWin2.Wasm.sln` fájlt Visual Studio 2022-ben.
2. A `BWin2.Wasm` legyen a startup project.
3. Indítsd `F5`-tel vagy `Ctrl+F5`-tel.

A projekt .NET 8 SDK-t igényel. A fejlesztői kiszolgáló csak a statikus WASM
fájlokat adja a böngészőnek; a játékhoz nincs backend.

Parancssorból:

```text
dotnet run --project BWin2.Wasm
```

Release build:

```text
dotnet publish BWin2.Wasm -c Release
```

A publikált `wwwroot` bármilyen statikus tárhelyre kitehető.

## Megőrzött működés

- szezon eleji bajnok- és kupagyőztes-fogadás;
- Fixtures billentyűkezelés: első `↓` a fordulóra, második `↓` az oddsokra;
- mérkőzésenként és kimenetelenként bejárható oddsok;
- bajnokságban az aktuális forduló és további öt forduló fogadható;
- a fogadáskori és az aktuális odds különbségének kijelzése;
- fogadott mérkőzések választható élő megjelenítése;
- 600 ms / játékperc, 1000 ms / kommentárrész;
- eredeti eredményszámítás, kommentárszótár és kommentárscriptek;
- tabella, keretek, góllövőlista, fogadási összesítő, kupa és új szezon;
- a kiválasztott bajnokcsapat villogó kiemelése a tabellán.

## Felépítés

- `Components` – Blazor képernyők, minden komponens külön `.razor` és
  `.razor.cs` fájlban;
- `State` – alkalmazás- és élőmeccs-állapot;
- `Services` – interfészek mögötti fogadás, odds, sorsolás, tabella,
  kommentár és meccsmotor;
- `Domain` – játékmodellek;
- `Data` – JSON betöltése és ellenőrzése;
- `wwwroot/data/game-data.json` – az eredeti játékadatok.

Az alkalmazás szándékosan nem ment böngészőn kívülre: újratöltéskor új játék
indul.
