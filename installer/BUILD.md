# MSI erstellen mit WixSharp

## Voraussetzungen (einmalig)

1. **WixSharp Project Templates** in Visual Studio installieren
   (Extensions → Manage Extensions → suche "WixSharp")

2. **Wix Toolset** wird automatisch als NuGet-Paket geladen — kein separates Tool nötig

## MSI bauen

**Schritt 1:** App im Release-Modus bauen
```
Visual Studio → Build → Batch Build → DS.IHF Release → Build
```

**Schritt 2:** Installer ausführen
```
Visual Studio → DS-IHF-Installer → Start (ohne Debugging)
```
oder per Kommandozeile:
```powershell
dotnet run --project installer\DS-IHF-Installer.csproj
```

Die fertige MSI liegt danach unter:
```
installer\output\DS-IHF-Setup.msi
```

## Intune (Win32-App)

```powershell
# IntuneWin-Paket erstellen
IntuneWinAppUtil.exe -c installer\output -s DS-IHF-Setup.msi -o installer\output
```

Intune-Konfiguration:
- Installationsbefehl:   `msiexec /i DS-IHF-Setup.msi /qn`
- Deinstallationsbefehl: `msiexec /x {B7C3D4E5-F6A7-8901-BCDE-F12345678901} /qn`
- Erkennungsregel:       Datei `%LOCALAPPDATA%\DS-IHF\DS.IHF.exe`
- Zuweisung:             Per Benutzer (nicht Gerät)
- Abhängigkeit:          .NET 8 Desktop Runtime
