using ClosedXML.Excel;
using PhotoJudge.Core;

namespace PhotoBOX.App.Results;

/// <summary>
/// F6-09: 3点セット出力（224×224画像フォルダ、xlsx）のうちCSV以外を担当。
/// </summary>
public static class ExportWriter
{
    /// <summary>
    /// 224×224判定用画像をフォルダにコピーする。
    /// 出力先: {outputDir}/{baseFileName}/
    /// </summary>
    public static void WriteImageFolder(
        IReadOnlyList<JudgeResult> results,
        IReadOnlyList<string> imagePaths,
        string outputDir,
        string baseFileName)
    {
        var folderPath = Path.Combine(outputDir, baseFileName);
        Directory.CreateDirectory(folderPath);

        for (int i = 0; i < results.Count; i++)
        {
            if (i >= imagePaths.Count || string.IsNullOrEmpty(imagePaths[i]))
                continue;

            var srcPath = imagePaths[i];
            if (!File.Exists(srcPath)) continue;

            try
            {
                // 224×224 にリサイズして保存
                using var fs = new FileStream(srcPath, FileMode.Open, FileAccess.Read);
                using var original = Image.FromStream(fs);
                using var resized = new Bitmap(224, 224);
                using (var g = Graphics.FromImage(resized))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(original, 0, 0, 224, 224);
                }

                var destFileName = Path.GetFileNameWithoutExtension(results[i].FileName) + ".jpg";
                var destPath = Path.Combine(folderPath, destFileName);
                resized.Save(destPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch
            {
                // 画像変換失敗時はスキップ（webp非対応等）
            }
        }
    }

    /// <summary>
    /// プレビュー確認用xlsxを生成する。
    /// 出力先: {outputDir}/{baseFileName}.xlsx
    /// </summary>
    public static void WriteXlsx(
        IReadOnlyList<JudgeResult> results,
        IReadOnlyList<string> imagePaths,
        IReadOnlyList<bool> ngFlags,
        string outputDir,
        string baseFileName)
    {
        var filePath = Path.Combine(outputDir, $"{baseFileName}.xlsx");

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("判定結果");

        // ヘッダー
        ws.Cell(1, 1).Value = "No.";
        ws.Cell(1, 2).Value = "サムネイル";
        ws.Cell(1, 3).Value = "ファイル名";
        ws.Cell(1, 4).Value = "カテゴリ";
        ws.Cell(1, 5).Value = "信頼度";
        ws.Cell(1, 6).Value = "NG";
        ws.Cell(1, 7).Value = "ファイルサイズ";
        ws.Cell(1, 8).Value = "ピクセル数";

        // ヘッダー書式
        var headerRow = ws.Range(1, 1, 1, 8);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // 列幅設定
        ws.Column(1).Width = 5;   // No.
        ws.Column(2).Width = 12;  // サムネイル
        ws.Column(3).Width = 30;  // ファイル名
        ws.Column(4).Width = 15;  // カテゴリ
        ws.Column(5).Width = 10;  // 信頼度
        ws.Column(6).Width = 5;   // NG
        ws.Column(7).Width = 12;  // ファイルサイズ
        ws.Column(8).Width = 15;  // ピクセル数

        var imageRowHeight = 60.0; // サムネイル行の高さ（ポイント）

        for (int i = 0; i < results.Count; i++)
        {
            var row = i + 2;
            var r = results[i];
            var ng = i < ngFlags.Count && ngFlags[i];

            ws.Row(row).Height = imageRowHeight;

            ws.Cell(row, 1).Value = i + 1;
            ws.Cell(row, 3).Value = r.FileName;
            ws.Cell(row, 4).Value = r.JudgedCategory;
            ws.Cell(row, 5).Value = Math.Round(r.Confidence, 3);
            ws.Cell(row, 6).Value = ng ? "NG" : "";

            // ファイルサイズ・ピクセル数
            if (i < imagePaths.Count && !string.IsNullOrEmpty(imagePaths[i]) && File.Exists(imagePaths[i]))
            {
                try
                {
                    var fi = new FileInfo(imagePaths[i]);
                    ws.Cell(row, 7).Value = FormatFileSize(fi.Length);

                    using var fs = new FileStream(imagePaths[i], FileMode.Open, FileAccess.Read);
                    using var img = Image.FromStream(fs);
                    ws.Cell(row, 8).Value = $"{img.Width}×{img.Height}";
                }
                catch { /* スキップ */ }
            }

            // サムネイル画像埋め込み
            if (i < imagePaths.Count && !string.IsNullOrEmpty(imagePaths[i]) && File.Exists(imagePaths[i]))
            {
                try
                {
                    // 一時的にサムネイルを生成してxlsxに埋め込む
                    using var fs = new FileStream(imagePaths[i], FileMode.Open, FileAccess.Read);
                    using var original = Image.FromStream(fs);
                    using var thumb = new Bitmap(80, 60);
                    using (var g = Graphics.FromImage(thumb))
                    {
                        g.Clear(Color.LightGray);
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                        // アスペクト比を維持してフィット
                        float scale = Math.Min(80f / original.Width, 60f / original.Height);
                        int sw = (int)(original.Width * scale);
                        int sh = (int)(original.Height * scale);
                        int sx = (80 - sw) / 2;
                        int sy = (60 - sh) / 2;
                        g.DrawImage(original, sx, sy, sw, sh);
                    }

                    // 一時ファイルに保存してClosedXMLに渡す
                    var tempPath = Path.Combine(Path.GetTempPath(), $"photobox_thumb_{i}.png");
                    thumb.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);

                    var picture = ws.AddPicture(tempPath)
                        .MoveTo(ws.Cell(row, 2))
                        .WithSize(80, 60);

                    // 一時ファイル削除
                    try { File.Delete(tempPath); } catch { }
                }
                catch { /* 画像埋め込み失敗時はスキップ */ }
            }

            // NG行の背景色
            if (ng)
            {
                ws.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 230, 230);
            }
            else if (r.JudgedCategory == "その他")
            {
                ws.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 255, 230);
            }
            else
            {
                ws.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.FromArgb(230, 255, 230);
            }
        }

        // オートフィルターを適用
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
