using PhotoJudge.Core;
using PhotoBOX.App.Runner;
using PhotoBOX.App.Results;

namespace PhotoBOX.App;

class Program
{
    static int Main(string[] args)
    {
        // 全リソースは AppContext.BaseDirectory からの相対パスで解決。
        // 開発時(bin/Debug/)でも publish後でも同じパス構造になる。
        var baseDir = AppContext.BaseDirectory;

        var modelPath = Path.Combine(baseDir, "Models", "mobilenetv2-7.onnx");
        var configDir = Path.Combine(baseDir, "Config");
        var testDataDir = Path.Combine(baseDir, "testdata");
        var resultsDir = Path.Combine(baseDir, "results");

        if (!File.Exists(modelPath))
        {
            Console.WriteLine($"エラー: ONNXモデルが見つかりません: {modelPath}");
            Console.WriteLine("Models/mobilenetv2-7.onnx を配置してください。");
            return 1;
        }

        if (!Directory.Exists(testDataDir))
        {
            Console.WriteLine($"エラー: テストデータフォルダが見つかりません: {testDataDir}");
            Console.WriteLine("testdata/ に画像ファイルを配置してください。");
            return 1;
        }

        // カテゴリ設定の列挙
        var configFiles = Directory.Exists(configDir)
            ? Directory.GetFiles(configDir, "*.json").OrderBy(f => f).ToList()
            : [];

        if (configFiles.Count == 0)
        {
            Console.WriteLine($"エラー: カテゴリ設定ファイルが見つかりません: {configDir}");
            Console.WriteLine("Config/ に設定JSONを配置してください。");
            return 1;
        }

        // 戦略の自動検出
        var strategies = StrategyLoader.LoadAll();
        if (strategies.Count == 0)
        {
            Console.WriteLine("エラー: 利用可能な戦略が見つかりません。");
            Console.WriteLine("PhotoJudge/Strategies/ に ICropStrategy 実装クラスを配置してください。");
            return 1;
        }

        // コマンドライン引数の解析
        string? requestedStrategy = null;
        string? requestedConfig = null;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--strategy")
                requestedStrategy = args[i + 1];
            if (args[i] == "--config")
                requestedConfig = args[i + 1];
        }

        // 戦略の選択
        var strategy = requestedStrategy != null
            ? strategies.FirstOrDefault(s => s.Name.Equals(requestedStrategy, StringComparison.OrdinalIgnoreCase))
            : strategies[0];

        if (strategy == null)
        {
            Console.WriteLine($"エラー: 戦略 '{requestedStrategy}' が見つかりません。");
            Console.WriteLine("利用可能な戦略:");
            foreach (var s in strategies)
                Console.WriteLine($"  - {s.Name}: {s.Description}");
            return 1;
        }

        // カテゴリ設定の選択
        var configPath = requestedConfig != null
            ? configFiles.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f)
                .Equals(requestedConfig, StringComparison.OrdinalIgnoreCase))
            : configFiles[0];

        if (configPath == null)
        {
            Console.WriteLine($"エラー: カテゴリ設定 '{requestedConfig}' が見つかりません。");
            Console.WriteLine("利用可能なカテゴリ設定:");
            foreach (var f in configFiles)
                Console.WriteLine($"  - {Path.GetFileNameWithoutExtension(f)}");
            return 1;
        }

        var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0";

        // アセンブリのビルド日時を取得
        // SingleFileアプリではAssembly.Locationが空になるため、実行ファイルパスを使用する
        var exePath = Environment.ProcessPath;
        var buildDate = exePath != null && File.Exists(exePath)
            ? File.GetLastWriteTime(exePath).ToString("yyyy-MM-dd HH:mm")
            : "不明";

        Console.WriteLine("╔══════════════════════════════════╗");
        Console.WriteLine("║      PhotoBOX コアテスト         ║");
        Console.WriteLine("╚══════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine($"  バージョン     : {version}");
        Console.WriteLine($"  ビルド日時     : {buildDate}");
        Console.WriteLine($"  戦略           : {strategy.Name} ({strategy.Description})");
        Console.WriteLine($"  カテゴリ設定   : {Path.GetFileNameWithoutExtension(configPath)}");
        Console.WriteLine();
        Console.WriteLine($"  利用可能な戦略 : {string.Join(", ", strategies.Select(s => s.Name))}");
        Console.WriteLine($"  利用可能な設定 : {string.Join(", ", configFiles.Select(f => Path.GetFileNameWithoutExtension(f)))}");
        Console.WriteLine($"  画像フォルダ   : {testDataDir}");
        Console.WriteLine($"  出力フォルダ   : {resultsDir}");
        Console.WriteLine();
        Console.WriteLine(new string('─', 50));
        Console.WriteLine();

        using var pipeline = new JudgePipeline(modelPath, configPath);

        var runner = new TestRunner(pipeline);
        var results = runner.RunAll(testDataDir, strategy);

        if (results.Count == 0)
        {
            Console.WriteLine("処理結果が0件です。testdata/ に画像ファイルを配置してください。");
            return 1;
        }

        var csvPath = CsvWriter.Write(results, resultsDir, version, buildDate);

        Console.WriteLine();
        Console.WriteLine(new string('─', 50));
        Console.WriteLine();
        Console.WriteLine($"  処理完了: {results.Count}枚");
        Console.WriteLine($"  CSV出力 : {csvPath}");
        Console.WriteLine();

        // カテゴリ別集計
        var summary = results.GroupBy(r => r.JudgedCategory)
            .OrderByDescending(g => g.Count())
            .Select(g => $"    {g.Key}: {g.Count()}枚");
        Console.WriteLine("  カテゴリ別集計:");
        foreach (var line in summary)
            Console.WriteLine(line);

        Console.WriteLine();

        return 0;
    }
}
