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
    // 各画像の元ファイルパスを保持（3点セット出力用）
    private readonly Dictionary<JudgeResult, string> _imagePaths = [];

    private bool _isRunning;

    public MainForm()
    {
        InitializeComponent();

        _baseDir = AppContext.BaseDirectory;
        _modelPath = Path.Combine(_baseDir, "Models", "mobilenetv2-7.onnx");
        _configDir = Path.Combine(_baseDir, "Config");

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
        lblSpecTitle.Text = strategyName;
        lblSpecParams.Text = "短辺：224px　長辺：元画像比率維持";
        lblSpecDesc.Text = _strategy?.Description ?? "";
        lblSpecCategory.Text = $"分類：{configName}";

        // カテゴリ一覧をConfigから読み取り表示
        if (_configPath != null)
        {
            var mapper = new CategoryMapper(_configPath);
            var activeCategories = mapper.Categories.Where(c => c != "その他").ToArray();
            lblSpecCategories.Text = string.Join(" / ", activeCategories) + "  + その他";
        }

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
        _imagePaths.Clear();

        progressBar.Minimum = 0;
        progressBar.Maximum = imageFiles.Length;
        progressBar.Value = 0;
        lblProgress.Text = $"0 / {imageFiles.Length} 枚";
        lblNgCount.Text = "NG: 0/0枚";
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

        lblStatus.Text = $"判定完了: {_results.Count}枚";
        lblProgress.Text = $"{_results.Count} / {_results.Count} 枚";
        lblNgCount.Text = $"NG: 0/{_results.Count}枚";
        btnRun.Enabled = true;
        btnExportCsv.Enabled = true;
        _isRunning = false;

        // F6-03: 判定実行後にデフォルトファイル名を自動生成
        UpdateDefaultExportFileName();
    }

    /// <summary>
    /// F6-03: デフォルトの書き出し予定ファイル名を生成してテキストボックスに設定
    /// </summary>
    private void UpdateDefaultExportFileName()
    {
        if (_results.Count == 0) return;
        var first = _results[0];
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        txtExportFileName.Text = $"{first.StrategyName}_{first.CategoryConfigName}_{timestamp}";
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
            // F6-03: ファイル名入力欄の値をベースファイル名として使用
            var baseFileName = txtExportFileName.Text.Trim();
            if (string.IsNullOrWhiteSpace(baseFileName))
            {
                MessageBox.Show("書き出し予定ファイル名を入力してください。", "情報",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // F6-09: デスクトップ直下に出力
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var ngList = _results.Select(r => _ngFlags.GetValueOrDefault(r, false)).ToList();

            // 3点セット出力
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
