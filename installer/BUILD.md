# Setup-EXE erstellen mit NSIS

## Voraussetzungen (einmalig)

NSIS herunterladen und installieren: https://nsis.sourceforge.io

## Setup-EXE bauen

**Schritt 1:** App veröffentlichen (Self-Contained)
- Rechtsklick auf DS.IHF → Veröffentlichen
- Ziel: Ordner → Release → Veröffentlichen
- Ausgabe: src\DS.IHF\bin\Release\net8.0-windows\win-x64\publish\DS.IHF.exe

**Schritt 2:** NSIS-Skript kompilieren
- Rechtsklick auf `installer\DS-IHF-Setup.nsi`
- **Compile NSIS Script** klicken

Die fertige Setup-EXE liegt danach unter:
```
installer\output\DS-IHF-Setup.exe
```

## Intune (Win32-App)

Installationsbefehl:   `DS-IHF-Setup.exe /S`
Deinstallationsbefehl: `%localappdata%\DS-IHF\Uninstall.exe /S`
Erkennungsregel:       Datei `%LOCALAPPDATA%\DS-IHF\DS.IHF.exe`
Zuweisung:             Per Benutzer (nicht Gerät)
Abhängigkeit:          .NET 8 Desktop Runtime


## Voraussetzungen (einmalig)

Inno Setup herunterladen und installieren:
https://jrsoftware.org/isdl.php

## Setup-EXE bauen

**Schritt 1:** App im Release-Modus bauen
- Visual Studio → Erstellen → Batchbuild
- Häkchen bei DS.IHF | Release | Any CPU
- Klick auf Erstellen

**Schritt 2:** Inno Setup öffnen
- `installer\DS-IHF-Setup.iss` mit Inno Setup öffnen
- Klick auf **Compile** (oder F9)

Die fertige Setup-EXE liegt danach unter:
```
installer\output\DS-IHF-Setup.exe
```

## Intune (Win32-App)

```powershell
# IntuneWin-Paket erstellen
IntuneWinAppUtil.exe -c installer\output -s DS-IHF-Setup.exe -o installer\output
```

Intune-Konfiguration:
- Installationsbefehl:   `DS-IHF-Setup.exe /VERYSILENT /SUPPRESSMSGBOXES`
- Deinstallationsbefehl: `%localappdata%\DS-IHF\unins000.exe /VERYSILENT`
- Erkennungsregel:       Datei `%LOCALAPPDATA%\DS-IHF\DS.IHF.exe`
- Zuweisung:             Per Benutzer (nicht Gerät)
- Abhängigkeit:          .NET 8 Desktop Runtime
