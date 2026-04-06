using System.Globalization;
using System.Text;
using PhotoJudge.Core;

namespace PhotoBOX.App.Results;

public static class CsvWriter
{
    private static readonly string[] Columns =
    [
        "No", "FileName", "NG", "JudgedCategory", "Confidence",
        "FileSize", "OriginalWidth", "OriginalHeight",
        "Top1Category", "Top1ClassIndex", "Top1Score",
        "Top2Category", "Top2ClassIndex", "Top2Score",
        "Top3Category", "Top3ClassIndex", "Top3Score",
        "Top4Category", "Top4ClassIndex", "Top4Score",
        "Top5Category", "Top5ClassIndex", "Top5Score",
        "JudgeRound", "CropX", "CropY", "CropWidth", "CropHeight",
        "StrategyName", "CategoryConfig", "Version", "Timestamp", "MonitorName"
    ];

    /// <summary>
    /// JudgeResult のリストをCSVファイルに出力する（コンソール互換）。
    /// </summary>
    public static string Write(IReadOnlyList<JudgeResult> results, string outputDir, string version, string buildDate)
    {
        if (results.Count == 0)
            throw new ArgumentException("結果が0件です。");

        var first = results[0];
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var baseFileName = $"{first.StrategyName}_{first.CategoryConfigName}_{timestamp}";
        return Write(results, outputDir, baseFileName, version, buildDate, null);
    }

    /// <summary>
    /// JudgeResult のリストをCSVファイルに出力する（GUI版：ベースファイル名指定）。
    /// </summary>
    public static string Write(IReadOnlyList<JudgeResult> results, string outputDir, string baseFileName,
        string version, string buildDate, IReadOnlyList<bool>? ngFlags)
    {
        if (results.Count == 0)
            throw new ArgumentException("結果が0件です。");

        var first = results[0];
        var fileName = $"{baseFileName}.csv";
        var filePath = Path.Combine(outputDir, fileName);

        var sb = new StringBuilder();

        // Excelでカラム分けされるよう区切り文字を明示
        sb.AppendLine("sep=,");

        // ヘッダーコメント
        sb.AppendLine($"# PhotoBOX CoreTest Result");
        sb.AppendLine($"# Version: {version}");
        sb.AppendLine($"# Build: {buildDate}");
        sb.AppendLine($"# Strategy: {first.StrategyName}");
        sb.AppendLine($"# CategoryConfig: {first.CategoryConfigName}");
        sb.AppendLine($"# Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"# ImageCount: {results.Count}");

        sb.AppendLine(string.Join(",", Columns));

        for (int i = 0; i < results.Count; i++)
        {
            var r = results[i];
            var ng = ngFlags != null && i < ngFlags.Count && ngFlags[i];

            var line = string.Join(",",
                i + 1,
                Escape(r.FileName),
                ng ? "true" : "false",
                Escape(r.JudgedCategory),
                r.Confidence.ToString("F6", CultureInfo.InvariantCulture),
                r.FileSize,
                r.OriginalWidth,
                r.OriginalHeight,
                Escape(r.Top1Category),
                r.Top1ClassIndex,
                r.Top1RawScore.ToString("F6", CultureInfo.InvariantCulture),
                Escape(r.Top2Category),
                r.Top2ClassIndex,
                r.Top2RawScore.ToString("F6", CultureInfo.InvariantCulture),
                Escape(r.Top3Category),
                r.Top3ClassIndex,
                r.Top3RawScore.ToString("F6", CultureInfo.InvariantCulture),
                Escape(r.Top4Category),
                r.Top4ClassIndex,
                r.Top4RawScore.ToString("F6", CultureInfo.InvariantCulture),
                Escape(r.Top5Category),
                r.Top5ClassIndex,
                r.Top5RawScore.ToString("F6", CultureInfo.InvariantCulture),
                r.JudgeRound,
                r.CropX,
                r.CropY,
                r.CropWidth,
                r.CropHeight,
                Escape(r.StrategyName),
                Escape(r.CategoryConfigName),
                Escape(r.Version),
                r.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                "\"\"" // MonitorName（未使用）
            );

            sb.AppendLine(line);
        }

        File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
        return filePath;
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
