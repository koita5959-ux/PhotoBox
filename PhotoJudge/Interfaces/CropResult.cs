using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PhotoJudge.Interfaces;

public class CropResult : IDisposable
{
    /// <summary>224x224に切り出された画像</summary>
    public required Image<Rgba32> CroppedImage { get; init; }

    /// <summary>使用した戦略名</summary>
    public required string StrategyName { get; init; }

    /// <summary>元画像のファイルパス</summary>
    public required string SourcePath { get; init; }

    /// <summary>元画像上の切り出し位置</summary>
    public required Rectangle CropRegion { get; init; }

    public void Dispose()
    {
        CroppedImage.Dispose();
        GC.SuppressFinalize(this);
    }
}
