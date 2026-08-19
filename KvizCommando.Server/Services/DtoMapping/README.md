# A képernyőadatokat összeállító szolgáltatások

Ez a mappa három, önmagában már túl hosszú szolgáltatást fog össze. A Team, a Question és a Screen almappa nem új réteget jelent: azért vannak, hogy az egymáshoz tartozó interfészeket és részfájlokat ne egy hosszú, vegyes listában kelljen keresni.

A szolgáltatások partial osztályok. Egy részfájl egy jól felismerhető feladatot tartalmaz. A TeamService.ManageTeam.cs például a csapattag kezelését, a ScreenService.Home.cs pedig a kezdőképernyő adatainak összeállítását. A mezők és a konstruktor az osztály nevével megegyező alapfájlban maradnak. Közös segédmetódus csak akkor kerüljön oda, ha valóban több rész használja; egy adott képernyőhöz tartozó segéd maradjon az adott részfájlban.

Az almappákban szándékosan megmarad a KvizCommando.Server.Services.DtoMapping namespace.

Ennek gyakorlati oka van. Most csak a fájlokat rendeztük át, nem a program felépítését vagy a szolgáltatások nyilvános helyét változtattuk meg. Így a meglévő hivatkozásokhoz és a dependency injection regisztrációhoz nem kell hozzányúlni.

A namespace-t akkor lenne érdemes az almappákhoz igazítani, ha a Team, Question vagy Screen később önálló modul lenne saját belső típusaival és világos határával. Egy pusztán navigációs mappa ehhez még nem elég ok.
