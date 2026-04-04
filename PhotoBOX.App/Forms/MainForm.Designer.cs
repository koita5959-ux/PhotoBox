namespace PhotoBOX.App.Forms;

partial class MainForm
{
    // SplitContainer (F6-01)
    private SplitContainer splitContainer;

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

    // 書き出し予定ファイル名 (F6-03: モニター名→ファイル名入力欄)
    private Label lblFileNameLabel;
    private TextBox txtExportFileName;
    private Button btnExportCsv;
    private Label lblNgCount;

    // 背景色凡例 (F6-10)
    private Label lblLegendOk;
    private Label lblLegendOther;
    private Label lblLegendNg;
    private Panel pnlLegendOkColor;
    private Panel pnlLegendOtherColor;
    private Panel pnlLegendNgColor;

    // ステータス
    private Label lblStatus;

    // 写真カード一覧
    private FlowLayoutPanel photoGrid;

    private void InitializeComponent()
    {
        splitContainer = new SplitContainer();
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
        lblFileNameLabel = new Label();
        txtExportFileName = new TextBox();
        btnExportCsv = new Button();
        lblNgCount = new Label();
        pnlLegendOkColor = new Panel();
        lblLegendOk = new Label();
        pnlLegendOtherColor = new Panel();
        lblLegendOther = new Label();
        pnlLegendNgColor = new Panel();
        lblLegendNg = new Label();
        lblStatus = new Label();
        photoGrid = new FlowLayoutPanel();

        SuspendLayout();
        splitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel2.SuspendLayout();
        rightPanel.SuspendLayout();

        var rpw = 280; // rightPanel width (F6-02: 拡大)
        var cw = rpw - 20; // content width (10px margin each side)
        var anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        // ── rightPanel ──
        rightPanel.Dock = DockStyle.Fill;
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
        btnSelectFolder.Location = new Point(rpw - 60, y - 1);
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
        var sepBottom = new Label();
        sepBottom.Location = new Point(10, y);
        sepBottom.Size = new Size(cw, 1);
        sepBottom.BackColor = SystemColors.ControlDark;
        sepBottom.Anchor = anchor;

        // ── 5. 書き出し予定ファイル名 (F6-03) ──
        y += 12;
        lblFileNameLabel.Text = "書き出し予定ファイル名";
        lblFileNameLabel.AutoSize = true;
        lblFileNameLabel.Location = new Point(10, y);
        lblFileNameLabel.Font = new Font(Font.FontFamily, 9f);
        lblFileNameLabel.ForeColor = SystemColors.GrayText;
        lblFileNameLabel.Anchor = anchor;

        y += 18;
        txtExportFileName.Location = new Point(10, y);
        txtExportFileName.Width = cw;
        txtExportFileName.Anchor = anchor;

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

        // ── 6.5 背景色凡例 (F6-10) ──
        y += 34;
        var sepLegend = new Label();
        sepLegend.Location = new Point(10, y);
        sepLegend.Size = new Size(cw, 1);
        sepLegend.BackColor = SystemColors.ControlDark;
        sepLegend.Anchor = anchor;

        y += 8;
        // 薄緑
        pnlLegendOkColor.Location = new Point(10, y);
        pnlLegendOkColor.Size = new Size(14, 14);
        pnlLegendOkColor.BackColor = Color.FromArgb(230, 255, 230);
        pnlLegendOkColor.BorderStyle = BorderStyle.FixedSingle;

        lblLegendOk.Text = "カテゴリ判定あり（その他以外）";
        lblLegendOk.AutoSize = true;
        lblLegendOk.Location = new Point(28, y);
        lblLegendOk.Font = new Font(Font.FontFamily, 8f);

        y += 18;
        // 薄黄
        pnlLegendOtherColor.Location = new Point(10, y);
        pnlLegendOtherColor.Size = new Size(14, 14);
        pnlLegendOtherColor.BackColor = Color.FromArgb(255, 255, 230);
        pnlLegendOtherColor.BorderStyle = BorderStyle.FixedSingle;

        lblLegendOther.Text = "その他";
        lblLegendOther.AutoSize = true;
        lblLegendOther.Location = new Point(28, y);
        lblLegendOther.Font = new Font(Font.FontFamily, 8f);

        y += 18;
        // 薄赤
        pnlLegendNgColor.Location = new Point(10, y);
        pnlLegendNgColor.Size = new Size(14, 14);
        pnlLegendNgColor.BackColor = Color.FromArgb(255, 230, 230);
        pnlLegendNgColor.BorderStyle = BorderStyle.FixedSingle;

        lblLegendNg.Text = "NG判定";
        lblLegendNg.AutoSize = true;
        lblLegendNg.Location = new Point(28, y);
        lblLegendNg.Font = new Font(Font.FontFamily, 8f);

        // ── 7. ステータス（右パネル最下部）──
        var sepStatus = new Label();
        sepStatus.Size = new Size(cw, 1);
        sepStatus.BackColor = SystemColors.ControlDark;
        sepStatus.Location = new Point(10, 0);
        sepStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        lblStatus.Text = "準備完了";
        lblStatus.AutoSize = false;
        lblStatus.Size = new Size(cw, 20);
        lblStatus.Location = new Point(10, 0);
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
        rightPanel.Controls.Add(lblFileNameLabel);
        rightPanel.Controls.Add(txtExportFileName);
        rightPanel.Controls.Add(btnExportCsv);
        rightPanel.Controls.Add(lblNgCount);
        rightPanel.Controls.Add(sepLegend);
        rightPanel.Controls.Add(pnlLegendOkColor);
        rightPanel.Controls.Add(lblLegendOk);
        rightPanel.Controls.Add(pnlLegendOtherColor);
        rightPanel.Controls.Add(lblLegendOther);
        rightPanel.Controls.Add(pnlLegendNgColor);
        rightPanel.Controls.Add(lblLegendNg);
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

        // ── SplitContainer (F6-01) ──
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel2;
        splitContainer.Orientation = Orientation.Vertical;
        splitContainer.Panel1MinSize = 300;
        splitContainer.Panel2MinSize = 250;
        splitContainer.SplitterWidth = 6;

        splitContainer.Panel1.Controls.Add(photoGrid);
        splitContainer.Panel2.Controls.Add(rightPanel);

        // ── MainForm (F6-02: 最小幅拡大) ──
        Text = "PhotoBOX";
        Size = new Size(1200, 700);
        MinimumSize = new Size(1050, 500);
        StartPosition = FormStartPosition.CenterScreen;

        Controls.Add(splitContainer);

        // SplitterDistance は Load 後に設定（FormのClientSizeが確定してから）
        Load += (s, e) =>
        {
            // 右パネル初期幅を280に設定
            splitContainer.SplitterDistance = ClientSize.Width - 280;
        };

        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        rightPanel.ResumeLayout(false);
        rightPanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
