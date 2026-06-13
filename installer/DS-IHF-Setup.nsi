; DS-IHF Serienbrief-Upload Setup
; NSIS Installer Script

Unicode True

!define APP_NAME "DS-IHF Serienbrief-Upload"
!define APP_VERSION "1.0.0"
!define APP_PUBLISHER "Aequitas-Software für Hausärztinnen- und Hausärzteverband (Tobias Paul)"
!define APP_EXE "DS.IHF.exe"
!define BUILD_DIR "..\src\DS.IHF\bin\Release\net8.0-windows\win-x64\publish"
!define INSTALL_DIR "$PROGRAMFILES64\DS-IHF"
!define UNINSTALL_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\DS-IHF"
!define APP_GUID "{B7C3D4E5-F6A7-8901-BCDE-F12345678901}"

Name "${APP_NAME}"
OutFile "output\DS-IHF-Setup.exe"
InstallDir "${INSTALL_DIR}"
RequestExecutionLevel admin
SetCompressor /SOLID lzma
Icon "..\src\DS.IHF\DSIHF.ico"
UninstallIcon "..\src\DS.IHF\DSIHF.ico"

; Moderne Oberfläche
!include "MUI2.nsh"
!define MUI_ABORTWARNING

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "German"

; ─── Installation ────────────────────────────────────────────────────────────
Section "Hauptprogramm" SecMain

    SetOutPath "$INSTDIR"

    File "${BUILD_DIR}\DS.IHF.exe"
    File "..\src\DS.IHF\DSIHF.ico"

    ; Startmenü-Einträge
    CreateDirectory "$SMPROGRAMS\DS-IHF"
    CreateShortcut "$SMPROGRAMS\DS-IHF\DS-IHF Serienbrief-Upload.lnk" \
        "$INSTDIR\${APP_EXE}" "" "$INSTDIR\DSIHF.ico" 0
    CreateShortcut "$SMPROGRAMS\DS-IHF\Deinstallieren.lnk" \
        "$INSTDIR\Uninstall.exe"

    ; Deinstallationsprogramm erstellen
    WriteUninstaller "$INSTDIR\Uninstall.exe"

    ; In Windows Programme & Features eintragen
    WriteRegStr HKLM "${UNINSTALL_KEY}" "DisplayName"     "${APP_NAME}"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "DisplayVersion"  "${APP_VERSION}"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "Publisher"       "${APP_PUBLISHER}"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "InstallLocation" "$INSTDIR"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "UninstallString" "$INSTDIR\Uninstall.exe"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "NoModify"        "1"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "NoRepair"        "1"

SectionEnd

; ─── Deinstallation ──────────────────────────────────────────────────────────
Section "Uninstall"

    ; Dateien entfernen
    Delete "$INSTDIR\DS.IHF.exe"
    Delete "$INSTDIR\DSIHF.ico"
    Delete "$INSTDIR\Uninstall.exe"
    RMDir  "$INSTDIR"

    ; Startmenü entfernen
    Delete "$SMPROGRAMS\DS-IHF\DS-IHF Serienbrief-Upload.lnk"
    Delete "$SMPROGRAMS\DS-IHF\Deinstallieren.lnk"
    RMDir  "$SMPROGRAMS\DS-IHF"

    ; Registry-Eintrag entfernen
    DeleteRegKey HKLM "${UNINSTALL_KEY}"

SectionEnd
