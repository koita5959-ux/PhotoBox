using System.Globalization;
using System.Text;
using PhotoJudge.Core;

namespace PhotoBOX.App.Results;

public static class CsvWriter
{
    private static readonly string[] Columns =
    [
        "FileName", "StrategyName", "CategoryConfig", "JudgedCategory",
        "Confidence", "Top1ClassIndex", "Top1RawScore", "JudgeRound",
        "CropX", "CropY", "CropWidth", "CropHeight",
        "Version", "Timestamp", "MonitorName", "NG",
        "FileSize", "OriginalWidth", "OriginalHeight"
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
                Escape(r.FileName),
                Escape(r.StrategyName),
                Escape(r.CategoryConfigName),
                Escape(r.JudgedCategory),
                r.Confidence.ToString("F6", CultureInfo.InvariantCulture),
                r.Top1ClassIndex,
                r.Top1RawScore.ToString("F6", CultureInfo.InvariantCulture),
                r.JudgeRound,
                r.CropX,
                r.CropY,
                r.CropWidth,
                r.CropHeight,
                Escape(r.Version),
                r.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                "\"\"",  // MonitorName（未使用）
                ng ? "true" : "false",
                r.FileSize,
                r.OriginalWidth,
                r.OriginalHeight
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
