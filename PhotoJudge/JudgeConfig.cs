namespace PhotoJudge
{
    /// <summary>
    /// 判定パラメータの集約。
    /// Claude.AIが判定精度の調整を検討する際、このファイルのみ確認すれば
    /// 現在の判定条件を把握できる。
    /// </summary>
    public static class JudgeConfig
    {
        /// <summary>
        /// Top5カテゴリ合算信頼度の閾値。
        /// これ未満の場合「その他」に判定される。
        /// </summary>
        public const float ConfidenceThreshold = 0.30f;

        /// <summary>
        /// 白黒ピクセル比率の閾値。
        /// クロップ画像の白(RGB>245)または黒(RGB&lt;10)のピクセルが
        /// この比率以上の場合、推論をスキップし「その他」に判定される。
        /// </summary>
        public const float BlankRatioThreshold = 0.30f;

        /// <summary>
        /// 白ピクセルの判定基準（R,G,B全てがこの値を超える場合に白と判定）
        /// </summary>
        public const byte WhiteThreshold = 245;

        /// <summary>
        /// 黒ピクセルの判定基準（R,G,B全てがこの値未満の場合に黒と判定）
        /// </summary>
        public const byte BlackThreshold = 10;
    }
}
