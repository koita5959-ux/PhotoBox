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

        // 1段目: 信頼度 + ファイルサイズ
        var confidenceText = $"信頼度:{result.Confidence:F3}";
        var fileSizeText = FormatFileSize(result.FileSize);
        lblConfidence.Text = $"{confidenceText} / {fileSizeText}";

        // 2段目: ピクセル数
        lblPixelInfo.Text = $"{result.OriginalWidth}×{result.OriginalHeight}";

        // 元画像プレビュー（左）
        try
        {
            using var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            using var original = Image.FromStream(fs);
            picThumbnail.Image = new Bitmap(original);
        }
        catch
        {
            // GDI+で読めない場合（webp等）→ ImageSharpで読み込み
            try
            {
                using var img = SixLabors.ImageSharp.Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(imagePath);
                using var ms = new MemoryStream();
                img.Save(ms, new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder());
                ms.Position = 0;
                picThumbnail.Image = new Bitmap(ms);
            }
            catch { }
        }

        // クロップ判定画像（右）
        try
        {
            using var ms = new MemoryStream(result.CroppedImageJpeg);
            picCropped.Image = new Bitmap(ms);
        }
        catch { }

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
            picCropped.Image?.Dispose();
        }
        base.Dispose(disposing);
    }
}
