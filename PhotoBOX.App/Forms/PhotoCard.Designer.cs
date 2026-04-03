namespace PhotoBOX.App.Forms;

partial class PhotoCard
{
    private PictureBox picThumbnail;
    private Label lblCategory;
    private Label lblConfidence;
    private Label lblFileName;
    private CheckBox chkNg;

    private void InitializeComponent()
    {
        picThumbnail = new PictureBox();
        lblCategory = new Label();
        lblConfidence = new Label();
        lblFileName = new Label();
        chkNg = new CheckBox();

        SuspendLayout();

        // picThumbnail
        picThumbnail.Location = new Point(10, 10);
        picThumbnail.Size = new Size(180, 180);
        picThumbnail.SizeMode = PictureBoxSizeMode.Zoom;
        picThumbnail.BackColor = Color.White;

        // lblCategory
        lblCategory.Location = new Point(10, 195);
        lblCategory.Size = new Size(180, 20);
        lblCategory.Font = new Font(Font.FontFamily, 9f, FontStyle.Bold);
        lblCategory.TextAlign = ContentAlignment.MiddleCenter;

        // lblConfidence
        lblConfidence.Location = new Point(10, 215);
        lblConfidence.Size = new Size(180, 18);
        lblConfidence.TextAlign = ContentAlignment.MiddleCenter;
        lblConfidence.Font = new Font(Font.FontFamily, 8f);

        // lblFileName
        lblFileName.Location = new Point(10, 233);
        lblFileName.Size = new Size(180, 18);
        lblFileName.TextAlign = ContentAlignment.MiddleCenter;
        lblFileName.Font = new Font(Font.FontFamily, 7.5f);
        lblFileName.ForeColor = Color.DimGray;

        // chkNg
        chkNg.Location = new Point(75, 254);
        chkNg.Size = new Size(50, 20);
        chkNg.Text = "NG";
        chkNg.TextAlign = ContentAlignment.MiddleCenter;

        // PhotoCard
        Size = new Size(200, 280);
        MinimumSize = new Size(200, 280);
        MaximumSize = new Size(200, 280);
        Margin = new Padding(5);
        BorderStyle = BorderStyle.FixedSingle;

        Controls.Add(picThumbnail);
        Controls.Add(lblCategory);
        Controls.Add(lblConfidence);
        Controls.Add(lblFileName);
        Controls.Add(chkNg);

        ResumeLayout(false);
    }
}
