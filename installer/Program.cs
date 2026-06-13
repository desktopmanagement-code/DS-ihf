using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.UI;

var buildDir = Path.GetFullPath(@"..\src\DS.IHF\bin\Release\net8.0-windows");

var project = new ManagedProject("DS-IHF Serienbrief-Upload",

    // Dateien installieren nach %LOCALAPPDATA%\DS-IHF
    new Dir(@"%LocalAppDataFolder%\DS-IHF",
        new Files($@"{buildDir}\*.*")),

    // Startmenü-Einträge
    new Dir(@"%ProgramMenuFolder%\DS-IHF",
        new ExeFileShortcut("DS-IHF Serienbrief-Upload", $@"[INSTALLFOLDER]DS.IHF.exe", "")
        {
            WorkingDirectory = "[INSTALLFOLDER]",
            Description = "DocuSign Serienbriefe hochladen"
        },
        new ExeFileShortcut("DS-IHF Serienbrief-Upload (Demo)", $@"[INSTALLFOLDER]DS.IHF.exe", "/Demo")
        {
            WorkingDirectory = "[INSTALLFOLDER]",
            Description = "DocuSign Serienbriefe hochladen - Demoumgebung"
        }
    )
);

project.GUID             = new Guid("B7C3D4E5-F6A7-8901-BCDE-F12345678901");
project.Version          = new Version("1.0.0.0");
project.Manufacturer     = "Hausärztinnen- und Hausärzteverband";
project.InstallScope     = InstallScope.perUser;
project.MajorUpgradeStrategy = MajorUpgradeStrategy.MostRecent;
project.UI               = WUI.WixUI_Minimal;
project.OutFileName      = "DS-IHF-Setup";
project.OutDir           = "output";

// .NET 8 Voraussetzung prüfen
project.LaunchConditions.Add(
    new LaunchCondition(
        "NETRUNTIME8 OR Installed",
        "Bitte installieren Sie zuerst .NET 8 Desktop Runtime.\nhttps://dotnet.microsoft.com/download/dotnet/8.0"));

Compiler.BuildMsi(project);
