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
            "サムネイル",        // B
            "ファイル名",        // C
            "戦略",             // D
            "分類設定",          // E
            "判定カテゴリ",       // F
            "信頼度",            // G
            "Top1Index",        // H (新規)
            "Top1Score",        // I (新規)
            "判定Round",        // J (新規)
            "CropX",            // K
            "CropY",            // L
            "CropWidth",        // M
            "CropHeight",       // N
            "Version",          // O
            "Timestamp",        // P
            "モニター名",        // Q
            "NG",               // R
            "ファイルサイズ",     // S
            "ピクセル数",        // T
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
        ws.Column(2).Width = 12;   // サムネイル
        ws.Column(3).Width = 25;   // ファイル名
        ws.Column(4).Width = 12;   // 戦略
        ws.Column(5).Width = 14;   // 分類設定
        ws.Column(6).Width = 12;   // 判定カテゴリ
        ws.Column(7).Width = 10;   // 信頼度
        ws.Column(8).Width = 10;   // Top1Index
        ws.Column(9).Width = 10;   // Top1Score
        ws.Column(10).Width = 8;   // 判定Round
        ws.Column(11).Width = 8;   // CropX
        ws.Column(12).Width = 8;   // CropY
        ws.Column(13).Width = 10;  // CropWidth
        ws.Column(14).Width = 10;  // CropHeight
        ws.Column(15).Width = 12;  // Version
        ws.Column(16).Width = 18;  // Timestamp
        ws.Column(17).Width = 12;  // モニター名
        ws.Column(18).Width = 5;   // NG
        ws.Column(19).Width = 12;  // ファイルサイズ
        ws.Column(20).Width = 15;  // ピクセル数

        var imageRowHeight = 60.0;

        for (int i = 0; i < results.Count; i++)
        {
            var row = i + 2;
            var r = results[i];
            var ng = i < ngFlags.Count && ngFlags[i];

            ws.Row(row).Height = imageRowHeight;

            ws.Cell(row, 1).Value = i + 1;
            // B列（サムネイル）は画像埋め込みで後述
            ws.Cell(row, 3).Value = r.FileName;
            ws.Cell(row, 4).Value = r.StrategyName;
            ws.Cell(row, 5).Value = r.CategoryConfigName;
            ws.Cell(row, 6).Value = r.JudgedCategory;
            ws.Cell(row, 7).Value = Math.Round(r.Confidence, 6);
            ws.Cell(row, 8).Value = r.Top1ClassIndex;
            ws.Cell(row, 9).Value = Math.Round(r.Top1RawScore, 6);
            ws.Cell(row, 10).Value = r.JudgeRound;
            ws.Cell(row, 11).Value = r.CropX;
            ws.Cell(row, 12).Value = r.CropY;
            ws.Cell(row, 13).Value = r.CropWidth;
            ws.Cell(row, 14).Value = r.CropHeight;
            ws.Cell(row, 15).Value = r.Version;
            ws.Cell(row, 16).Value = r.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            ws.Cell(row, 17).Value = "";  // MonitorName
            ws.Cell(row, 18).Value = ng ? "NG" : "";
            ws.Cell(row, 19).Value = FormatFileSize(r.FileSize);
            ws.Cell(row, 20).Value = $"{r.OriginalWidth}\u00d7{r.OriginalHeight}";

            // B列：判定用224×224クロップ画像の埋め込み
            if (r.CroppedImageJpeg.Length > 0)
            {
                try
                {
                    var tempPath = Path.Combine(Path.GetTempPath(), $"photobox_thumb_{i}.jpg");
                    File.WriteAllBytes(tempPath, r.CroppedImageJpeg);

                    ws.AddPicture(tempPath)
                        .MoveTo(ws.Cell(row, 2))
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
