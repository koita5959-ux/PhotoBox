using PhotoJudge.Interfaces;
using PhotoJudge.Inference;
using PhotoJudge.CategoryMapping;

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

    /// <summary>
    /// 1枚の画像を指定戦略で切り出し → ONNX判定 → カテゴリマッピング → 結果返却。
    /// </summary>
    public JudgeResult Judge(string imagePath, ICropStrategy strategy)
    {
        using var cropResult = strategy.Crop(imagePath);

        var normalized = ImageNetNormalizer.Normalize(cropResult.CroppedImage);
        var onnxResult = _classifier.Classify(normalized);
        var category = _mapper.Resolve(onnxResult.TopClassIndex);

        var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0";

        return new JudgeResult
        {
            FileName = Path.GetFileName(imagePath),
            StrategyName = cropResult.StrategyName,
            CategoryConfigName = _mapper.ConfigName,
            JudgedCategory = category,
            Confidence = onnxResult.TopScore,
            CropX = cropResult.CropRegion.X,
            CropY = cropResult.CropRegion.Y,
            CropWidth = cropResult.CropRegion.Width,
            CropHeight = cropResult.CropRegion.Height,
            Version = version,
            Timestamp = DateTime.Now
        };
    }

    public void Dispose()
    {
        _classifier.Dispose();
        GC.SuppressFinalize(this);
    }
}
