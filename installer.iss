; PhotoBOX Inno Setup Script
; 4択ウィザード対応版

#define MyAppName "PhotoBOX"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "koita5959-ux"
#define MyAppExeName "PhotoBOX.App.exe"
#define MyStrategyName "CenterCrop"

[Setup]
AppId={code:GetAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autodesktop}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=installer
OutputBaseFilename=PhotoBOX_Setup_v{#MyAppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
DisableDirPage=auto
DisableProgramGroupPage=auto
UsePreviousLanguage=no

[Languages]
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

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
; 別バージョン追加インストール用アイコン
Name: "{group}\{#MyAppName} - {#MyStrategyName} v{#MyAppVersion}"; Filename: "{app}\{#MyAppExeName}"; Check: IsSideInstall

[UninstallDelete]
Type: filesandordirs; Name: "{app}\results"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
Filename: "{cmd}"; Parameters: "/c start """" ""{app}"""; Flags: nowait postinstall skipifsilent runhidden; Description: "インストールフォルダを開く"

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
  SideNamePage: TInputQueryWizardPage;

// ── AppIdの動的生成 ──
function GetAppId(Param: String): String;
begin
  if SelectedMode = MODE_SIDE then
    Result := '{#MyAppName}_' + SideNamePage.Values[0]
  else
    Result := '{{B8A3D2E1-7F4C-4E9A-A1D6-3C5E8F2B9D47}';
end;

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
  // まずHKCUを検索（PrivilegesRequired=lowest時）
  if RegQueryStringValue(HKCU,
    'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{B8A3D2E1-7F4C-4E9A-A1D6-3C5E8F2B9D47}_is1',
    'UninstallString', UninstStr) then
  begin
    Result := RemoveQuotes(UninstStr);
    Exit;
  end;
  // 見つからなければHKLM（admin時の旧インストール）
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
  // まずHKCUを検索
  if RegQueryStringValue(HKCU,
    'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{B8A3D2E1-7F4C-4E9A-A1D6-3C5E8F2B9D47}_is1',
    'InstallLocation', Dir) then
  begin
    Result := Dir;
    Exit;
  end;
  // 見つからなければHKLM
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

// ── 指定ルートキー配下の全PhotoBOXを検出して削除 ──
procedure RemovePhotoBOXFromRoot(RootKey: Integer);
var
  Keys: TArrayOfString;
  I: Integer;
  DisplayName, UninstStr, InstDir: String;
  ResultCode: Integer;
  BasePath: String;
begin
  BasePath := 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall';
  if RegGetSubkeyNames(RootKey, BasePath, Keys) then
  begin
    for I := 0 to GetArrayLength(Keys) - 1 do
    begin
      if RegQueryStringValue(RootKey, BasePath + '\' + Keys[I], 'DisplayName', DisplayName) then
      begin
        if Pos('PhotoBOX', DisplayName) > 0 then
        begin
          if RegQueryStringValue(RootKey, BasePath + '\' + Keys[I], 'UninstallString', UninstStr) then
          begin
            UninstStr := RemoveQuotes(UninstStr);
            if FileExists(UninstStr) then
              Exec(UninstStr, '/SILENT /NORESTART', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
          end;
          if RegQueryStringValue(RootKey, BasePath + '\' + Keys[I], 'InstallLocation', InstDir) then
            CleanLeftoverDir(InstDir);
        end;
      end;
    end;
  end;
end;

// ── 全PhotoBOX関連のインストールを検出して削除 ──
procedure RemoveAllPhotoBOXInstalls();
begin
  // HKCUとHKLMの両方を検索
  RemovePhotoBOXFromRoot(HKCU);
  RemovePhotoBOXFromRoot(HKLM);

  // デスクトップ上のPhotoBOX関連ショートカット削除
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
    'PhotoBOXはコアテスト用アプリです。判定ルールの検証・強化のため、デスクトップにインストールされます。');

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

  // 識別名入力ページ（MODE_SIDE時のみ表示）
  SideNamePage := CreateInputQueryPage(ModePage.ID,
    '識別名の入力',
    '並立インストールの識別名を入力してください。',
    'この識別名がフォルダ名・ショートカット名に使われます。');
  SideNamePage.Add('識別名（例: CenterCrop_v1）:', False);
  SideNamePage.Values[0] := 'CenterCrop_v1.0.0';
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

  // 識別名入力ページの「次へ」でバリデーション＋フォルダ名・グループ名を設定
  if CurPageID = SideNamePage.ID then
  begin
    if SideNamePage.Values[0] = '' then
    begin
      MsgBox('識別名を入力してください。', mbError, MB_OK);
      Result := False;
      Exit;
    end;

    Dir := ExpandConstant('{autodesktop}\{#MyAppName}_') + SideNamePage.Values[0];
    if DirExists(Dir) then
    begin
      if MsgBox('この識別名は既にインストールされています。上書きしますか？',
         mbConfirmation, MB_YESNO) = IDNO then
      begin
        Result := False;
        Exit;
      end;
    end;

    WizardForm.DirEdit.Text := Dir;
    WizardForm.GroupEdit.Text := '{#MyAppName} - ' + SideNamePage.Values[0];
  end;

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
        // 識別名入力ページの「次へ」で反映
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

  // 識別名入力ページはMODE_SIDE以外ではスキップ
  if (PageID = SideNamePage.ID) and (SelectedMode <> MODE_SIDE) then
    Result := True;

  // フォルダ選択ページは全モードでスキップ
  if (PageID = wpSelectDir) then
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
