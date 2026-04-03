using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PhotoJudge.Inference;

public static class ImageNetNormalizer
{
    private const int Size = 224;

    // ImageNet normalization parameters
    private const float MeanR = 0.485f, MeanG = 0.456f, MeanB = 0.406f;
    private const float StdR = 0.229f, StdG = 0.224f, StdB = 0.225f;

    /// <summary>
    /// 224x224 の Image を NCHW 形式・ImageNet正規化済み float[1*3*224*224] に変換する。
    /// </summary>
    public static float[] Normalize(Image<Rgba32> image)
    {
        if (image.Width != Size || image.Height != Size)
            throw new ArgumentException($"Image must be {Size}x{Size}, got {image.Width}x{image.Height}");

        var result = new float[3 * Size * Size];
        int plane = Size * Size;

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                var p = image[x, y];
                int idx = y * Size + x;
                result[idx]             = (p.R / 255f - MeanR) / StdR;
                result[plane + idx]     = (p.G / 255f - MeanG) / StdG;
                result[2 * plane + idx] = (p.B / 255f - MeanB) / StdB;
            }
        }

        return result;
    }
}
