namespace PhotoBOX.App.Forms;

partial class PhotoCard
{
    private PictureBox picThumbnail;
    private Label lblCategory;
    private CheckBox chkNg;
    private Label lblConfidence;
    private Label lblFileName;

    private void InitializeComponent()
    {
        picThumbnail = new PictureBox();
        lblCategory = new Label();
        chkNg = new CheckBox();
        lblConfidence = new Label();
        lblFileName = new Label();

        SuspendLayout();

        // picThumbnail (F6-04: BackColor=LightGray)
        picThumbnail.Location = new Point(5, 5);
        picThumbnail.Size = new Size(190, 170);
        picThumbnail.SizeMode = PictureBoxSizeMode.Zoom;
        picThumbnail.BackColor = Color.LightGray;

        // F6-07: 分類名とNGチェックボックスを横並び
        // lblCategory
        lblCategory.Location = new Point(5, 178);
        lblCategory.Size = new Size(140, 18);
        lblCategory.Font = new Font(Font.FontFamily, 9f, FontStyle.Bold);
        lblCategory.TextAlign = ContentAlignment.MiddleLeft;

        // chkNg (F6-07: 分類名の右隣)
        chkNg.Location = new Point(145, 178);
        chkNg.Size = new Size(50, 18);
        chkNg.Text = "NG";
        chkNg.TextAlign = ContentAlignment.MiddleCenter;

        // lblConfidence (F6-06: ファイルサイズ・ピクセル数追加は PhotoCard.cs で設定)
        lblConfidence.Location = new Point(5, 197);
        lblConfidence.Size = new Size(190, 30);
        lblConfidence.TextAlign = ContentAlignment.TopLeft;
        lblConfidence.Font = new Font(Font.FontFamily, 7.5f);

        // lblFileName
        lblFileName.Location = new Point(5, 228);
        lblFileName.Size = new Size(190, 16);
        lblFileName.TextAlign = ContentAlignment.MiddleCenter;
        lblFileName.Font = new Font(Font.FontFamily, 7f);
        lblFileName.ForeColor = Color.DimGray;

        // PhotoCard (F6-05: 高さ成り行き — 固定サイズ廃止)
        Size = new Size(200, 248);
        MinimumSize = new Size(200, 0);
        MaximumSize = new Size(200, 0);
        Margin = new Padding(5);
        BorderStyle = BorderStyle.FixedSingle;

        Controls.Add(picThumbnail);
        Controls.Add(lblCategory);
        Controls.Add(chkNg);
        Controls.Add(lblConfidence);
        Controls.Add(lblFileName);

        ResumeLayout(false);
    }
}
