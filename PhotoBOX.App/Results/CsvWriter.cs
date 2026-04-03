using System.Globalization;
using System.Text;
using PhotoJudge.Core;

namespace PhotoBOX.App.Results;

public static class CsvWriter
{
    private static readonly string[] BaseColumns =
    [
        "FileName", "StrategyName", "CategoryConfig", "JudgedCategory",
        "Confidence", "CropX", "CropY", "CropWidth", "CropHeight",
        "Version", "Timestamp"
    ];

    private static readonly string[] ExtendedColumns =
    [
        "FileName", "StrategyName", "CategoryConfig", "JudgedCategory",
        "Confidence", "CropX", "CropY", "CropWidth", "CropHeight",
        "Version", "Timestamp", "MonitorName", "NG"
    ];

    /// <summary>
    /// JudgeResult のリストをCSVファイルに出力する（コンソール互換）。
    /// </summary>
    public static string Write(IReadOnlyList<JudgeResult> results, string outputDir, string version, string buildDate)
    {
        return Write(results, outputDir, version, buildDate, "", null);
    }

    /// <summary>
    /// JudgeResult のリストをCSVファイルに出力する（GUI版：モニター名・NG判定付き）。
    /// </summary>
    public static string Write(IReadOnlyList<JudgeResult> results, string outputDir, string version, string buildDate,
        string monitorName, IReadOnlyList<bool>? ngFlags)
    {
        if (results.Count == 0)
            throw new ArgumentException("結果が0件です。");

        var hasExtended = ngFlags != null;
        var first = results[0];
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{first.StrategyName}_{first.CategoryConfigName}_{timestamp}.csv";
        var filePath = Path.Combine(outputDir, fileName);

        Directory.CreateDirectory(outputDir);

        var sb = new StringBuilder();

        // ヘッダーコメント
        sb.AppendLine($"# PhotoBOX CoreTest Result");
        sb.AppendLine($"# Version: {version}");
        sb.AppendLine($"# Build: {buildDate}");
        sb.AppendLine($"# Strategy: {first.StrategyName}");
        sb.AppendLine($"# CategoryConfig: {first.CategoryConfigName}");
        if (!string.IsNullOrEmpty(monitorName))
            sb.AppendLine($"# Monitor: {monitorName}");
        sb.AppendLine($"# Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"# ImageCount: {results.Count}");

        sb.AppendLine(string.Join(",", hasExtended ? ExtendedColumns : BaseColumns));

        for (int i = 0; i < results.Count; i++)
        {
            var r = results[i];
            var baseLine = string.Join(",",
                Escape(r.FileName),
                Escape(r.StrategyName),
                Escape(r.CategoryConfigName),
                Escape(r.JudgedCategory),
                r.Confidence.ToString("F6", CultureInfo.InvariantCulture),
                r.CropX,
                r.CropY,
                r.CropWidth,
                r.CropHeight,
                Escape(r.Version),
                r.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")
            );

            if (hasExtended)
            {
                var ng = ngFlags != null && i < ngFlags.Count && ngFlags[i];
                baseLine += $",{Escape(monitorName)},{(ng ? "true" : "false")}";
            }

            sb.AppendLine(baseLine);
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
