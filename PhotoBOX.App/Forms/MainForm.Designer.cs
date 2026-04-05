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
    private Label lblSpecCategories;

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
        lblSpecCategories = new Label();
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

        const int margin = 12; // 左右余白

        // ── rightPanel ──
        rightPanel.Dock = DockStyle.Fill;
        rightPanel.BackColor = SystemColors.Control;
        rightPanel.Paint += (s, e) =>
        {
            using var pen = new Pen(SystemColors.ControlDark, 1);
            e.Graphics.DrawLine(pen, 0, 0, 0, rightPanel.Height);
        };

        // 幅可変コントロールをResizeで再配置するヘルパー
        var stretchControls = new List<Control>();
        var sepControls = new List<Label>();

        // ── 1. フォルダ選択 (Y=10) ──
        int y = margin;

        lblFolderLabel.Text = "フォルダ";
        lblFolderLabel.AutoSize = true;
        lblFolderLabel.Location = new Point(margin, y);
        lblFolderLabel.Font = new Font(Font.FontFamily, 9f);
        lblFolderLabel.ForeColor = SystemColors.GrayText;

        y += 18;
        txtFolderPath.Location = new Point(margin, y);
        txtFolderPath.Width = 100; // Resizeで再計算
        stretchControls.Add(txtFolderPath);

        btnSelectFolder.Text = "選択";
        btnSelectFolder.Size = new Size(50, 23);
        btnSelectFolder.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        // ── 2. 判定実行ボタン（固定幅） ──
        y += 30;
        btnRun.Text = "判定実行";
        btnRun.Location = new Point(margin, y);
        btnRun.Size = new Size(120, 28);

        // ── 3. プログレスバー ──
        y += 36;
        progressBar.Location = new Point(margin, y);
        progressBar.Size = new Size(100, 18); // Resizeで再計算
        stretchControls.Add(progressBar);

        y += 20;
        lblProgress.Text = "0 / 0 枚";
        lblProgress.Location = new Point(margin, y);
        lblProgress.Size = new Size(100, 16); // Resizeで再計算
        lblProgress.Font = new Font(Font.FontFamily, 9f);
        lblProgress.TextAlign = ContentAlignment.MiddleCenter;
        stretchControls.Add(lblProgress);

        // ── 4. 仕様情報 ──
        y += 24;

        var sepTop = new Label();
        sepTop.Location = new Point(margin, y);
        sepTop.Size = new Size(100, 1);
        sepTop.BackColor = SystemColors.ControlDark;
        sepControls.Add(sepTop);

        y += 8;
        lblSpecTitle.Location = new Point(margin, y);
        lblSpecTitle.AutoSize = true;
        lblSpecTitle.Font = new Font(Font.FontFamily, 9f, FontStyle.Bold);

        y += 20;
        lblSpecParams.Location = new Point(margin, y);
        lblSpecParams.AutoSize = true;
        lblSpecParams.Font = new Font(Font.FontFamily, 8.5f);

        y += 18;
        lblSpecDesc.Location = new Point(margin, y);
        lblSpecDesc.Size = new Size(100, 32); // Resizeで再計算
        lblSpecDesc.Font = new Font(Font.FontFamily, 8.5f);
        lblSpecDesc.ForeColor = SystemColors.GrayText;
        stretchControls.Add(lblSpecDesc);

        y += 34;
        lblSpecCategory.Location = new Point(margin, y);
        lblSpecCategory.AutoSize = true;
        lblSpecCategory.Font = new Font(Font.FontFamily, 8.5f);

        y += 20;
        lblSpecCategories.Location = new Point(margin + 8, y);
        lblSpecCategories.AutoSize = true;
        lblSpecCategories.Font = new Font(Font.FontFamily, 8.5f);
        lblSpecCategories.ForeColor = SystemColors.GrayText;
        lblSpecCategories.Text = "";

        y += 20;
        var sepBottom = new Label();
        sepBottom.Location = new Point(margin, y);
        sepBottom.Size = new Size(100, 1);
        sepBottom.BackColor = SystemColors.ControlDark;
        sepControls.Add(sepBottom);

        // ── 5. 書き出し予定ファイル名 (F6-03) ──
        y += 12;
        lblFileNameLabel.Text = "書き出し予定ファイル名";
        lblFileNameLabel.AutoSize = true;
        lblFileNameLabel.Location = new Point(margin, y);
        lblFileNameLabel.Font = new Font(Font.FontFamily, 9f);
        lblFileNameLabel.ForeColor = SystemColors.GrayText;

        y += 18;
        txtExportFileName.Location = new Point(margin, y);
        txtExportFileName.Width = 100; // Resizeで再計算
        stretchControls.Add(txtExportFileName);

        // ── 6. CSV出力 + NG件数 ──
        y += 30;
        btnExportCsv.Text = "CSV出力";
        btnExportCsv.Location = new Point(margin, y);
        btnExportCsv.Size = new Size(90, 26);

        lblNgCount.Text = "NG: 0/0枚";
        lblNgCount.AutoSize = true;
        lblNgCount.Location = new Point(margin + 95, y + 5);
        lblNgCount.Font = new Font(Font.FontFamily, 9f, FontStyle.Bold);
        lblNgCount.ForeColor = Color.FromArgb(226, 75, 74);

        // ── 6.5 背景色凡例 (F6-10) ──
        y += 34;
        var sepLegend = new Label();
        sepLegend.Location = new Point(margin, y);
        sepLegend.Size = new Size(100, 1);
        sepLegend.BackColor = SystemColors.ControlDark;
        sepControls.Add(sepLegend);

        y += 8;
        // 薄緑
        pnlLegendOkColor.Location = new Point(margin, y);
        pnlLegendOkColor.Size = new Size(14, 14);
        pnlLegendOkColor.BackColor = Color.FromArgb(230, 255, 230);
        pnlLegendOkColor.BorderStyle = BorderStyle.FixedSingle;

        lblLegendOk.Text = "カテゴリ判定あり（その他以外）";
        lblLegendOk.AutoSize = true;
        lblLegendOk.Location = new Point(margin + 18, y);
        lblLegendOk.Font = new Font(Font.FontFamily, 8f);

        y += 18;
        // 薄黄
        pnlLegendOtherColor.Location = new Point(margin, y);
        pnlLegendOtherColor.Size = new Size(14, 14);
        pnlLegendOtherColor.BackColor = Color.FromArgb(255, 255, 230);
        pnlLegendOtherColor.BorderStyle = BorderStyle.FixedSingle;

        lblLegendOther.Text = "その他";
        lblLegendOther.AutoSize = true;
        lblLegendOther.Location = new Point(margin + 18, y);
        lblLegendOther.Font = new Font(Font.FontFamily, 8f);

        y += 18;
        // 薄赤
        pnlLegendNgColor.Location = new Point(margin, y);
        pnlLegendNgColor.Size = new Size(14, 14);
        pnlLegendNgColor.BackColor = Color.FromArgb(255, 230, 230);
        pnlLegendNgColor.BorderStyle = BorderStyle.FixedSingle;

        lblLegendNg.Text = "NG判定";
        lblLegendNg.AutoSize = true;
        lblLegendNg.Location = new Point(margin + 18, y);
        lblLegendNg.Font = new Font(Font.FontFamily, 8f);

        // ── 7. ステータス（右パネル最下部）──
        var sepStatus = new Label();
        sepStatus.Size = new Size(100, 1);
        sepStatus.BackColor = SystemColors.ControlDark;
        sepStatus.Location = new Point(margin, 0);
        sepControls.Add(sepStatus);

        lblStatus.Text = "準備完了";
        lblStatus.AutoSize = false;
        lblStatus.Size = new Size(100, 20);
        lblStatus.Location = new Point(margin, 0);
        lblStatus.Font = new Font(Font.FontFamily, 9f);
        lblStatus.ForeColor = SystemColors.GrayText;
        stretchControls.Add(lblStatus);

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
        rightPanel.Controls.Add(lblSpecCategories);
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

        // Resizeで幅可変コントロールとステータス位置を再計算
        rightPanel.Resize += (s, e) =>
        {
            var cw = rightPanel.ClientSize.Width - margin * 2;
            if (cw < 50) return;

            foreach (var ctrl in stretchControls)
                ctrl.Width = cw;
            foreach (var sep in sepControls)
                sep.Width = cw;

            // フォルダTextBoxは「選択」ボタン分を引く
            txtFolderPath.Width = cw - 55;
            btnSelectFolder.Location = new Point(rightPanel.ClientSize.Width - margin - 50, txtFolderPath.Top - 1);

            // ステータスを最下部に配置
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
        splitContainer.SplitterWidth = 6;
        // MinSizeはLoad後に設定（EndInit時の制約違反を回避）

        splitContainer.Panel1.Controls.Add(photoGrid);
        splitContainer.Panel2.Controls.Add(rightPanel);

        // ── MainForm (F6-02: 最小幅拡大) ──
        Text = "PhotoBOX";
        Size = new Size(1200, 700);
        MinimumSize = new Size(1050, 500);
        StartPosition = FormStartPosition.CenterScreen;

        Controls.Add(splitContainer);

        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();

        // Load後にサイズ確定してからMinSizeとSplitterDistanceを設定
        Load += (s, e) =>
        {
            splitContainer.SplitterDistance = ClientSize.Width - 280;
            splitContainer.Panel1MinSize = 300;
            splitContainer.Panel2MinSize = 250;
        };
        splitContainer.ResumeLayout(false);
        rightPanel.ResumeLayout(false);
        rightPanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
