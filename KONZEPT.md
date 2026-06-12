# DS-IHF C# Migrationkonzept
## Serienbriefe automatisch an DocuSign senden

---

## 1. Ausgangslage (PowerShell)

| Skript | Aufgabe |
|---|---|
| `IHFlauncherv2.ps1` | Einstiegspunkt: liest settings.json, holt JWT-Token, startet SendIHF |
| `OAuth/jwt.ps1` | JWT-Grant: RSA-signiertes JWT → Access Token + Account ID |
| `OAuth/code_grant.ps1` | Authorization Code Grant (nur für Ersteinrichtung/Consent) |
| `SendIHFv3.ps1` | Ordner-Dialog, öffnet alle .docx via Word-COM, liest DSEMAIL/DSASP/DSVA aus dem Text, sendet Envelope an DocuSign eSignature API |
| `config/settings.json` | Pro-User-Konfiguration: IntegrationKey, ImpersonationGUID, AccountId |
| `config/private.key` | RSA Private Key für JWT-Signierung |

**Ablauf heute:**
1. User erstellt Serienbriefe in Word → .docx-Dateien mit eingebetteten Tags (`DSEMAIL:`, `DSASP:`, `DSVA:`)
2. User startet `IHFlauncherv2.ps1`
3. Script holt JWT-Token (RSA-signiert, 1h gültig)
4. Script öffnet Ordner-Dialog, iteriert alle .docx
5. Jedes Dokument wird per Multipart-POST an DocuSign gesendet

---

## 2. C#-Zielarchitektur

```
DS-IHF.sln
├── src/
│   └── DS.IHF/                        ← Hauptanwendung (Console/WinForms)
│       ├── Program.cs                  ← Einstiegspunkt
│       ├── Services/
│       │   ├── DocuSignAuthService.cs  ← JWT-Token holen (ersetzt jwt.ps1)
│       │   └── DocuSignSendService.cs  ← Envelope senden (ersetzt SendIHFv3.ps1)
│       ├── Models/
│       │   ├── AppSettings.cs          ← Typstarkes Abbild von settings.json
│       │   └── DocumentMetadata.cs     ← DSEMAIL, DSASP, DSVA aus docx
│       ├── Helpers/
│       │   └── WordDocumentReader.cs   ← Text aus .docx extrahieren (ohne Word-COM)
│       └── config/
│           └── settings.json           ← Pro-User-Konfiguration (wird nicht überschrieben)
└── tests/
    └── DS.IHF.Tests/
        └── DocuSignAuthServiceTests.cs
```

---

## 3. Wichtige Designentscheidungen

### 3.1 JWT-Auth in C# (kein NuGet-Hack mehr)
Das PS1-Skript lädt `DerConverter` und `PemUtils` über NuGet zur Laufzeit.
In C# verwenden wir **`System.Security.Cryptography.RSA`** (built-in .NET 8) direkt — kein externer NuGet nötig.

```
private.key  →  RSA.ImportFromPem()  →  JWT signieren  →  POST /oauth/token  →  Access Token
```

### 3.2 Word-Dokument lesen ohne Word-COM
Das PS1-Skript öffnet Word als COM-Objekt (langsam, braucht Word installiert).
In C# verwenden wir **`DocumentFormat.OpenXml`** (NuGet: `DocumentFormat.OpenXml`) — liest .docx direkt, ohne Word.

### 3.3 settings.json — pro User editierbar
- Wird bei Installation nach `%APPDATA%\DS-IHF\config\settings.json` kopiert
- `private.key` ebenfalls nach `%APPDATA%\DS-IHF\config\private.key`
- App liest immer aus `%APPDATA%\DS-IHF\config\` → jeder User hat seine eigene Konfiguration
- Admins können eine Vorlage (`settings.template.json`) über Intune/MSI vorbelegen

### 3.4 Einmaliger Consent (ersetzt ConfirmationProd.txt)
- Beim ersten Start: Browser öffnen für DocuSign-Consent (wie heute)
- Danach: Marker-Datei in `%APPDATA%\DS-IHF\config\consent.done` → kein Browser mehr
- Token wird **nicht** auf Disk gespeichert — wird im Arbeitsspeicher gehalten (1h gültig), bei Bedarf neu geholt

---

## 4. settings.json (C#-Version)

```json
{
  "INTEGRATION_KEY_JWT": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "IMPERSONATION_USER_GUID": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "ACCOUNT_ID": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "CC_EMAIL": "kopie@firma.de",
  "CC_NAME": "Kopie Empfänger",
  "DOCUSIGN_BASE_URI": "https://eu.docusign.net/restapi",
  "DOCUSIGN_AUTH_SERVER": "https://account.docusign.com"
}
```

Felder, die der Admin vorbelegt (über MSI/Intune):
- `INTEGRATION_KEY_JWT`, `ACCOUNT_ID`, `DOCUSIGN_BASE_URI`, `DOCUSIGN_AUTH_SERVER`

Felder, die der User selbst einträgt:
- `CC_EMAIL`, `CC_NAME`, `IMPERSONATION_USER_GUID`

---

## 5. Ablauf C#-Anwendung

```
Programmstart
    │
    ├─ settings.json laden aus %APPDATA%\DS-IHF\config\
    ├─ private.key laden aus %APPDATA%\DS-IHF\config\
    │
    ├─ Consent bereits erteilt? (consent.done vorhanden?)
    │   Nein → Browser öffnen → warten → consent.done schreiben
    │
    ├─ JWT erstellen (RSA-signiert, exp = jetzt + 3600s)
    ├─ POST https://account.docusign.com/oauth/token → Access Token
    │
    ├─ FolderBrowserDialog öffnen → Ordner wählen
    ├─ Alle *.docx im Ordner auflisten
    │
    └─ foreach .docx:
        ├─ DocumentMetadata aus .docx lesen (DSEMAIL, DSASP, DSVA)
        ├─ Envelope JSON aufbauen
        ├─ Multipart-POST an DocuSign /v2.1/accounts/{id}/envelopes
        ├─ Ergebnis in DS-IHF.log schreiben
        └─ Weiter mit nächster Datei
```

---

## 6. Rollout — MSI

### Installer-Inhalt
```
DS-IHF-Setup.msi
    INSTALLDIR  = C:\Program Files\DS-IHF\
        DS.IHF.exe
        DS.IHF.dll
        DocumentFormat.OpenXml.dll  (alle Dependencies)
    
    USERAPPDATA  = %APPDATA%\DS-IHF\config\   (pro User, bei Erstinstall)
        settings.json          ← Vorlage mit Admin-Vorbelegungen
        settings.template.json ← Original für Reset
        private.key            ← LEER (User/Admin muss befüllen)
```

### MSI-Eigenschaften (WiX oder Advanced Installer)
- `INSTALLDIR`: `C:\Program Files\DS-IHF\`
- Shortcut auf Desktop + Startmenü → `DS.IHF.exe`
- Per-User-Konfiguration wird nur beim **Erstinstall** nach `%APPDATA%` kopiert  
  (nicht bei Updates überschrieben — `NeverOverwrite`-Flag)
- Kein Admin-Recht zur Laufzeit nötig (App läuft als normaler User)

---

## 7. Rollout — Intune (Win32-App)

### Paketierung
```
intunewin-Paket:
    DS-IHF-Setup.msi
    install.cmd   → msiexec /i DS-IHF-Setup.msi /qn
    uninstall.cmd → msiexec /x {PRODUCT-GUID} /qn
```

### Intune-Konfiguration
| Feld | Wert |
|---|---|
| Installationsbefehl | `msiexec /i DS-IHF-Setup.msi /qn` |
| Deinstallationsbefehl | `msiexec /x {PRODUCT-GUID} /qn` |
| Erkennungsregel | Datei: `C:\Program Files\DS-IHF\DS.IHF.exe` |
| Zuweisung | Benutzergruppe (nicht Gerätegruppe) → damit %APPDATA% pro User korrekt befüllt wird |
| Abhängigkeit | .NET 8 Desktop Runtime (als eigene Win32-App oder via winget) |

### Pro-User-Konfiguration via Intune
Optionen:
1. **Intune-PowerShell-Skript** (einmalig nach Erstinstall):  
   Kopiert `settings.json` mit vorbelgten Werten nach `%APPDATA%\DS-IHF\config\`
2. **Administrative Templates (ADMX)** wenn künftig Registry-basierte Settings gewünscht
3. **Einfachste Variante**: Admin-Felder direkt im MSI als Properties mitgeben  
   `msiexec /i DS-IHF-Setup.msi INTEGRATION_KEY="xxx" ACCOUNT_ID="yyy" /qn`

---

## 8. Abhängigkeiten

| Paket | Zweck | NuGet |
|---|---|---|
| `DocumentFormat.OpenXml` | .docx lesen ohne Word | `DocumentFormat.OpenXml` |
| .NET 8 | Runtime | Systemvoraussetzung |
| Kein externer NuGet für JWT | RSA + Base64 ist .NET built-in | — |

---

## 9. Was bewusst weggelassen wird

| PS1-Feature | Grund für Wegfall |
|---|---|
| `Install-NugetPackage.ps1` | Nicht nötig — .NET hat RSA built-in |
| Token-Datei `ds_access_tokenJWT.txt` | Token im Speicher halten, sicherer |
| `code_grant.ps1` | Nur für Ersteinrichtung/Consent, wird vereinfacht |
| Word-COM Objekt | Ersetzt durch OpenXml — kein Word nötig |
| `QUICKSTART`-Modus | Nicht relevant für Produktivbetrieb |

---

## 10. Nächste Schritte

1. `AppSettings.cs` + Settings-Loader
2. `DocuSignAuthService.cs` — JWT + Token
3. `WordDocumentReader.cs` — DSEMAIL/DSASP/DSVA aus .docx
4. `DocuSignSendService.cs` — Envelope senden
5. `Program.cs` — Ablaufsteuerung + Folder-Dialog
6. MSI-Projekt (WiX oder Advanced Installer)
