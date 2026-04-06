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

    /// <summary>Top1のカテゴリ名（CategoryMapper変換後）</summary>
    public required string Top1Category { get; init; }

    /// <summary>Top1のクラスインデックス（記録用）</summary>
    public required int Top1ClassIndex { get; init; }

    /// <summary>Top1の合算前信頼度（記録用）</summary>
    public required float Top1RawScore { get; init; }

    /// <summary>Top2のカテゴリ名</summary>
    public required string Top2Category { get; init; }

    /// <summary>Top2のクラスインデックス</summary>
    public required int Top2ClassIndex { get; init; }

    /// <summary>Top2の信頼度</summary>
    public required float Top2RawScore { get; init; }

    /// <summary>Top3のカテゴリ名</summary>
    public required string Top3Category { get; init; }

    /// <summary>Top3のクラスインデックス</summary>
    public required int Top3ClassIndex { get; init; }

    /// <summary>Top3の信頼度</summary>
    public required float Top3RawScore { get; init; }

    /// <summary>Top4のカテゴリ名</summary>
    public required string Top4Category { get; init; }

    /// <summary>Top4のクラスインデックス</summary>
    public required int Top4ClassIndex { get; init; }

    /// <summary>Top4の信頼度</summary>
    public required float Top4RawScore { get; init; }

    /// <summary>Top5のカテゴリ名</summary>
    public required string Top5Category { get; init; }

    /// <summary>Top5のクラスインデックス</summary>
    public required int Top5ClassIndex { get; init; }

    /// <summary>Top5の信頼度</summary>
    public required float Top5RawScore { get; init; }

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
    public JudgeResult Judge(string imagePath, ICropStrategy strategy, string[] activeCategories, int judgeRound)
    {
        using var cropResult = strategy.Crop(imagePath);

        // 白黒余白チェック: 判定領域として信用できない画像はスキップ
        var blankRatio = CalcBlankPixelRatio(cropResult.CroppedImage);
        var skipInference = blankRatio >= JudgeConfig.BlankRatioThreshold;

        ResolveResult resolveResult;
        IReadOnlyList<(int ClassIndex, float Score)>? top5Raw = null;

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
            top5Raw = onnxResult.Top5;
            resolveResult = _mapper.Resolve(onnxResult.Top5, activeCategories);
        }

        // Top1〜5の各クラスインデックスをカテゴリ名に1対1変換（合算前の素の分類結果）
        string top1Cat = "", top2Cat = "", top3Cat = "", top4Cat = "", top5Cat = "";
        int top1Idx = -1, top2Idx = -1, top3Idx = -1, top4Idx = -1, top5Idx = -1;
        float top1Scr = 0f, top2Scr = 0f, top3Scr = 0f, top4Scr = 0f, top5Scr = 0f;

        if (top5Raw != null)
        {
            if (top5Raw.Count > 0) { top1Cat = _mapper.Resolve(top5Raw[0].ClassIndex); top1Idx = top5Raw[0].ClassIndex; top1Scr = top5Raw[0].Score; }
            if (top5Raw.Count > 1) { top2Cat = _mapper.Resolve(top5Raw[1].ClassIndex); top2Idx = top5Raw[1].ClassIndex; top2Scr = top5Raw[1].Score; }
            if (top5Raw.Count > 2) { top3Cat = _mapper.Resolve(top5Raw[2].ClassIndex); top3Idx = top5Raw[2].ClassIndex; top3Scr = top5Raw[2].Score; }
            if (top5Raw.Count > 3) { top4Cat = _mapper.Resolve(top5Raw[3].ClassIndex); top4Idx = top5Raw[3].ClassIndex; top4Scr = top5Raw[3].Score; }
            if (top5Raw.Count > 4) { top5Cat = _mapper.Resolve(top5Raw[4].ClassIndex); top5Idx = top5Raw[4].ClassIndex; top5Scr = top5Raw[4].Score; }
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
            Top1Category = top1Cat,
            Top1ClassIndex = top1Idx,
            Top1RawScore = top1Scr,
            Top2Category = top2Cat,
            Top2ClassIndex = top2Idx,
            Top2RawScore = top2Scr,
            Top3Category = top3Cat,
            Top3ClassIndex = top3Idx,
            Top3RawScore = top3Scr,
            Top4Category = top4Cat,
            Top4ClassIndex = top4Idx,
            Top4RawScore = top4Scr,
            Top5Category = top5Cat,
            Top5ClassIndex = top5Idx,
            Top5RawScore = top5Scr,
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
                if ((p.R > JudgeConfig.WhiteThreshold && p.G > JudgeConfig.WhiteThreshold && p.B > JudgeConfig.WhiteThreshold) ||
                    (p.R < JudgeConfig.BlackThreshold && p.G < JudgeConfig.BlackThreshold && p.B < JudgeConfig.BlackThreshold))
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
