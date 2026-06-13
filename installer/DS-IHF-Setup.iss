#define AppName "DS-IHF Serienbrief-Upload"
#define AppVersion "1.0.0"
#define AppPublisher "Hausärztinnen- und Hausärzteverband"
#define AppExeName "DS.IHF.exe"
#define BuildDir "..\src\DS.IHF\bin\Release\net8.0-windows"

[Setup]
AppId={{B7C3D4E5-F6A7-8901-BCDE-F12345678901}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={localappdata}\DS-IHF
DefaultGroupName=DS-IHF
DisableProgramGroupPage=no
OutputDir=output
OutputBaseFilename=DS-IHF-Setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=lowest
WizardStyle=modern
SetupIconFile=
UninstallDisplayName={#AppName}

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "Desktop-Verknüpfung erstellen"; GroupDescription: "Zusätzliche Symbole:"; Flags: unchecked

[Files]
Source: "{#BuildDir}\DS.IHF.exe";                                                          DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\DS.IHF.dll";                                                          DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\DS.IHF.deps.json";                                                    DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\DS.IHF.runtimeconfig.json";                                           DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\DocumentFormat.OpenXml.dll";                                          DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\DocumentFormat.OpenXml.Framework.dll";                                DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildDir}\System.IO.Packaging.dll";                                             DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Startmenü
Name: "{group}\DS-IHF Serienbrief-Upload";        Filename: "{app}\{#AppExeName}"; Comment: "DocuSign Serienbriefe hochladen"
Name: "{group}\DS-IHF Serienbrief-Upload (Demo)"; Filename: "{app}\{#AppExeName}"; Parameters: "/Demo"; Comment: "DocuSign Serienbriefe hochladen - Demoumgebung"
Name: "{group}\Deinstallieren";                   Filename: "{uninstallexe}"

; Desktop (optional)
Name: "{autodesktop}\DS-IHF Serienbrief-Upload";  Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "DS-IHF jetzt starten"; Flags: nowait postinstall skipifsilent

[Code]
// .NET 8 Runtime prüfen
function IsDotNet8Installed(): Boolean;
var
  key: String;
begin
  key := 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.NETCore.App';
  Result := RegKeyExists(HKLM, key) or RegKeyExists(HKCU, key);
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  if not IsDotNet8Installed() then
  begin
    if MsgBox('.NET 8 Desktop Runtime ist nicht installiert.' + #13#10 +
              'Die Anwendung benötigt .NET 8 um zu funktionieren.' + #13#10 + #13#10 +
              'Möchten Sie die Download-Seite öffnen?',
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/8.0', '', '', SW_SHOW, ewNoWait, Result);
    end;
    Result := False;
  end;
end;
