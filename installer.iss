; PhotoBOX Inno Setup Script
; 1フォルダ複数exe方式・自動採番

#define MyAppName "PhotoBOX"
#define MyAppVersion "1.01"
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
CreateUninstallRegKey=no
Uninstallable=no

[Languages]
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Files]
; 全インストール・上書き: PhotoBOX.App.exeを配置
Source: "publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion; Check: IsNotSideInstall
; 別バージョン追加: 自動採番でリネームして配置
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
Filename: "{app}\{code:GetLaunchExeName}"; Description: "{code:GetLaunchDescription}"; Flags: nowait postinstall skipifsilent
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
  SideExeName: String;

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

// ── 自動採番で次のexe名を決定 ──
function GetNextSideExeName(): String;
var
  FindRec: TFindRec;
  AppDir, FileName, Prefix, SeqStr: String;
  MaxSeq, Seq: Integer;
begin
  AppDir := ExpandConstant('{autodesktop}\{#MyAppName}');
  Prefix := 'CenterCrop_v{#MyAppVersion}.';
  MaxSeq := 0;

  if FindFirst(AppDir + '\CenterCrop_v{#MyAppVersion}.*.exe', FindRec) then
  begin
    try
      repeat
        FileName := FindRec.Name;
        // .exeを除去してシーケンス番号を取得
        SeqStr := Copy(FileName, Length(Prefix) + 1, Length(FileName) - Length(Prefix) - 4);
        Seq := StrToIntDef(SeqStr, 0);
        if Seq > MaxSeq then
          MaxSeq := Seq;
      until not FindNext(FindRec);
    finally
      FindClose(FindRec);
    end;
  end;

  Result := 'CenterCrop_v{#MyAppVersion}.' + Format('%.2d', [MaxSeq + 1]) + '.exe';
end;

// ── 自動採番exeのファイル名を返す（[Files]セクション用） ──
function GetSideExeName(Param: String): String;
begin
  Result := SideExeName;
end;

// ── 実行するexe名を返す（[Run]セクション用） ──
function GetLaunchExeName(Param: String): String;
begin
  if SelectedMode = MODE_SIDE then
    Result := SideExeName
  else
    Result := '{#MyAppExeName}';
end;

// ── 完了画面の実行ファイル説明 ──
function GetLaunchDescription(Param: String): String;
begin
  if SelectedMode = MODE_SIDE then
    Result := SideExeName + ' を実行する'
  else
    Result := '{#MyAppExeName} を実行する';
end;

// ── インストール準備完了画面の表示内容 ──
function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
begin
  Result := '';
  Result := Result + 'インストール先:' + NewLine + Space + ExpandConstant('{app}') + NewLine + NewLine;
  if SelectedMode = MODE_SIDE then
    Result := Result + 'インストールするexe:' + NewLine + Space + SideExeName + NewLine
  else if SelectedMode = MODE_OVERWRITE then
    Result := Result + 'インストールするexe:' + NewLine + Space + '{#MyAppExeName}（上書き）' + NewLine
  else
    Result := Result + 'モード: 全インストール（クリーン）' + NewLine;
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
  AppDir := ExpandConstant('{autodesktop}\{#MyAppName}');
  CleanLeftoverDir(AppDir);
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
  SideExeName := '';

  ModePage := CreateCustomPage(wpWelcome,
    'インストールモードの選択',
    'PhotoBOXはコアテスト用アプリです。判定ルールの検証・強化のため、デスクトップにインストールされます。');

  Labels[0] := '全インストール（推奨）';
  Labels[1] := '上書きインストール（EXEのみ更新）';
  Labels[2] := '別バージョン追加インストール';
  Labels[3] := 'すべてを削除（完全アンインストール）';

  Descs[0] := 'PhotoBOXフォルダを初期化し、全ファイルをクリーンインストールします。';
  Descs[1] := 'アプリ本体（PhotoBOX.App.exe）のみ更新します。Config・Modelsは維持します。';
  Descs[2] := '自動採番されたexeを追加配置します。既存exeはそのまま維持します。';
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
  AppDir: String;
begin
  Result := True;
  AppDir := ExpandConstant('{autodesktop}\{#MyAppName}');

  if CurPageID = ModePage.ID then
  begin
    SelectedMode := GetSelectedMode();
    WizardForm.DirEdit.Text := AppDir;

    case SelectedMode of
      MODE_FULL:
      begin
        CleanLeftoverDir(AppDir);
      end;

      MODE_OVERWRITE:
      begin
        // 追加処理なし
      end;

      MODE_SIDE:
      begin
        SideExeName := GetNextSideExeName();
        if MsgBox(SideExeName + ' としてインストールします。' + #13#10 + 'よろしいですか？',
           mbConfirmation, MB_YESNO) = IDNO then
        begin
          Result := False;
          Exit;
        end;
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

  if PageID = wpSelectDir then
    Result := True;

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
