using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace PhotoJudge.Inference;

public class OnnxClassifyResult
{
    public required int TopClassIndex { get; init; }
    public required float TopScore { get; init; }
    public required List<(int ClassIndex, float Score)> Top5 { get; init; }
}

public class OnnxClassifier : IDisposable
{
    private readonly InferenceSession _session;
    private readonly string _inputName;

    public OnnxClassifier(string modelPath)
    {
        _session = new InferenceSession(modelPath);
        _inputName = _session.InputMetadata.Keys.First();
    }

    /// <summary>
    /// NCHW形式・ImageNet正規化済みのfloat配列 [1, 3, 224, 224] を推論する。
    /// 出力にsoftmaxを適用し、Top5クラスを返す。
    /// </summary>
    public OnnxClassifyResult Classify(float[] normalizedPixels)
    {
        var tensor = new DenseTensor<float>(normalizedPixels, [1, 3, 224, 224]);
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor(_inputName, tensor)
        };

        using var results = _session.Run(inputs);
        var output = results.First().AsEnumerable<float>().ToArray();

        // Softmax
        float maxVal = output.Max();
        var exps = output.Select(v => MathF.Exp(v - maxVal)).ToArray();
        float sumExp = exps.Sum();
        var probs = exps.Select(e => e / sumExp).ToArray();

        // Top5
        var top5 = probs
            .Select((score, index) => (index, score))
            .OrderByDescending(x => x.score)
            .Take(5)
            .Select(x => (x.index, x.score))
            .ToList();

        return new OnnxClassifyResult
        {
            TopClassIndex = top5[0].index,
            TopScore = top5[0].score,
            Top5 = top5
        };
    }

    public void Dispose()
    {
        _session.Dispose();
        GC.SuppressFinalize(this);
    }
}
