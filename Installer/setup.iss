; --------------------------------------------
; Inno Setup Script for AsterixViewer
; --------------------------------------------

[Setup]
AppName=Asterix Parser
AppVersion=1.0.0
DefaultDirName={pf}\AsterixParser
DefaultGroupName=Asterix Parser
OutputDir=installer
OutputBaseFilename=AsterixParserInstaller
Compression=lzma
SolidCompression=yes
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
DisableProgramGroupPage=yes

; Optional: create a desktop shortcut
;SetupIconFile=..\AsterixViewer\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\ASTERIXParser.exe

[Files]
; Copy everything from the publish folder
Source: "..\AsterixViewer\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs

[Icons]
Name: "{group}\ASTERIXParser"; Filename: "{app}\ASTERIXParser.exe"
Name: "{commondesktop}\ASTERIXParser"; Filename: "{app}\ASTERIXParser.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop icon"; GroupDescription: "Additional Tasks"

[Run]
Filename: "{app}\ASTERIXParser.exe"; Description: "Launch Asterix Parser"; Flags: nowait postinstall skipifsilent
