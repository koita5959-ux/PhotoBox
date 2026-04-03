; PhotoBOX Inno Setup Script
; 1フォルダ複数exe方式

#define MyAppName "PhotoBOX"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "koita5959-ux"
#define MyAppExeName "PhotoBOX.App.exe"

[Setup]
AppId={{B8A3D2E1-7F4C-4E9A-A1D6-3C5E8F2B9D47}
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
DisableDirPage=yes
DisableProgramGroupPage=yes

[Languages]
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Files]
; 全インストール・上書き: PhotoBOX.App.exeを配置
Source: "publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion; Check: IsNotSideInstall
; 別バージョン追加: 識別名でリネームして配置
Source: "publish\{#MyAppExeName}"; DestDir: "{app}"; DestName: "{code:GetSideExeName}"; Flags: ignoreversion; Check: IsSideInstall
; Models（全インストール時は常に配置）
Source: "publish\Models\*"; DestDir: "{app}\Models"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsFullInstall
; Models（上書き・別バージョン追加時は存在しなければ配置）
Source: "publish\Models\*"; DestDir: "{app}\Models"; Flags: onlyifdoesntexist recursesubdirs createallsubdirs; Check: IsNotFullInstall
; Config（全インストール時は常に配置）
Source: "publish\Config\*"; DestDir: "{app}\Config"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: IsFullInstall
; Config（上書き・別バージョン追加時は存在しなければ配置）
Source: "publish\Config\*"; DestDir: "{app}\Config"; Flags: onlyifdoesntexist recursesubdirs createallsubdirs; Check: IsNotFullInstall

[Icons]
; スタートメニューへの登録は行わない

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Run]
Filename: "{app}\{code:GetLaunchExeName}"; Description: "PhotoBOXを実行する"; Flags: nowait postinstall skipifsilent
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

// ── Check関数：ファイルコピーの制御 ──
function IsFullInstall(): Boolean;
begin
  Result := (SelectedMode = MODE_FULL);
end;

function IsNotFullInstall(): Boolean;
begin
  Result := (SelectedMode <> MODE_FULL);
end;

function IsSideInstall(): Boolean;
begin
  Result := (SelectedMode = MODE_SIDE);
end;

function IsNotSideInstall(): Boolean;
begin
  Result := (SelectedMode <> MODE_SIDE);
end;

// ── 識別名exeのファイル名を返す ──
function GetSideExeName(Param: String): String;
begin
  Result := SideNamePage.Values[0] + '.exe';
end;

// ── 実行するexe名を返す（[Run]セクション用） ──
function GetLaunchExeName(Param: String): String;
begin
  if SelectedMode = MODE_SIDE then
    Result := SideNamePage.Values[0] + '.exe'
  else
    Result := '{#MyAppExeName}';
end;

// ── 残骸フォルダの削除 ──
procedure CleanLeftoverDir(Dir: String);
begin
  if (Dir <> '') and DirExists(Dir) then
    DelTree(Dir, True, True, True);
end;

// ── 指定ルートキー配下の全PhotoBOXレジストリを削除 ──
procedure RemovePhotoBOXFromRoot(RootKey: Integer);
var
  Keys: TArrayOfString;
  I: Integer;
  DisplayName: String;
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
          RegDeleteKeyIncludingSubkeys(RootKey, BasePath + '\' + Keys[I]);
      end;
    end;
  end;
end;

// ── 全PhotoBOX関連を削除 ──
procedure RemoveAllPhotoBOXInstalls();
var
  AppDir: String;
begin
  // デスクトップのPhotoBOXフォルダを削除
  AppDir := ExpandConstant('{autodesktop}\{#MyAppName}');
  CleanLeftoverDir(AppDir);

  // HKCU/HKLMのレジストリ掃除
  RemovePhotoBOXFromRoot(HKCU);
  RemovePhotoBOXFromRoot(HKLM);
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

  Descs[0] := 'PhotoBOXフォルダを初期化し、全ファイルをクリーンインストールします。';
  Descs[1] := 'アプリ本体（PhotoBOX.App.exe）のみ更新します。Config・Modelsは維持します。';
  Descs[2] := '識別名付きのexeを追加配置します。既存exeはそのまま維持します。';
  Descs[3] := 'デスクトップのPhotoBOXフォルダとレジストリを完全に削除します。';

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
    'この識別名がexeファイル名になります。（例: CenterCrop_v1 → CenterCrop_v1.exe）');
  SideNamePage.Add('識別名:', False);
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
  AppDir, ExePath: String;
begin
  Result := True;
  AppDir := ExpandConstant('{autodesktop}\{#MyAppName}');

  // 識別名入力ページのバリデーション
  if CurPageID = SideNamePage.ID then
  begin
    if SideNamePage.Values[0] = '' then
    begin
      MsgBox('識別名を入力してください。', mbError, MB_OK);
      Result := False;
      Exit;
    end;

    ExePath := AppDir + '\' + SideNamePage.Values[0] + '.exe';
    if FileExists(ExePath) then
    begin
      if MsgBox('この識別名のexeは既に存在します。上書きしますか？',
         mbConfirmation, MB_YESNO) = IDNO then
      begin
        Result := False;
        Exit;
      end;
    end;

    // インストール先は常に同一フォルダ
    WizardForm.DirEdit.Text := AppDir;
  end;

  // モード選択ページの処理
  if CurPageID = ModePage.ID then
  begin
    SelectedMode := GetSelectedMode();

    // 全モード共通: インストール先を固定
    WizardForm.DirEdit.Text := AppDir;

    case SelectedMode of
      MODE_FULL:
      begin
        // デスクトップのPhotoBOXフォルダを丸ごと削除
        CleanLeftoverDir(AppDir);
      end;

      MODE_OVERWRITE:
      begin
        // 追加処理なし
      end;

      MODE_SIDE:
      begin
        // 識別名入力ページで処理
      end;

      MODE_REMOVE:
      begin
        if MsgBox('デスクトップのPhotoBOXフォルダとレジストリ情報を全て削除します。' + #13#10 + 'よろしいですか？',
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
  if PageID = wpSelectDir then
    Result := True;

  // スタートメニューフォルダ指定ページは全モードでスキップ
  if PageID = wpSelectProgramGroup then
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
