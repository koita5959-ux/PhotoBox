using PhotoJudge.Core;
using PhotoJudge.Interfaces;
using PhotoJudge.CategoryMapping;
using PhotoBOX.App.Results;

namespace PhotoBOX.App.Forms;

public partial class MainForm : Form
{
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp"];

    private readonly string _baseDir;
    private readonly string _modelPath;
    private readonly string _configDir;

    private string _version = "0.0.0";
    private string _buildDate = "不明";
    private ICropStrategy? _strategy;
    private string? _configPath;

    private List<JudgeResult> _results = [];
    private readonly Dictionary<JudgeResult, bool> _ngFlags = [];
    private readonly Dictionary<JudgeResult, string> _imagePaths = [];

    // カテゴリチェックボックス管理
    private readonly List<CheckBox> _categoryCheckBoxes = [];

    // 再判定サイクル管理
    private int _currentRound;
    private readonly HashSet<string> _usedCategories = [];

    private bool _isRunning;

    public MainForm()
    {
        InitializeComponent();

        _baseDir = AppContext.BaseDirectory;
        _modelPath = Path.Combine(_baseDir, "Models", "mobilenetv2-7.onnx");
        _configDir = Path.Combine(_baseDir, "Config");

        txtFolderPath.Text = "";

        btnSelectFolder.Click += BtnSelectFolder_Click;
        btnRun.Click += BtnRun_Click;
        btnExportCsv.Click += BtnExportCsv_Click;
        btnReJudge.Click += BtnReJudge_Click;

        Load += MainForm_Load;
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        _version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0";
        var exePath = Environment.ProcessPath;
        _buildDate = exePath != null && File.Exists(exePath)
            ? File.GetLastWriteTime(exePath).ToString("yyyy-MM-dd HH:mm")
            : "不明";

        var strategies = StrategyLoader.LoadAll();
        _strategy = strategies.Count > 0 ? strategies[0] : null;

        var configFiles = Directory.Exists(_configDir)
            ? Directory.GetFiles(_configDir, "*.json").OrderBy(f => f).ToList()
            : [];
        _configPath = configFiles.Count > 0 ? configFiles[0] : null;

        var exeName = Path.GetFileNameWithoutExtension(Environment.ProcessPath ?? "PhotoBOX");
        Text = $"PhotoBOX - {exeName}";

        var strategyName = _strategy?.Name ?? "未検出";
        var configName = _configPath != null ? Path.GetFileNameWithoutExtension(_configPath) : "未検出";
        lblSpecTitle.Text = strategyName;
        lblSpecParams.Text = "短辺：224px　長辺：元画像比率維持";
        lblSpecDesc.Text = _strategy?.Description ?? "";
        lblSpecCategory.Text = $"分類：{configName}";

        if (_configPath != null)
            BuildCategoryCheckList(_configPath);

        // 再判定ボタンは初期非表示
        btnReJudge.Visible = false;

        if (!File.Exists(_modelPath))
        {
            MessageBox.Show(
                $"ONNXモデルが見つかりません:\n{_modelPath}\n\nModels/mobilenetv2-7.onnx を配置してください。",
                "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnRun.Enabled = false;
        }

        if (_strategy == null)
        {
            MessageBox.Show("利用可能な戦略が見つかりません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnRun.Enabled = false;
        }

        if (_configPath == null)
        {
            MessageBox.Show(
                $"カテゴリ設定ファイルが見つかりません:\n{_configDir}\n\nConfig/ に設定JSONを配置してください。",
                "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnRun.Enabled = false;
        }
    }

    private void BuildCategoryCheckList(string configPath)
    {
        var mapper = new CategoryMapper(configPath);
        var descriptions = mapper.CategoryDescriptions;
        var categories = mapper.Categories.Where(c => c != "その他").ToArray();

        pnlCategoryList.Controls.Clear();
        _categoryCheckBoxes.Clear();

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.White,
            Padding = new Padding(2)
        };

        foreach (var cat in categories)
        {
            var tooltip = descriptions?.GetValueOrDefault(cat, "") ?? "";
            var cb = new CheckBox
            {
                Text = cat,
                AutoSize = true,
                Checked = true,
                Font = new Font(Font.FontFamily, 8f),
                Margin = new Padding(1, 1, 1, 1)
            };
            flow.Controls.Add(cb);
            _categoryCheckBoxes.Add(cb);

            if (!string.IsNullOrEmpty(tooltip))
            {
                var tt = new ToolTip();
                tt.SetToolTip(cb, tooltip);
            }
        }

        pnlCategoryList.Controls.Add(flow);
    }

    private string[] GetActiveCategories()
    {
        return _categoryCheckBoxes
            .Where(cb => cb.Checked && cb.Enabled)
            .Select(cb => cb.Text)
            .ToArray();
    }

    private void BtnSelectFolder_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog();
        dialog.Description = "判定対象の画像フォルダを選択してください";
        if (!string.IsNullOrEmpty(txtFolderPath.Text) && Directory.Exists(txtFolderPath.Text))
            dialog.SelectedPath = txtFolderPath.Text;

        if (dialog.ShowDialog() == DialogResult.OK)
            txtFolderPath.Text = dialog.SelectedPath;
    }

    /// <summary>
    /// 1回目の判定実行。全カードクリアして最初から判定する。
    /// </summary>
    private async void BtnRun_Click(object? sender, EventArgs e)
    {
        if (_isRunning) return;
        if (_strategy == null || _configPath == null) return;

        if (string.IsNullOrWhiteSpace(txtFolderPath.Text))
        {
            BtnSelectFolder_Click(sender, e);
            if (string.IsNullOrWhiteSpace(txtFolderPath.Text)) return;
        }

        var folderPath = txtFolderPath.Text.Trim();
        if (!Directory.Exists(folderPath))
        {
            MessageBox.Show("指定されたフォルダが存在しません。", "エラー",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var activeCategories = GetActiveCategories();
        if (activeCategories.Length == 0)
        {
            MessageBox.Show("カテゴリを1つ以上選択してください。", "情報",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var imageFiles = Directory.GetFiles(folderPath)
            .Where(f => ImageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .OrderBy(f => f)
            .ToArray();

        if (imageFiles.Length == 0)
        {
            MessageBox.Show("対象フォルダに画像ファイルがありません。", "エラー",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // 全クリア（新規判定）
        _isRunning = true;
        btnRun.Enabled = false;
        btnReJudge.Visible = false;
        btnExportCsv.Enabled = false;
        _currentRound = 1;
        _usedCategories.Clear();

        foreach (Control c in photoGrid.Controls)
            c.Dispose();
        photoGrid.Controls.Clear();
        _results.Clear();
        _ngFlags.Clear();
        _imagePaths.Clear();

        progressBar.Minimum = 0;
        progressBar.Maximum = imageFiles.Length;
        progressBar.Value = 0;
        lblProgress.Text = $"0 / {imageFiles.Length} 枚";
        lblNgCount.Text = "NG: 0/0枚";
        lblStatus.Text = "判定中... (Round 1)";

        var strategy = _strategy;
        var configPath = _configPath;
        var modelPath = _modelPath;
        var selectedCategories = activeCategories;

        // 使用済みカテゴリを記録
        foreach (var cat in selectedCategories)
            _usedCategories.Add(cat);

        await Task.Run(() =>
        {
            using var pipeline = new JudgePipeline(modelPath, configPath);

            for (int i = 0; i < imageFiles.Length; i++)
            {
                var file = imageFiles[i];
                var result = pipeline.Judge(file, strategy, selectedCategories, 1);

                Invoke(() =>
                {
                    _results.Add(result);
                    _ngFlags[result] = false;
                    _imagePaths[result] = file;

                    var card = new PhotoCard(result, file);
                    card.NgChanged += (_, _) =>
                    {
                        _ngFlags[card.Result] = card.IsNg;
                        UpdateNgCount();
                    };
                    photoGrid.Controls.Add(card);

                    progressBar.Value = i + 1;
                    lblProgress.Text = $"{i + 1} / {imageFiles.Length} 枚";
                    lblStatus.Text = $"判定中: {i + 1}/{imageFiles.Length}枚 - {result.FileName}";
                });
            }
        });

        OnJudgeComplete();
    }

    /// <summary>
    /// その他再判定。「その他」かつNG以外の写真のみ再判定する。
    /// </summary>
    private async void BtnReJudge_Click(object? sender, EventArgs e)
    {
        if (_isRunning) return;
        if (_strategy == null || _configPath == null) return;

        // 新たに選択されたカテゴリ（Enabled=trueかつChecked=true）
        var newCategories = GetActiveCategories();
        if (newCategories.Length == 0)
        {
            MessageBox.Show("追加カテゴリを1つ以上選択してください。", "情報",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // 再判定対象: 「その他」かつNG以外
        var targets = new List<(int index, JudgeResult result, string imagePath)>();
        for (int i = 0; i < _results.Count; i++)
        {
            var r = _results[i];
            if (r.JudgedCategory == "その他" && !_ngFlags.GetValueOrDefault(r, false))
            {
                if (_imagePaths.TryGetValue(r, out var path))
                    targets.Add((i, r, path));
            }
        }

        if (targets.Count == 0)
        {
            MessageBox.Show("再判定対象の「その他」写真がありません。", "情報",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _isRunning = true;
        _currentRound++;
        btnRun.Enabled = false;
        btnReJudge.Visible = false;
        btnExportCsv.Enabled = false;

        // 使用済みカテゴリを記録
        foreach (var cat in newCategories)
            _usedCategories.Add(cat);

        // 再判定の有効カテゴリ = 今回新たに選択した分のみ
        var selectedCategories = newCategories;
        var round = _currentRound;

        progressBar.Minimum = 0;
        progressBar.Maximum = targets.Count;
        progressBar.Value = 0;
        lblStatus.Text = $"再判定中... (Round {round})";

        var strategy = _strategy;
        var configPath = _configPath;
        var modelPath = _modelPath;

        await Task.Run(() =>
        {
            using var pipeline = new JudgePipeline(modelPath, configPath);

            for (int t = 0; t < targets.Count; t++)
            {
                var (idx, oldResult, imagePath) = targets[t];
                var newResult = pipeline.Judge(imagePath, strategy, selectedCategories, round);

                Invoke(() =>
                {
                    // 結果を差し替え
                    var oldNg = _ngFlags.GetValueOrDefault(oldResult, false);
                    _ngFlags.Remove(oldResult);
                    _imagePaths.Remove(oldResult);

                    _results[idx] = newResult;
                    _ngFlags[newResult] = oldNg;
                    _imagePaths[newResult] = imagePath;

                    // カード更新
                    if (idx < photoGrid.Controls.Count && photoGrid.Controls[idx] is PhotoCard oldCard)
                    {
                        oldCard.UpdateResult(newResult);
                    }

                    progressBar.Value = t + 1;
                    lblProgress.Text = $"{t + 1} / {targets.Count} 枚 (再判定)";
                    lblStatus.Text = $"再判定中: {t + 1}/{targets.Count}枚 - {newResult.FileName}";
                });
            }
        });

        OnJudgeComplete();
    }

    /// <summary>
    /// 判定完了後の共通処理。
    /// </summary>
    private void OnJudgeComplete()
    {
        var otherCount = _results.Count(r => r.JudgedCategory == "その他" && !_ngFlags.GetValueOrDefault(r, false));
        var hasUnusedCategories = _categoryCheckBoxes.Any(cb => cb.Enabled && !cb.Checked)
                                 || _categoryCheckBoxes.Any(cb => !_usedCategories.Contains(cb.Text));

        lblStatus.Text = $"判定完了: {_results.Count}枚 (Round {_currentRound})"
                         + (otherCount > 0 ? $"  その他: {otherCount}枚" : "");
        lblProgress.Text = $"{_results.Count} / {_results.Count} 枚";
        UpdateNgCount();
        btnRun.Enabled = true;
        btnExportCsv.Enabled = true;
        _isRunning = false;

        // 使用済みカテゴリをグレーアウト
        foreach (var cb in _categoryCheckBoxes)
        {
            if (_usedCategories.Contains(cb.Text))
            {
                cb.Enabled = false;
                cb.Checked = true;
            }
        }

        // 「その他再判定」ボタンの表示条件:
        // 1. 「その他」の写真が1枚以上ある
        // 2. まだ選択されていないカテゴリが存在する
        var unusedExists = _categoryCheckBoxes.Any(cb => cb.Enabled);
        btnReJudge.Visible = otherCount > 0 && unusedExists;

        UpdateDefaultExportFileName();
    }

    private void UpdateDefaultExportFileName()
    {
        if (_results.Count == 0) return;
        var first = _results[0];
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var roundSuffix = _currentRound > 1 ? $"_R{_currentRound}" : "";
        txtExportFileName.Text = $"{first.StrategyName}_{first.CategoryConfigName}{roundSuffix}_{timestamp}";
    }

    private void UpdateNgCount()
    {
        var ngCount = _ngFlags.Values.Count(v => v);
        lblNgCount.Text = $"NG: {ngCount}/{_results.Count}枚";
    }

    private void BtnExportCsv_Click(object? sender, EventArgs e)
    {
        if (_results.Count == 0)
        {
            MessageBox.Show("判定結果がありません。先に判定を実行してください。", "情報",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var baseFileName = txtExportFileName.Text.Trim();
            if (string.IsNullOrWhiteSpace(baseFileName))
            {
                MessageBox.Show("書き出し予定ファイル名を入力してください。", "情報",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var ngList = _results.Select(r => _ngFlags.GetValueOrDefault(r, false)).ToList();

            var csvPath = CsvWriter.Write(_results, desktopPath, baseFileName, _version, _buildDate, ngList);
            ExportWriter.WriteImageFolder(_results, desktopPath, baseFileName);
            ExportWriter.WriteXlsx(_results, ngList, desktopPath, baseFileName);

            lblStatus.Text = $"出力完了: {baseFileName}";
            MessageBox.Show(
                $"デスクトップに出力しました:\n" +
                $"  {baseFileName}.csv\n" +
                $"  {baseFileName}.xlsx\n" +
                $"  {baseFileName}/（224×224画像）",
                "出力完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"出力に失敗しました:\n{ex.Message}", "エラー",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
