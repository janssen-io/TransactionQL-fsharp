; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "TransactionQL"
#define MyAppVersion "{version}" ; replaced by FAKE - Build
#define Source "{sourceDir}"     ; replaced by FAKE - Build
#define MyAppPublisher "Stan Ionițoiu-Janssen"
#define MyAppURL "https://github.com/janssen-io/TransactionQL-fsharp"
#define MyAppExeName "TransactionQL.DesktopApp.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{67BFB698-C4B5-4609-B529-F9653B528577}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={userappdata}\tql
DisableDirPage=yes
; "ArchitecturesAllowed=x64compatible" specifies that Setup cannot run
; on anything but x64 and Windows 11 on Arm.
ArchitecturesAllowed=x64compatible
; "ArchitecturesInstallIn64BitMode=x64compatible" requests that the
; install be done in "64-bit mode" on x64 or Windows 11 on Arm,
; meaning it should use the native 64-bit Program Files directory and
; the 64-bit view of the registry.
ArchitecturesInstallIn64BitMode=x64compatible
ChangesAssociations=yes
DisableProgramGroupPage=yes
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=lowest
OutputDir=C:\Users\mail\Data\Software\TransactionQL-fsharp\Setup
OutputBaseFilename=tql_setup
SetupIconFile=C:\Users\mail\Data\Software\TransactionQL-fsharp\src\TransactionQL.DesktopApp\Assets\lion.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Types]
Name: "full";     Description: "Full installation"
Name: "cli-only"; Description: "Just the CLI"
Name: "Custom";   Description: "Custom installation"; Flags: iscustom

[Components]
Name: "CLI";           Description: "Command Line Interface (tql.exe)";       Types: full cli-only custom;
Name: "GUI";           Description: "Graphical User Interface (Ledger Leo)";  Types: full custom;
Name: "Plugins";       Description: "Transaction Parsers";                    Types: full custom;
Name: "Plugins\ASN";   Description: "ASN";                                    Types: full cli-only custom; Flags: checkablealone
Name: "Plugins\Bunq";  Description: "Bunq";                                   Types: full cli-only custom; Flags: checkablealone
Name: "Plugins\ING";   Description: "ING";                                    Types: full cli-only custom; Flags: checkablealone

[Files]
Source: "{#Source}\desktop\*";          Components: GUI;          DestDir: "{app}\app";                           Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#Source}\tql.exe";            Components: CLI;          DestDir: "{app}";         DestName: "tql.exe";  Flags: ignoreversion
Source: "{#Source}\plugins\asn.dll";    Components: Plugins\ASN;  DestDir: "{app}\plugins"; DestName: "asn.dll";  Flags: ignoreversion
Source: "{#Source}\plugins\bunq.dll";   Components: Plugins\Bunq; DestDir: "{app}\plugins"; DestName: "bunq.dll"; Flags: ignoreversion
Source: "{#Source}\plugins\ing.dll";    Components: Plugins\ING;  DestDir: "{app}\plugins"; DestName: "ing.dll";  Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\app\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent; Components: GUI
