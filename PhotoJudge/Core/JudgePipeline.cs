using PhotoJudge.Interfaces;
using PhotoJudge.Inference;
using PhotoJudge.CategoryMapping;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace PhotoJudge.Core;

public class JudgeResult
{
    public required string FileName { get; init; }
    public required string StrategyName { get; init; }
    public required string CategoryConfigName { get; init; }
    public required string JudgedCategory { get; init; }
    public required float Confidence { get; init; }
    public required int CropX { get; init; }
    public required int CropY { get; init; }
    public required int CropWidth { get; init; }
    public required int CropHeight { get; init; }
    public required string Version { get; init; }
    public required DateTime Timestamp { get; init; }

    /// <summary>判定に使用したクロップ済み224×224画像（JPEG）</summary>
    public required byte[] CroppedImageJpeg { get; init; }

    /// <summary>元ファイルのサイズ（バイト）</summary>
    public required long FileSize { get; init; }

    /// <summary>元画像の幅（ピクセル）</summary>
    public required int OriginalWidth { get; init; }

    /// <summary>元画像の高さ（ピクセル）</summary>
    public required int OriginalHeight { get; init; }

    /// <summary>Top1のクラスインデックス（記録用）</summary>
    public required int Top1ClassIndex { get; init; }

    /// <summary>Top1の合算前信頼度（記録用）</summary>
    public required float Top1RawScore { get; init; }

    /// <summary>何回目の判定か（1, 2, 3...）</summary>
    public required int JudgeRound { get; init; }
}

public class JudgePipeline : IDisposable
{
    private readonly OnnxClassifier _classifier;
    private readonly CategoryMapper _mapper;

    public JudgePipeline(string modelPath, string categoryConfigPath)
    {
        _classifier = new OnnxClassifier(modelPath);
        _mapper = new CategoryMapper(categoryConfigPath);
    }

    public string CategoryConfigName => _mapper.ConfigName;

    /// <summary>現在の分類設定に含まれるカテゴリ一覧（「その他」を含む）</summary>
    public string[] Categories => _mapper.Categories;

    /// <summary>大分類→カテゴリ名（UI表示用）</summary>
    public Dictionary<string, string[]>? CategoryGroups => _mapper.CategoryGroups;

    /// <summary>カテゴリ名→説明（UI表示用）</summary>
    public Dictionary<string, string>? CategoryDescriptions => _mapper.CategoryDescriptions;

    /// <summary>
    /// 1枚の画像を指定戦略で切り出し → ONNX判定 → カテゴリマッピング → 結果返却。
    /// </summary>
    /// <param name="imagePath">画像ファイルパス</param>
    /// <param name="strategy">切り出し戦略</param>
    /// <param name="activeCategories">有効カテゴリ（利用者がチェックしたカテゴリ名の配列）</param>
    /// <param name="judgeRound">判定ラウンド（1, 2, 3...）</param>
    /// <summary>白黒余白比率の閾値。これ以上なら判定スキップ</summary>
    public const float BlankRatioThreshold = 0.30f;

    public JudgeResult Judge(string imagePath, ICropStrategy strategy, string[] activeCategories, int judgeRound)
    {
        using var cropResult = strategy.Crop(imagePath);

        // 白黒余白チェック: 判定領域として信用できない画像はスキップ
        var blankRatio = CalcBlankPixelRatio(cropResult.CroppedImage);
        var skipInference = blankRatio >= BlankRatioThreshold;

        ResolveResult resolveResult;

        if (skipInference)
        {
            // 余白が多すぎる → 推論せず「その他」
            resolveResult = new ResolveResult
            {
                Category = "その他",
                AggregatedConfidence = 0f,
                Top1ClassIndex = -1,
                Top1RawScore = 0f
            };
        }
        else
        {
            var normalized = ImageNetNormalizer.Normalize(cropResult.CroppedImage);
            var onnxResult = _classifier.Classify(normalized);
            resolveResult = _mapper.Resolve(onnxResult.Top5, activeCategories);
        }

        // 判定に使用した224×224画像をJPEGバイト列として保持（Export用）
        using var ms = new MemoryStream();
        cropResult.CroppedImage.SaveAsJpeg(ms, new JpegEncoder { Quality = 95 });
        var jpegBytes = ms.ToArray();

        var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0";
        var fileSize = new FileInfo(imagePath).Length;

        return new JudgeResult
        {
            FileName = Path.GetFileName(imagePath),
            StrategyName = cropResult.StrategyName,
            CategoryConfigName = _mapper.ConfigName,
            JudgedCategory = resolveResult.Category,
            Confidence = resolveResult.AggregatedConfidence,
            CropX = cropResult.CropRegion.X,
            CropY = cropResult.CropRegion.Y,
            CropWidth = cropResult.CropRegion.Width,
            CropHeight = cropResult.CropRegion.Height,
            Version = version,
            Timestamp = DateTime.Now,
            CroppedImageJpeg = jpegBytes,
            FileSize = fileSize,
            OriginalWidth = cropResult.OriginalWidth,
            OriginalHeight = cropResult.OriginalHeight,
            Top1ClassIndex = resolveResult.Top1ClassIndex,
            Top1RawScore = resolveResult.Top1RawScore,
            JudgeRound = judgeRound
        };
    }

    /// <summary>
    /// 224×224画像の白（RGB>245）または黒（RGB&lt;10）ピクセルの比率を返す。
    /// </summary>
    private static float CalcBlankPixelRatio(Image<Rgba32> image)
    {
        int blankCount = 0;
        int total = image.Width * image.Height;

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var p = image[x, y];
                if ((p.R > 245 && p.G > 245 && p.B > 245) ||
                    (p.R < 10 && p.G < 10 && p.B < 10))
                {
                    blankCount++;
                }
            }
        }

        return (float)blankCount / total;
    }

    public void Dispose()
    {
        _classifier.Dispose();
        GC.SuppressFinalize(this);
    }
}
