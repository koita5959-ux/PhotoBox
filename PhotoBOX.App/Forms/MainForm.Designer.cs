namespace PhotoBOX.App.Forms;

partial class MainForm
{
    private Panel topPanel;
    private Label lblFolder;
    private TextBox txtFolderPath;
    private Button btnSelectFolder;
    private Button btnRun;
    private ProgressBar progressBar;
    private Label lblSpecInfo;

    private Panel midPanel;
    private Label lblMonitor;
    private TextBox txtMonitorName;
    private Button btnExportCsv;
    private Label lblNgCount;

    private FlowLayoutPanel photoGrid;

    private StatusStrip statusBar;
    private ToolStripStatusLabel lblStatus;

    private void InitializeComponent()
    {
        topPanel = new Panel();
        lblFolder = new Label();
        txtFolderPath = new TextBox();
        btnSelectFolder = new Button();
        btnRun = new Button();
        progressBar = new ProgressBar();
        lblSpecInfo = new Label();

        midPanel = new Panel();
        lblMonitor = new Label();
        txtMonitorName = new TextBox();
        btnExportCsv = new Button();
        lblNgCount = new Label();

        photoGrid = new FlowLayoutPanel();

        statusBar = new StatusStrip();
        lblStatus = new ToolStripStatusLabel();

        SuspendLayout();
        topPanel.SuspendLayout();
        midPanel.SuspendLayout();

        // ── topPanel ──
        topPanel.Dock = DockStyle.Top;
        topPanel.Height = 40;
        topPanel.Padding = new Padding(5, 5, 5, 5);

        lblFolder.Text = "フォルダ:";
        lblFolder.AutoSize = true;
        lblFolder.Location = new Point(8, 12);

        txtFolderPath.Location = new Point(70, 9);
        txtFolderPath.Width = 320;

        btnSelectFolder.Text = "選択";
        btnSelectFolder.Location = new Point(396, 7);
        btnSelectFolder.Size = new Size(50, 26);

        btnRun.Text = "判定実行";
        btnRun.Location = new Point(452, 7);
        btnRun.Size = new Size(75, 26);

        progressBar.Location = new Point(533, 10);
        progressBar.Size = new Size(100, 22);

        lblSpecInfo.AutoSize = true;
        lblSpecInfo.Location = new Point(640, 12);
        lblSpecInfo.ForeColor = Color.DimGray;
        lblSpecInfo.Font = new Font(Font.FontFamily, 8f);

        topPanel.Controls.Add(lblFolder);
        topPanel.Controls.Add(txtFolderPath);
        topPanel.Controls.Add(btnSelectFolder);
        topPanel.Controls.Add(btnRun);
        topPanel.Controls.Add(progressBar);
        topPanel.Controls.Add(lblSpecInfo);

        // ── midPanel ──
        midPanel.Dock = DockStyle.Top;
        midPanel.Height = 30;
        midPanel.Padding = new Padding(5, 2, 5, 2);

        lblMonitor.Text = "モニター名:";
        lblMonitor.AutoSize = true;
        lblMonitor.Location = new Point(8, 7);

        txtMonitorName.Location = new Point(80, 4);
        txtMonitorName.Width = 150;

        btnExportCsv.Text = "CSV出力";
        btnExportCsv.Location = new Point(240, 2);
        btnExportCsv.Size = new Size(70, 24);

        lblNgCount.Text = "NG: 0/0枚";
        lblNgCount.AutoSize = true;
        lblNgCount.Location = new Point(320, 7);
        lblNgCount.Font = new Font(Font.FontFamily, 9f, FontStyle.Bold);

        midPanel.Controls.Add(lblMonitor);
        midPanel.Controls.Add(txtMonitorName);
        midPanel.Controls.Add(btnExportCsv);
        midPanel.Controls.Add(lblNgCount);

        // ── photoGrid ──
        photoGrid.Dock = DockStyle.Fill;
        photoGrid.AutoScroll = true;
        photoGrid.WrapContents = true;
        photoGrid.BackColor = Color.WhiteSmoke;
        photoGrid.Padding = new Padding(5);

        // ── statusBar ──
        statusBar.Dock = DockStyle.Bottom;
        lblStatus.Text = "準備完了";
        lblStatus.Spring = true;
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        statusBar.Items.Add(lblStatus);

        // ── MainForm ──
        Text = "PhotoBOX";
        Size = new Size(960, 640);
        MinimumSize = new Size(800, 500);
        StartPosition = FormStartPosition.CenterScreen;

        // Add order matters: statusBar first (Bottom), then topPanel/midPanel (Top), then photoGrid (Fill)
        Controls.Add(photoGrid);
        Controls.Add(midPanel);
        Controls.Add(topPanel);
        Controls.Add(statusBar);

        topPanel.ResumeLayout(false);
        topPanel.PerformLayout();
        midPanel.ResumeLayout(false);
        midPanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
