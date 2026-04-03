namespace PhotoJudge.Interfaces;

public interface ICropStrategy
{
    /// <summary>戦略の識別名。CSVやUIに表示される</summary>
    string Name { get; }

    /// <summary>戦略の簡潔な説明</summary>
    string Description { get; }

    /// <summary>画像パスを受け取り、224x224の切り出し結果を返す</summary>
    CropResult Crop(string imagePath);
}
