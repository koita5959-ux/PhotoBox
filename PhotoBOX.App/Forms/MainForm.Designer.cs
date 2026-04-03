namespace PhotoBOX.App.Forms;

partial class MainForm
{
    // 右パネル
    private Panel rightPanel;

    // フォルダ選択
    private Label lblFolderLabel;
    private TextBox txtFolderPath;
    private Button btnSelectFolder;

    // 判定実行
    private Button btnRun;
    private ProgressBar progressBar;
    private Label lblProgress;

    // 仕様情報
    private Label lblSpecTitle;
    private Label lblSpecParams;
    private Label lblSpecDesc;
    private Label lblSpecCategory;

    // モニター名・CSV・NG
    private Label lblMonitorLabel;
    private TextBox txtMonitorName;
    private Button btnExportCsv;
    private Label lblNgCount;

    // ステータス
    private Label lblStatus;

    // 写真カード一覧
    private FlowLayoutPanel photoGrid;

    private void InitializeComponent()
    {
        rightPanel = new Panel();
        lblFolderLabel = new Label();
        txtFolderPath = new TextBox();
        btnSelectFolder = new Button();
        btnRun = new Button();
        progressBar = new ProgressBar();
        lblProgress = new Label();
        lblSpecTitle = new Label();
        lblSpecParams = new Label();
        lblSpecDesc = new Label();
        lblSpecCategory = new Label();
        lblMonitorLabel = new Label();
        txtMonitorName = new TextBox();
        btnExportCsv = new Button();
        lblNgCount = new Label();
        lblStatus = new Label();
        photoGrid = new FlowLayoutPanel();

        SuspendLayout();
        rightPanel.SuspendLayout();

        var pw = 210; // rightPanel width
        var cw = pw - 20; // content width (10px margin each side)
        var anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // ── rightPanel ──
        rightPanel.Dock = DockStyle.Right;
        rightPanel.Width = pw;
        rightPanel.BackColor = SystemColors.Control;
        rightPanel.Padding = new Padding(10);
        rightPanel.Paint += (s, e) =>
        {
            using var pen = new Pen(SystemColors.ControlDark, 1);
            e.Graphics.DrawLine(pen, 0, 0, 0, rightPanel.Height);
        };

        // ── 1. フォルダ選択 (Y=10) ──
        int y = 10;

        lblFolderLabel.Text = "フォルダ";
        lblFolderLabel.AutoSize = true;
        lblFolderLabel.Location = new Point(10, y);
        lblFolderLabel.Font = new Font(Font.FontFamily, 9f);
        lblFolderLabel.ForeColor = SystemColors.GrayText;
        lblFolderLabel.Anchor = anchor;

        y += 18;
        txtFolderPath.Location = new Point(10, y);
        txtFolderPath.Width = cw - 55;
        txtFolderPath.Anchor = anchor;

        btnSelectFolder.Text = "選択";
        btnSelectFolder.Location = new Point(pw - 60, y - 1);
        btnSelectFolder.Size = new Size(50, 23);
        btnSelectFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        // ── 2. 判定実行ボタン ──
        y += 30;
        btnRun.Text = "判定実行";
        btnRun.Location = new Point(10, y);
        btnRun.Size = new Size(cw, 30);
        btnRun.Anchor = anchor;

        // ── 3. プログレスバー ──
        y += 38;
        progressBar.Location = new Point(10, y);
        progressBar.Size = new Size(cw, 18);
        progressBar.Anchor = anchor;

        y += 20;
        lblProgress.Text = "0 / 0 枚";
        lblProgress.Location = new Point(10, y);
        lblProgress.Size = new Size(cw, 16);
        lblProgress.Font = new Font(Font.FontFamily, 9f);
        lblProgress.TextAlign = ContentAlignment.MiddleCenter;
        lblProgress.Anchor = anchor;

        // ── 4. 仕様情報 ──
        y += 24;

        // 上区切り線（Labelで代用）
        var sepTop = new Label();
        sepTop.Location = new Point(10, y);
        sepTop.Size = new Size(cw, 1);
        sepTop.BackColor = SystemColors.ControlDark;
        sepTop.Anchor = anchor;

        y += 8;
        lblSpecTitle.Location = new Point(10, y);
        lblSpecTitle.AutoSize = true;
        lblSpecTitle.Font = new Font(Font.FontFamily, 9f, FontStyle.Bold);
        lblSpecTitle.Anchor = anchor;

        y += 20;
        lblSpecParams.Location = new Point(10, y);
        lblSpecParams.AutoSize = true;
        lblSpecParams.Font = new Font(Font.FontFamily, 8.5f);
        lblSpecParams.Anchor = anchor;

        y += 18;
        lblSpecDesc.Location = new Point(10, y);
        lblSpecDesc.Size = new Size(cw, 32);
        lblSpecDesc.Font = new Font(Font.FontFamily, 8.5f);
        lblSpecDesc.ForeColor = SystemColors.GrayText;
        lblSpecDesc.Anchor = anchor;

        y += 34;
        lblSpecCategory.Location = new Point(10, y);
        lblSpecCategory.AutoSize = true;
        lblSpecCategory.Font = new Font(Font.FontFamily, 8.5f);
        lblSpecCategory.Anchor = anchor;

        y += 20;
        // 下区切り線
        var sepBottom = new Label();
        sepBottom.Location = new Point(10, y);
        sepBottom.Size = new Size(cw, 1);
        sepBottom.BackColor = SystemColors.ControlDark;
        sepBottom.Anchor = anchor;

        // ── 5. モニター名 ──
        y += 12;
        lblMonitorLabel.Text = "モニター名";
        lblMonitorLabel.AutoSize = true;
        lblMonitorLabel.Location = new Point(10, y);
        lblMonitorLabel.Font = new Font(Font.FontFamily, 9f);
        lblMonitorLabel.ForeColor = SystemColors.GrayText;
        lblMonitorLabel.Anchor = anchor;

        y += 18;
        txtMonitorName.Location = new Point(10, y);
        txtMonitorName.Width = cw;
        txtMonitorName.Anchor = anchor;

        // ── 6. CSV出力 + NG件数 ──
        y += 30;
        btnExportCsv.Text = "CSV出力";
        btnExportCsv.Location = new Point(10, y);
        btnExportCsv.Size = new Size(90, 26);
        btnExportCsv.Anchor = AnchorStyles.Top | AnchorStyles.Left;

        lblNgCount.Text = "NG: 0/0枚";
        lblNgCount.AutoSize = true;
        lblNgCount.Location = new Point(105, y + 5);
        lblNgCount.Font = new Font(Font.FontFamily, 9f, FontStyle.Bold);
        lblNgCount.ForeColor = Color.FromArgb(226, 75, 74);
        lblNgCount.Anchor = AnchorStyles.Top | AnchorStyles.Left;

        // ── 7. ステータス（右パネル最下部）──
        // 区切り線
        var sepStatus = new Label();
        sepStatus.Size = new Size(cw, 1);
        sepStatus.BackColor = SystemColors.ControlDark;
        sepStatus.Location = new Point(10, 0); // Y is set by anchor
        sepStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        lblStatus.Text = "準備完了";
        lblStatus.AutoSize = false;
        lblStatus.Size = new Size(cw, 20);
        lblStatus.Location = new Point(10, 0); // Y is set by anchor
        lblStatus.Font = new Font(Font.FontFamily, 9f);
        lblStatus.ForeColor = SystemColors.GrayText;
        lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        // 右パネルにコントロール追加
        rightPanel.Controls.Add(lblFolderLabel);
        rightPanel.Controls.Add(txtFolderPath);
        rightPanel.Controls.Add(btnSelectFolder);
        rightPanel.Controls.Add(btnRun);
        rightPanel.Controls.Add(progressBar);
        rightPanel.Controls.Add(lblProgress);
        rightPanel.Controls.Add(sepTop);
        rightPanel.Controls.Add(lblSpecTitle);
        rightPanel.Controls.Add(lblSpecParams);
        rightPanel.Controls.Add(lblSpecDesc);
        rightPanel.Controls.Add(lblSpecCategory);
        rightPanel.Controls.Add(sepBottom);
        rightPanel.Controls.Add(lblMonitorLabel);
        rightPanel.Controls.Add(txtMonitorName);
        rightPanel.Controls.Add(btnExportCsv);
        rightPanel.Controls.Add(lblNgCount);
        rightPanel.Controls.Add(sepStatus);
        rightPanel.Controls.Add(lblStatus);

        // ステータスを最下部に配置（パネルの高さから逆算）
        rightPanel.Resize += (s, e) =>
        {
            sepStatus.Top = rightPanel.ClientSize.Height - 28;
            lblStatus.Top = rightPanel.ClientSize.Height - 24;
        };

        // ── photoGrid ──
        photoGrid.Dock = DockStyle.Fill;
        photoGrid.AutoScroll = true;
        photoGrid.WrapContents = true;
        photoGrid.BackColor = Color.WhiteSmoke;
        photoGrid.Padding = new Padding(10, 10, 0, 10);

        // ── MainForm ──
        Text = "PhotoBOX";
        Size = new Size(1200, 700);
        MinimumSize = new Size(900, 500);
        StartPosition = FormStartPosition.CenterScreen;

        // Add order: rightPanel (Right) first, then photoGrid (Fill)
        Controls.Add(photoGrid);
        Controls.Add(rightPanel);

        rightPanel.ResumeLayout(false);
        rightPanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
