using System.Text.Json;

namespace PhotoJudge.CategoryMapping;

public class CategoryConfig
{
    public required string ConfigName { get; init; }
    public required string Description { get; init; }
    public required string[] Categories { get; init; }
    public required Dictionary<int, string> Mapping { get; init; }
}

public class CategoryMapper
{
    private readonly CategoryConfig _config;

    public string ConfigName => _config.ConfigName;
    public string[] Categories => _config.Categories;

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

        _config = new CategoryConfig
        {
            ConfigName = configName,
            Description = description,
            Categories = categories,
            Mapping = mapping
        };
    }

    /// <summary>
    /// ONNXクラスインデックスをカテゴリ名に変換する。
    /// マッピングに存在しないインデックスは「その他」を返す。
    /// </summary>
    public string Resolve(int classIndex)
    {
        return _config.Mapping.TryGetValue(classIndex, out var category) ? category : "その他";
    }
}
