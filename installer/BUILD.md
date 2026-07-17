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
installer\output\DS-Versand-Setup.exe
```

## Intune (Win32-App)

Installationsbefehl:   `DS-Versand-Setup.exe /S`
Deinstallationsbefehl: `%programfiles%\DS-Versand\Uninstall.exe /S`
Erkennungsregel:       Datei `%programfiles%\DS-Versand\DS.IHF.exe`
Zuweisung:             Per Gerät (admin-Installation)
Abhängigkeit:          keine (.NET ist selbst enthalten)
