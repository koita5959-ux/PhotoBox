using PhotoJudge.Core;
using PhotoJudge.Interfaces;

namespace PhotoBOX.App.Runner;

public class TestRunner
{
    private static readonly string[] SupportedExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff"];

    private readonly JudgePipeline _pipeline;

    public TestRunner(JudgePipeline pipeline)
    {
        _pipeline = pipeline;
    }

    /// <summary>
    /// 指定ディレクトリ内の全画像を、指定戦略で判定する。
    /// </summary>
    public List<JudgeResult> RunAll(string imageDir, ICropStrategy strategy)
    {
        var files = Directory.GetFiles(imageDir)
            .Where(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .OrderBy(f => f)
            .ToList();

        var results = new List<JudgeResult>();

        foreach (var file in files)
        {
            Console.WriteLine($"  判定中: {Path.GetFileName(file)}");
            var allCategories = _pipeline.Categories.Where(c => c != "その他").ToArray();
            var result = _pipeline.Judge(file, strategy, allCategories, 1);
            results.Add(result);
            Console.WriteLine($"    → {result.JudgedCategory} ({result.Confidence:P1})");
        }

        return results;
    }
}
