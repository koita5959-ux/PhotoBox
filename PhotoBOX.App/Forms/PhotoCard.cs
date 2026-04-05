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
        lblFileName.Text = result.FileName;

        // F6-06: 信頼度 + ファイルサイズ + ピクセル数
        var confidenceText = $"信頼度:{result.Confidence:F3}";
        var fileSizeText = "";
        var pixelText = "";

        try
        {
            var fi = new FileInfo(imagePath);
            fileSizeText = FormatFileSize(fi.Length);
        }
        catch { /* ファイルアクセス失敗時はスキップ */ }

        // F6-08: .webp対応を含む画像読み込み
        try
        {
            using var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            using var original = Image.FromStream(fs);
            picThumbnail.Image = new Bitmap(original);
            pixelText = $"{original.Width}×{original.Height}";
        }
        catch
        {
            // GDI+で読めない場合（webp等）→ ImageSharpで読み込み
            try
            {
                using var img = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(imagePath);
                pixelText = $"{img.Width}×{img.Height}";
                using var ms = new MemoryStream();
                img.Save(ms, new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder());
                ms.Position = 0;
                picThumbnail.Image = new Bitmap(ms);
            }
            catch
            {
                pixelText = "";
            }
        }

        // 信頼度行の組み立て
        var parts = new List<string> { confidenceText };
        if (!string.IsNullOrEmpty(fileSizeText)) parts.Add(fileSizeText);
        if (!string.IsNullOrEmpty(pixelText)) parts.Add(pixelText);
        lblConfidence.Text = string.Join(" / ", parts);

        UpdateBackgroundColor();

        chkNg.CheckedChanged += (_, _) =>
        {
            _isNg = chkNg.Checked;
            UpdateBackgroundColor();
            NgChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes >= 1024 * 1024)
            return $"{bytes / (1024.0 * 1024.0):F1}MB";
        return $"{bytes / 1024.0:F0}KB";
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
