# Kliensoldali képernyőállapotok

Ebben a mappában marad az alkalmazás közös állapota és az aktuális munkamenet adata. A Home, Question, Team, Solo és VsGame almappák az egyes képernyőkhöz tartozó állapotinterfészt és annak megvalósítását tartják együtt.

Az almappák itt a fájlok gyors megtalálását szolgálják. Nem külön cache-rétegek, ezért minden bennük lévő típus szándékosan megtartja ezt a namespace-t:

    KvizCommando.Client.Services.ClientCache

Így a MainLayout, a dependency injection regisztráció és a feature-ök ugyanazokat a típusokat használják, mint korábban. A mappabontás önmagában nem változtatja meg az állapotok élettartamát, betöltését vagy érvénytelenítését.

Külön namespace akkor lenne indokolt, ha valamelyik állapot saját, önálló cache-modullá válna. Jelenleg mind az öt ugyanannak a kliensoldali állapotkezelésnek a része.
