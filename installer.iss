; PhotoBOX Inno Setup Script
; 4択ウィザード対応版

#define MyAppName "PhotoBOX"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "koita5959-ux"
#define MyAppExeName "PhotoBOX.App.exe"
#define MyStrategyName "CenterCrop"

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
DisableDirPage=auto
DisableProgramGroupPage=auto

[Languages]
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
; パブリッシュ出力からexeをコピー（全モードで配置）
Source: "publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
; ONNXモデル（上書きモード時はスキップ）
Source: "publish\Models\*"; DestDir: "{app}\Models"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsFullInstall
; カテゴリ設定（上書きモード時はスキップ）
Source: "publish\Config\*"; DestDir: "{app}\Config"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsFullInstall
; テストデータ（上書きモード時はスキップ）
Source: "publish\testdata\*"; DestDir: "{app}\testdata"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsFullInstall

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Check: IsNotSideInstall
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Check: IsNotSideInstall
; 別バージョン追加インストール用アイコン
Name: "{group}\{#MyAppName} - {#MyStrategyName} v{#MyAppVersion}"; Filename: "{app}\{#MyAppExeName}"; Check: IsSideInstall
Name: "{autodesktop}\{#MyAppName} - {#MyStrategyName} v{#MyAppVersion}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Check: IsSideInstall

[UninstallDelete]
Type: filesandordirs; Name: "{app}\results"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
const
  MODE_FULL = 0;
  MODE_OVERWRITE = 1;
  MODE_SIDE = 2;
  MODE_REMOVE = 3;

var
  ModePage: TWizardPage;
  ModeRadios: array[0..3] of TRadioButton;
  SelectedMode: Integer;

// ── Check関数：ファイルコピーの制御 ──
function IsFullInstall(): Boolean;
begin
  Result := (SelectedMode <> MODE_OVERWRITE);
end;

function IsSideInstall(): Boolean;
begin
  Result := (SelectedMode = MODE_SIDE);
end;

function IsNotSideInstall(): Boolean;
begin
  Result := (SelectedMode <> MODE_SIDE);
end;

// ── 既存PhotoBOXのアンインストール情報をレジストリから取得 ──
function GetExistingUninstallString(): String;
var
  UninstStr: String;
begin
  Result := '';
  if RegQueryStringValue(HKLM,
    'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{B8A3D2E1-7F4C-4E9A-A1D6-3C5E8F2B9D47}_is1',
    'UninstallString', UninstStr) then
    Result := RemoveQuotes(UninstStr);
end;

function GetExistingInstallDir(): String;
var
  Dir: String;
begin
  Result := '';
  if RegQueryStringValue(HKLM,
    'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{B8A3D2E1-7F4C-4E9A-A1D6-3C5E8F2B9D47}_is1',
    'InstallLocation', Dir) then
    Result := Dir;
end;

// ── 既存インストールのアンインストーラーを実行 ──
function RunUninstaller(): Boolean;
var
  UninstStr: String;
  ResultCode: Integer;
begin
  Result := True;
  UninstStr := GetExistingUninstallString();
  if UninstStr <> '' then
  begin
    if FileExists(UninstStr) then
    begin
      Exec(UninstStr, '/SILENT /NORESTART', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
      Result := (ResultCode = 0);
    end;
  end;
end;

// ── 残骸フォルダの削除 ──
procedure CleanLeftoverDir(Dir: String);
begin
  if (Dir <> '') and DirExists(Dir) then
    DelTree(Dir, True, True, True);
end;

// ── 全PhotoBOX関連のレジストリキーを列挙して削除 ──
procedure RemoveAllPhotoBOXInstalls();
var
  Keys: TArrayOfString;
  I: Integer;
  DisplayName, UninstStr, InstDir: String;
  ResultCode: Integer;
  BasePath: String;
begin
  BasePath := 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall';
  if RegGetSubkeyNames(HKLM, BasePath, Keys) then
  begin
    for I := 0 to GetArrayLength(Keys) - 1 do
    begin
      if RegQueryStringValue(HKLM, BasePath + '\' + Keys[I], 'DisplayName', DisplayName) then
      begin
        if Pos('PhotoBOX', DisplayName) > 0 then
        begin
          if RegQueryStringValue(HKLM, BasePath + '\' + Keys[I], 'UninstallString', UninstStr) then
          begin
            UninstStr := RemoveQuotes(UninstStr);
            if FileExists(UninstStr) then
              Exec(UninstStr, '/SILENT /NORESTART', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
          end;
          if RegQueryStringValue(HKLM, BasePath + '\' + Keys[I], 'InstallLocation', InstDir) then
            CleanLeftoverDir(InstDir);
        end;
      end;
    end;
  end;

  // デスクトップショートカット削除
  if FileExists(ExpandConstant('{autodesktop}\{#MyAppName}.lnk')) then
    DeleteFile(ExpandConstant('{autodesktop}\{#MyAppName}.lnk'));
end;

// ── カスタムウィザードページの初期化 ──
procedure InitializeWizard();
var
  Labels: array[0..3] of String;
  Descs: array[0..3] of String;
  I, Y: Integer;
  Lbl: TLabel;
begin
  SelectedMode := MODE_FULL;

  ModePage := CreateCustomPage(wpWelcome,
    'インストールモードの選択',
    '実行するインストールモードを選択してください。');

  Labels[0] := '全インストール（推奨）';
  Labels[1] := '上書きインストール（EXEのみ更新）';
  Labels[2] := '別バージョン追加インストール';
  Labels[3] := 'すべてを削除（完全アンインストール）';

  Descs[0] := '既存をアンインストールし、全ファイルをクリーンインストールします。';
  Descs[1] := 'アプリ本体のみ更新します。testdata・Config・Modelsは維持します。';
  Descs[2] := '既存と別フォルダに並立インストールします。';
  Descs[3] := '全てのPhotoBOXを検出して完全に削除します。';

  Y := 0;
  for I := 0 to 3 do
  begin
    ModeRadios[I] := TRadioButton.Create(ModePage);
    ModeRadios[I].Parent := ModePage.Surface;
    ModeRadios[I].Top := Y;
    ModeRadios[I].Left := 8;
    ModeRadios[I].Width := ModePage.SurfaceWidth - 16;
    ModeRadios[I].Caption := Labels[I];
    ModeRadios[I].Font.Style := [fsBold];
    ModeRadios[I].Checked := (I = 0);

    Lbl := TLabel.Create(ModePage);
    Lbl.Parent := ModePage.Surface;
    Lbl.Top := Y + 22;
    Lbl.Left := 28;
    Lbl.Width := ModePage.SurfaceWidth - 40;
    Lbl.Caption := Descs[I];
    Lbl.Font.Color := clGray;

    Y := Y + 52;
  end;
end;

// ── 選択されたモードを取得 ──
function GetSelectedMode(): Integer;
var
  I: Integer;
begin
  Result := MODE_FULL;
  for I := 0 to 3 do
    if ModeRadios[I].Checked then
    begin
      Result := I;
      Exit;
    end;
end;

// ── 「次へ」ボタン押下時の処理 ──
function NextButtonClick(CurPageID: Integer): Boolean;
var
  Dir: String;
begin
  Result := True;

  if CurPageID = ModePage.ID then
  begin
    SelectedMode := GetSelectedMode();

    case SelectedMode of
      MODE_FULL:
      begin
        Dir := GetExistingInstallDir();
        RunUninstaller();
        CleanLeftoverDir(Dir);
      end;

      MODE_OVERWRITE:
      begin
        // EXEのみ上書き — 追加処理なし
      end;

      MODE_SIDE:
      begin
        WizardForm.DirEdit.Text := ExpandConstant('{autopf}\{#MyAppName}_{#MyStrategyName}_v{#MyAppVersion}');
        WizardForm.GroupEdit.Text := '{#MyAppName} - {#MyStrategyName} v{#MyAppVersion}';
      end;

      MODE_REMOVE:
      begin
        if MsgBox('全てのPhotoBOX関連ファイルを削除します。' + #13#10 + 'よろしいですか？',
           mbConfirmation, MB_YESNO) = IDYES then
        begin
          RemoveAllPhotoBOXInstalls();
          MsgBox('全てのPhotoBOXを削除しました。' + #13#10 + 'セットアップを終了します。', mbInformation, MB_OK);
          WizardForm.Close;
          Result := False;
        end
        else
          Result := False;
      end;
    end;
  end;
end;

// ── インストール不要なページをスキップ ──
function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;

  // 上書きインストール・全インストール時はフォルダ選択をスキップ
  if (PageID = wpSelectDir) and (SelectedMode <> MODE_SIDE) then
    Result := True;
end;

// ── アンインストール時に残骸を完全除去 ──
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    if DirExists(ExpandConstant('{app}')) then
      DelTree(ExpandConstant('{app}'), True, True, True);
  end;
end;
