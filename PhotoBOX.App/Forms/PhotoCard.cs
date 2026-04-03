using PhotoJudge.Core;

namespace PhotoBOX.App.Forms;

public partial class PhotoCard : UserControl
{
    private static readonly Color ColorOk = Color.FromArgb(230, 255, 230);
    private static readonly Color ColorOther = Color.FromArgb(255, 255, 230);
    private static readonly Color ColorNg = Color.FromArgb(255, 230, 230);

    private bool _isNg;

    public bool IsNg
    {
        get => _isNg;
        set
        {
            _isNg = value;
            chkNg.Checked = value;
            UpdateBackgroundColor();
            NgChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public JudgeResult Result { get; }

    public event EventHandler? NgChanged;

    public PhotoCard(JudgeResult result, string imagePath)
    {
        InitializeComponent();
        Result = result;

        lblCategory.Text = result.JudgedCategory;
        lblConfidence.Text = $"信頼度: {result.Confidence:F3}";
        lblFileName.Text = result.FileName;

        try
        {
            using var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            picThumbnail.Image = Image.FromStream(fs);
        }
        catch
        {
            // 画像読み込みに失敗した場合はデフォルト表示
        }

        UpdateBackgroundColor();

        chkNg.CheckedChanged += (_, _) =>
        {
            _isNg = chkNg.Checked;
            UpdateBackgroundColor();
            NgChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void UpdateBackgroundColor()
    {
        if (_isNg)
            BackColor = ColorNg;
        else if (Result.JudgedCategory == "その他")
            BackColor = ColorOther;
        else
            BackColor = ColorOk;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            picThumbnail.Image?.Dispose();
        }
        base.Dispose(disposing);
    }
}
