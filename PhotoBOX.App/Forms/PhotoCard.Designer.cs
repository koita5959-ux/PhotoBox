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
        picThumbnail.Location = new Point(4, 4);
        picThumbnail.Size = new Size(95, 80);
        picThumbnail.SizeMode = PictureBoxSizeMode.Zoom;
        picThumbnail.BackColor = Color.LightGray;

        // ── 中央: 分析情報 ──
        lblCategory.Location = new Point(103, 4);
        lblCategory.Size = new Size(100, 16);
        lblCategory.Font = new Font(Font.FontFamily, 8.5f, FontStyle.Bold);
        lblCategory.TextAlign = ContentAlignment.MiddleLeft;

        chkNg.Location = new Point(203, 4);
        chkNg.Size = new Size(42, 16);
        chkNg.Text = "NG";
        chkNg.Font = new Font(Font.FontFamily, 7.5f);
        chkNg.TextAlign = ContentAlignment.MiddleCenter;

        lblConfidence.Location = new Point(103, 22);
        lblConfidence.Size = new Size(140, 14);
        lblConfidence.TextAlign = ContentAlignment.MiddleLeft;
        lblConfidence.Font = new Font(Font.FontFamily, 7f);

        lblPixelInfo.Location = new Point(103, 38);
        lblPixelInfo.Size = new Size(140, 14);
        lblPixelInfo.TextAlign = ContentAlignment.MiddleLeft;
        lblPixelInfo.Font = new Font(Font.FontFamily, 7f);

        // ── 右: クロップ判定画像（小） ──
        picCropped.Location = new Point(248, 4);
        picCropped.Size = new Size(48, 48);
        picCropped.SizeMode = PictureBoxSizeMode.Zoom;
        picCropped.BackColor = Color.LightGray;
        picCropped.BorderStyle = BorderStyle.FixedSingle;

        // ── 下部: ファイル名 ──
        lblFileName.Location = new Point(4, 88);
        lblFileName.Size = new Size(292, 14);
        lblFileName.TextAlign = ContentAlignment.MiddleLeft;
        lblFileName.Font = new Font(Font.FontFamily, 6.5f);
        lblFileName.ForeColor = Color.DimGray;

        // ── PhotoCard 本体 ──
        Size = new Size(300, 105);
        MinimumSize = new Size(300, 105);
        Margin = new Padding(4);
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
