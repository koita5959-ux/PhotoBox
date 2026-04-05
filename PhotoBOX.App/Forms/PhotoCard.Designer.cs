namespace PhotoBOX.App.Forms;

partial class PhotoCard
{
    private PictureBox picThumbnail;
    private PictureBox picCropped;
    private Label lblCategory;
    private CheckBox chkNg;
    private Label lblConfidence;
    private Label lblPixelInfo;
    private Label lblFileName;

    private void InitializeComponent()
    {
        picThumbnail = new PictureBox();
        picCropped = new PictureBox();
        lblCategory = new Label();
        chkNg = new CheckBox();
        lblConfidence = new Label();
        lblPixelInfo = new Label();
        lblFileName = new Label();

        SuspendLayout();

        // ── 左: 元画像プレビュー ──
        picThumbnail.Location = new Point(5, 5);
        picThumbnail.Size = new Size(130, 110);
        picThumbnail.SizeMode = PictureBoxSizeMode.Zoom;
        picThumbnail.BackColor = Color.LightGray;

        // ── 中央: 分析情報 ──
        // カテゴリ名
        lblCategory.Location = new Point(140, 8);
        lblCategory.Size = new Size(130, 18);
        lblCategory.Font = new Font(Font.FontFamily, 9f, FontStyle.Bold);
        lblCategory.TextAlign = ContentAlignment.MiddleLeft;

        // NGチェック（カテゴリ名の右）
        chkNg.Location = new Point(270, 8);
        chkNg.Size = new Size(45, 18);
        chkNg.Text = "NG";
        chkNg.TextAlign = ContentAlignment.MiddleCenter;

        // 信頼度 + ファイルサイズ（1段目）
        lblConfidence.Location = new Point(140, 30);
        lblConfidence.Size = new Size(175, 16);
        lblConfidence.TextAlign = ContentAlignment.MiddleLeft;
        lblConfidence.Font = new Font(Font.FontFamily, 7.5f);

        // ピクセル数（2段目）
        lblPixelInfo.Location = new Point(140, 48);
        lblPixelInfo.Size = new Size(175, 16);
        lblPixelInfo.TextAlign = ContentAlignment.MiddleLeft;
        lblPixelInfo.Font = new Font(Font.FontFamily, 7.5f);

        // ── 右: クロップ判定画像（小） ──
        picCropped.Location = new Point(320, 8);
        picCropped.Size = new Size(55, 55);
        picCropped.SizeMode = PictureBoxSizeMode.Zoom;
        picCropped.BackColor = Color.LightGray;
        picCropped.BorderStyle = BorderStyle.FixedSingle;

        // ── 下部: ファイル名（全幅） ──
        lblFileName.Location = new Point(5, 120);
        lblFileName.Size = new Size(375, 16);
        lblFileName.TextAlign = ContentAlignment.MiddleLeft;
        lblFileName.Font = new Font(Font.FontFamily, 7f);
        lblFileName.ForeColor = Color.DimGray;

        // ── PhotoCard 本体 ──
        Size = new Size(385, 140);
        MinimumSize = new Size(385, 140);
        Margin = new Padding(5);
        BorderStyle = BorderStyle.FixedSingle;

        Controls.Add(picThumbnail);
        Controls.Add(lblCategory);
        Controls.Add(chkNg);
        Controls.Add(lblConfidence);
        Controls.Add(lblPixelInfo);
        Controls.Add(picCropped);
        Controls.Add(lblFileName);

        ResumeLayout(false);
    }
}
