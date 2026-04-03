using PhotoJudge.Core;
using PhotoJudge.Interfaces;
using PhotoBOX.App.Results;

namespace PhotoBOX.App.Forms;

public partial class MainForm : Form
{
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp"];

    private readonly string _baseDir;
    private readonly string _modelPath;
    private readonly string _configDir;
    private readonly string _resultsDir;

    private string _version = "0.0.0";
    private string _buildDate = "不明";
    private ICropStrategy? _strategy;
    private string? _configPath;

    private List<JudgeResult> _results = [];
    private readonly Dictionary<JudgeResult, bool> _ngFlags = [];

    private bool _isRunning;

    public MainForm()
    {
        InitializeComponent();

        _baseDir = AppContext.BaseDirectory;
        _modelPath = Path.Combine(_baseDir, "Models", "mobilenetv2-7.onnx");
        _configDir = Path.Combine(_baseDir, "Config");
        _resultsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "PhotoBOX", "results");

        // フォルダパスは空欄（判定実行時にダイアログで選択）
        txtFolderPath.Text = "";

        // イベント
        btnSelectFolder.Click += BtnSelectFolder_Click;
        btnRun.Click += BtnRun_Click;
        btnExportCsv.Click += BtnExportCsv_Click;

        Load += MainForm_Load;
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        // バージョン情報
        _version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0";
        var exePath = Environment.ProcessPath;
        _buildDate = exePath != null && File.Exists(exePath)
            ? File.GetLastWriteTime(exePath).ToString("yyyy-MM-dd HH:mm")
            : "不明";

        // 戦略・カテゴリ設定の検出
        var strategies = StrategyLoader.LoadAll();
        _strategy = strategies.Count > 0 ? strategies[0] : null;

        var configFiles = Directory.Exists(_configDir)
            ? Directory.GetFiles(_configDir, "*.json").OrderBy(f => f).ToList()
            : [];
        _configPath = configFiles.Count > 0 ? configFiles[0] : null;

        // タイトルバーにexe名を表示
        var exeName = Path.GetFileNameWithoutExtension(Environment.ProcessPath ?? "PhotoBOX");
        Text = $"PhotoBOX - {exeName}";

        // 仕様情報ラベル
        var strategyName = _strategy?.Name ?? "未検出";
        var configName = _configPath != null ? Path.GetFileNameWithoutExtension(_configPath) : "未検出";
        lblSpecLine1.Text = $"短辺：224px　長辺：元画像比率維持　戦略：{strategyName}";
        lblSpecLine2.Text = $"{_strategy?.Description ?? ""}　|　分類：{configName}";

        // 起動時の前提条件チェック
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

    private void BtnSelectFolder_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog();
        dialog.Description = "判定対象の画像フォルダを選択してください";
        if (!string.IsNullOrEmpty(txtFolderPath.Text) && Directory.Exists(txtFolderPath.Text))
            dialog.SelectedPath = txtFolderPath.Text;

        if (dialog.ShowDialog() == DialogResult.OK)
            txtFolderPath.Text = dialog.SelectedPath;
    }

    private async void BtnRun_Click(object? sender, EventArgs e)
    {
        if (_isRunning) return;
        if (_strategy == null || _configPath == null) return;

        // フォルダ未指定なら自動でダイアログを開く
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

        // UI準備
        _isRunning = true;
        btnRun.Enabled = false;
        btnExportCsv.Enabled = false;

        // 既存カードクリア
        foreach (Control c in photoGrid.Controls)
            c.Dispose();
        photoGrid.Controls.Clear();
        _results.Clear();
        _ngFlags.Clear();

        progressBar.Minimum = 0;
        progressBar.Maximum = imageFiles.Length;
        progressBar.Value = 0;
        lblNgCount.Text = $"NG: 0/0枚";
        lblStatus.Text = "判定中...";

        var strategy = _strategy;
        var configPath = _configPath;
        var modelPath = _modelPath;

        await Task.Run(() =>
        {
            using var pipeline = new JudgePipeline(modelPath, configPath);

            for (int i = 0; i < imageFiles.Length; i++)
            {
                var file = imageFiles[i];
                var result = pipeline.Judge(file, strategy);

                Invoke(() =>
                {
                    _results.Add(result);
                    _ngFlags[result] = false;

                    var card = new PhotoCard(result, file);
                    card.NgChanged += (_, _) =>
                    {
                        _ngFlags[card.Result] = card.IsNg;
                        UpdateNgCount();
                    };
                    photoGrid.Controls.Add(card);

                    progressBar.Value = i + 1;
                    lblStatus.Text = $"判定中: {i + 1}/{imageFiles.Length}枚 - {result.FileName}";
                });
            }
        });

        lblStatus.Text = $"判定完了: {_results.Count}枚";
        lblNgCount.Text = $"NG: 0/{_results.Count}枚";
        btnRun.Enabled = true;
        btnExportCsv.Enabled = true;
        _isRunning = false;
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
            var monitorName = txtMonitorName.Text.Trim();
            var ngList = _results.Select(r => _ngFlags.GetValueOrDefault(r, false)).ToList();
            var csvPath = CsvWriter.Write(_results, _resultsDir, _version, _buildDate, monitorName, ngList);

            lblStatus.Text = $"CSV出力完了: {csvPath}";
            MessageBox.Show($"CSVを出力しました:\n{csvPath}", "CSV出力完了",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"CSV出力に失敗しました:\n{ex.Message}", "エラー",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
