; PhotoBOX Inno Setup Script
; モニター向け上書きインストール版

#define MyAppName "PhotoBOX"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "koita5959-ux"
#define MyAppExeName "PhotoBOX.App.exe"

[Setup]
AppId={{B8A3D2E1-7F4C-4E9A-A1D6-3C5E8F2B9D47}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=installer
OutputBaseFilename=PhotoBOX_Setup_v{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin

[Languages]
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; パブリッシュ出力からexeをコピー
Source: "publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
; ONNXモデル
Source: "publish\Models\*"; DestDir: "{app}\Models"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: DirExists(ExpandConstant('{src}\publish\Models'))
; カテゴリ設定
Source: "publish\Config\*"; DestDir: "{app}\Config"; Flags: ignoreversion recursesubdirs createallsubdirs
; テストデータ
Source: "publish\testdata\*"; DestDir: "{app}\testdata"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[UninstallDelete]
Type: filesandordirs; Name: "{app}\results"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
