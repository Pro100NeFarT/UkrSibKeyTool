#define MyAppName "UkrSibKeyTool"
#define MyAppVersion "1.1.0"
#define MyAppPublisher "Pro100NeFarT"
#define MyAppURL "https://github.com/Pro100NeFarT/UkrSibKeyTool"
#define MyAppExeName "UkrSibKeyTool.exe"

[Setup]
AppId={{F10192A8-3BC2-42AD-A873-C283AF22F33F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=..\installer-output
OutputBaseFilename=UkrSibKeyTool-Setup-{#MyAppVersion}
SetupIconFile=..\Icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; Lets Setup close a running instance of the app (via Restart Manager) before
; overwriting its files instead of failing with a "file in use" error — needed
; both for a normal reinstall-over-running-app and for the in-app auto-updater,
; which launches this installer silently while the app is still executing.
CloseApplications=yes
CloseApplicationsFilter=*.exe
RestartApplications=no

[Languages]
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce

[Files]
Source: "..\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; No "skipifsilent": the in-app auto-updater runs this installer silently and
; relies on Setup itself to relaunch the app afterward.
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall
