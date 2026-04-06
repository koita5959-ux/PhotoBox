using System.Text.Json;
using PhotoJudge;

namespace PhotoJudge.CategoryMapping;

public class CategoryConfig
{
    public required string ConfigName { get; init; }
    public required string Description { get; init; }
    public required string[] Categories { get; init; }
    public required Dictionary<int, string> Mapping { get; init; }

    /// <summary>大分類→カテゴリ名の対応（UI表示用）</summary>
    public Dictionary<string, string[]>? CategoryGroups { get; init; }

    /// <summary>カテゴリ名→利用者向け説明（UI表示用）</summary>
    public Dictionary<string, string>? CategoryDescriptions { get; init; }
}

/// <summary>
/// CategoryMapper.Resolve の戻り値。
/// 合算信頼度・Top1情報を含む。
/// </summary>
public class ResolveResult
{
    /// <summary>判定カテゴリ名（または「その他」）</summary>
    public required string Category { get; init; }

    /// <summary>Top5カテゴリ合算後の信頼度</summary>
    public required float AggregatedConfidence { get; init; }

    /// <summary>Top1のクラスインデックス（記録用）</summary>
    public required int Top1ClassIndex { get; init; }

    /// <summary>Top1の合算前信頼度（記録用）</summary>
    public required float Top1RawScore { get; init; }
}

public class CategoryMapper
{
    private readonly CategoryConfig _config;

    /// <summary>Top5カテゴリ合算後の信頼度閾値（全カテゴリ共通）。JudgeConfigから参照</summary>
    public static float ConfidenceThreshold => JudgeConfig.ConfidenceThreshold;

    public string ConfigName => _config.ConfigName;
    public string[] Categories => _config.Categories;

    /// <summary>大分類→カテゴリ名（UI表示用）。JSONに定義がなければnull</summary>
    public Dictionary<string, string[]>? CategoryGroups => _config.CategoryGroups;

    /// <summary>カテゴリ名→説明（UI表示用）。JSONに定義がなければnull</summary>
    public Dictionary<string, string>? CategoryDescriptions => _config.CategoryDescriptions;

    public CategoryMapper(string configPath)
    {
        var json = File.ReadAllText(configPath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var configName = root.GetProperty("configName").GetString()!;
        var description = root.GetProperty("description").GetString()!;
        var categories = root.GetProperty("categories").EnumerateArray()
            .Select(e => e.GetString()!).ToArray();

        var mapping = new Dictionary<int, string>();
        foreach (var prop in root.GetProperty("mapping").EnumerateObject())
        {
            mapping[int.Parse(prop.Name)] = prop.Value.GetString()!;
        }

        // Optional: categoryGroups
        Dictionary<string, string[]>? groups = null;
        if (root.TryGetProperty("categoryGroups", out var groupsElem))
        {
            groups = new Dictionary<string, string[]>();
            foreach (var prop in groupsElem.EnumerateObject())
            {
                groups[prop.Name] = prop.Value.EnumerateArray()
                    .Select(e => e.GetString()!).ToArray();
            }
        }

        // Optional: categoryDescriptions
        Dictionary<string, string>? descriptions = null;
        if (root.TryGetProperty("categoryDescriptions", out var descElem))
        {
            descriptions = new Dictionary<string, string>();
            foreach (var prop in descElem.EnumerateObject())
            {
                descriptions[prop.Name] = prop.Value.GetString()!;
            }
        }

        _config = new CategoryConfig
        {
            ConfigName = configName,
            Description = description,
            Categories = categories,
            Mapping = mapping,
            CategoryGroups = groups,
            CategoryDescriptions = descriptions
        };
    }

    /// <summary>
    /// Top5リストと有効カテゴリリストから判定カテゴリを決定する。
    ///
    /// 処理:
    /// 1. Top5の各クラスインデックスをカテゴリ名に変換（マッピングにないものは「その他」）
    /// 2. カテゴリ別に信頼度を合算
    /// 3. 合算信頼度が最も高いカテゴリを特定
    /// 4. そのカテゴリが有効カテゴリリストに含まれるか確認
    /// 5. 含まれており、かつ合算信頼度 >= 閾値 → そのカテゴリ名を返す
    /// 6. それ以外 → 「その他」を返す
    /// </summary>
    public ResolveResult Resolve(IReadOnlyList<(int ClassIndex, float Score)> top5, string[] activeCategories)
    {
        var top1Index = top5.Count > 0 ? top5[0].ClassIndex : -1;
        var top1Score = top5.Count > 0 ? top5[0].Score : 0f;

        // Step 1-2: カテゴリ別に信頼度を合算
        var categoryScores = new Dictionary<string, float>();
        foreach (var (classIndex, score) in top5)
        {
            var category = _config.Mapping.TryGetValue(classIndex, out var cat) ? cat : "その他";
            if (!categoryScores.ContainsKey(category))
                categoryScores[category] = 0f;
            categoryScores[category] += score;
        }

        // Step 3: 合算信頼度が最も高いカテゴリ
        string bestCategory = "その他";
        float bestScore = 0f;
        foreach (var (category, score) in categoryScores)
        {
            if (score > bestScore)
            {
                bestScore = score;
                bestCategory = category;
            }
        }

        // Step 4-6: 有効カテゴリ確認 + 閾値判定
        var activeSet = new HashSet<string>(activeCategories);
        bool isValid = bestCategory != "その他"
                       && activeSet.Contains(bestCategory)
                       && bestScore >= ConfidenceThreshold;

        return new ResolveResult
        {
            Category = isValid ? bestCategory : "その他",
            AggregatedConfidence = isValid ? bestScore : categoryScores.GetValueOrDefault("その他", 0f),
            Top1ClassIndex = top1Index,
            Top1RawScore = top1Score
        };
    }

    /// <summary>
    /// v1.02互換: 単一クラスインデックスからカテゴリ名を返す。
    /// TestRunner等の後方互換用。
    /// </summary>
    public string Resolve(int classIndex)
    {
        return _config.Mapping.TryGetValue(classIndex, out var category) ? category : "その他";
    }
}
