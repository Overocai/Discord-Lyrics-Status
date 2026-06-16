; ============================================================
;  Discord Lyrics — Inno Setup installer script
;  Build with: iscc DiscordLyrics.iss   (Inno Setup 6+)
;  First publish the app:
;    dotnet publish ..\DiscordLyrics.csproj -c Release -r win-x64 ^
;        --self-contained true -p:PublishSingleFile=true -o ..\..\publish
; ============================================================

#define MyAppName        "Discord Lyrics"
#define MyAppVersion     "1.0.0"
#define MyAppPublisher   "Overocai"
#define MyAppURL         "https://github.com/Overocai/Discord-Lyrics-Status"
#define MyAppExeName     "DiscordLyrics.exe"

[Setup]
AppId={{B7B2E9C4-0F2A-4F1E-9C8D-DL10STATUS001}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=..\..\dist
; Stable file name so a GitHub "latest/download" link always resolves.
OutputBaseFilename=DiscordLyrics-Setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\Assets\icon.ico
WizardImageFile=installer-sidebar.bmp
WizardSmallImageFile=installer-small.bmp
LicenseFile=..\..\docs\EULA.txt
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17763

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce
Name: "startup"; Description: "Start Discord Lyrics when Windows starts"; GroupDescription: "Startup:"; Flags: unchecked

[Files]
; Everything produced by `dotnet publish` (placed in ..\..\publish)
Source: "..\..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}";        Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}";  Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
; Optional "launch at startup" task
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; \
    ValueType: string; ValueName: "DiscordLyrics"; ValueData: """{app}\{#MyAppExeName}"""; \
    Flags: uninsdeletevalue; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; \
    Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\DiscordLyrics"
