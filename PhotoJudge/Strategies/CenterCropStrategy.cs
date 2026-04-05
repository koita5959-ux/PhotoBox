using PhotoJudge.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PhotoJudge.Strategies;

/// <summary>
/// 画像の中心から短辺基準の正方形を切り出し、224x224にリサイズする。
/// 最も単純なベースライン戦略。
/// </summary>
public class CenterCropStrategy : ICropStrategy
{
    private const int TargetSize = 224;

    public string Name => "CenterCrop";
    public string Description => "画像中心から短辺基準の正方形を切り出す";

    public CropResult Crop(string imagePath)
    {
        using var image = Image.Load<Rgba32>(imagePath);

        int shortSide = Math.Min(image.Width, image.Height);
        int left = (image.Width - shortSide) / 2;
        int top = (image.Height - shortSide) / 2;
        var cropRegion = new Rectangle(left, top, shortSide, shortSide);

        int originalWidth = image.Width;
        int originalHeight = image.Height;

        var cropped = image.Clone(ctx =>
        {
            ctx.Crop(cropRegion);
            ctx.Resize(TargetSize, TargetSize, KnownResamplers.Lanczos3);
        });

        return new CropResult
        {
            CroppedImage = cropped,
            StrategyName = Name,
            SourcePath = imagePath,
            CropRegion = cropRegion,
            OriginalWidth = originalWidth,
            OriginalHeight = originalHeight
        };
    }
}
