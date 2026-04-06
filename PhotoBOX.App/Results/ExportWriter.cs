using ClosedXML.Excel;
using PhotoJudge.Core;

namespace PhotoBOX.App.Results;

/// <summary>
/// 3点セット出力（224×224画像フォルダ、xlsx）のうちCSV以外を担当。
/// </summary>
public static class ExportWriter
{
    /// <summary>
    /// 判定に使用した224×224クロップ画像をフォルダに保存する。
    /// </summary>
    public static void WriteImageFolder(
        IReadOnlyList<JudgeResult> results,
        string outputDir,
        string baseFileName)
    {
        var folderPath = Path.Combine(outputDir, baseFileName);
        Directory.CreateDirectory(folderPath);

        for (int i = 0; i < results.Count; i++)
        {
            var r = results[i];
            if (r.CroppedImageJpeg.Length == 0) continue;

            var destFileName = Path.GetFileNameWithoutExtension(r.FileName) + ".jpg";
            var destPath = Path.Combine(folderPath, destFileName);
            File.WriteAllBytes(destPath, r.CroppedImageJpeg);
        }
    }

    /// <summary>
    /// NGレポート用xlsxを生成する。
    /// </summary>
    public static void WriteXlsx(
        IReadOnlyList<JudgeResult> results,
        IReadOnlyList<bool> ngFlags,
        string outputDir,
        string baseFileName)
    {
        var filePath = Path.Combine(outputDir, $"{baseFileName}.xlsx");

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("判定結果");

        var headers = new[]
        {
            "No.",              // A
            "ファイル名",        // B
            "サムネイル",        // C
            "NG",               // D
            "判定カテゴリ",       // E
            "信頼度",            // F
            "ファイルサイズ",     // G
            "幅",               // H
            "高さ",              // I
        };

        for (int c = 0; c < headers.Length; c++)
            ws.Cell(1, c + 1).Value = headers[c];

        // ヘッダー書式
        var headerRow = ws.Range(1, 1, 1, headers.Length);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // 列幅設定
        ws.Column(1).Width = 5;    // No.
        ws.Column(2).Width = 25;   // ファイル名
        ws.Column(3).Width = 12;   // サムネイル
        ws.Column(4).Width = 5;    // NG
        ws.Column(5).Width = 14;   // 判定カテゴリ
        ws.Column(6).Width = 10;   // 信頼度
        ws.Column(7).Width = 12;   // ファイルサイズ
        ws.Column(8).Width = 8;    // 幅
        ws.Column(9).Width = 8;    // 高さ

        var imageRowHeight = 60.0;

        for (int i = 0; i < results.Count; i++)
        {
            var row = i + 2;
            var r = results[i];
            var ng = i < ngFlags.Count && ngFlags[i];

            ws.Row(row).Height = imageRowHeight;

            ws.Cell(row, 1).Value = i + 1;                               // A: No.
            ws.Cell(row, 2).Value = r.FileName;                          // B: ファイル名
            // C列（サムネイル）は画像埋め込みで後述
            ws.Cell(row, 4).Value = ng ? "NG" : "";                      // D: NG
            ws.Cell(row, 5).Value = r.JudgedCategory;                    // E: 判定カテゴリ
            ws.Cell(row, 6).Value = Math.Round(r.Confidence, 6);         // F: 信頼度
            ws.Cell(row, 7).Value = FormatFileSize(r.FileSize);          // G: ファイルサイズ
            ws.Cell(row, 8).Value = r.OriginalWidth;                     // H: 幅
            ws.Cell(row, 9).Value = r.OriginalHeight;                    // I: 高さ

            // C列：判定用224×224クロップ画像の埋め込み
            if (r.CroppedImageJpeg.Length > 0)
            {
                try
                {
                    var tempPath = Path.Combine(Path.GetTempPath(), $"photobox_thumb_{i}.jpg");
                    File.WriteAllBytes(tempPath, r.CroppedImageJpeg);

                    ws.AddPicture(tempPath)
                        .MoveTo(ws.Cell(row, 3))
                        .WithSize(60, 60);

                    try { File.Delete(tempPath); } catch { }
                }
                catch { /* 画像埋め込み失敗時はスキップ */ }
            }

            // 行の背景色
            if (ng)
            {
                ws.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 230, 230);
            }
            else if (r.JudgedCategory == "その他")
            {
                ws.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 230);
            }
            else
            {
                ws.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.FromArgb(230, 255, 230);
            }
        }

        ws.RangeUsed()?.SetAutoFilter();

        workbook.SaveAs(filePath);
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes >= 1024 * 1024)
            return $"{bytes / (1024.0 * 1024.0):F1}MB";
        return $"{bytes / 1024.0:F0}KB";
    }
}
