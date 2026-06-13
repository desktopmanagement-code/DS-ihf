# MSI erstellen

## Voraussetzungen (einmalig)

```powershell
# WiX 4 installieren
dotnet tool install --global wix

# WiX UI Extension hinzufügen
wix extension add WixToolset.UI.wixext/4.0.5
```

Außerdem: **HeatWave for VS2022** Extension in Visual Studio installieren.

## MSI bauen

```powershell
# 1. App im Release-Modus bauen
dotnet build ..\src\DS.IHF\DS.IHF.csproj -c Release

# 2. MSI erstellen
dotnet build DS-IHF-Installer.wixproj -c Release
```

Die fertige MSI liegt danach unter:
```
installer\bin\Release\DS-IHF-Setup.msi
```

## Intune (Win32-App)

```powershell
# IntuneWin-Paket erstellen (IntuneWinAppUtil.exe benötigt)
IntuneWinAppUtil.exe -c . -s DS-IHF-Setup.msi -o .
```

Intune-Konfiguration:
- Installationsbefehl:   `msiexec /i DS-IHF-Setup.msi /qn`
- Deinstallationsbefehl: `msiexec /x {B7C3D4E5-F6A7-8901-BCDE-F12345678901} /qn`
- Erkennungsregel:       Datei `%LOCALAPPDATA%\DS-IHF\DS.IHF.exe`
- Zuweisung:             Per Benutzer (nicht Gerät)
- Abhängigkeit:          .NET 8 Desktop Runtime
