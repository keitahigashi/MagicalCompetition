namespace MagicalCompetition.Core.Model
{
    /// <summary>
    /// ゲーム設定値を保持する不変クラス。
    /// Unity非依存の純粋C#クラス。
    /// </summary>
    public class GameConfig
    {
        // ─── 定数 ──────────────────────────────────────────────────────────────

        /// <summary>デッキの総カード枚数。</summary>
        public const int TotalCards = 45;

        /// <summary>初期手札枚数。</summary>
        public const int HandSize = 3;

        /// <summary>初期場札の数字。</summary>
        public const int InitialFieldNumber = 5;

        // ─── プロパティ ────────────────────────────────────────────────────────

        /// <summary>AI対戦相手の人数（1〜4）。</summary>
        public int AICount { get; }

        /// <summary>総プレイヤー数（人間1人 + AI）。</summary>
        public int TotalPlayerCount => AICount + 1;

        // ─── コンストラクタ ───────────────────────────────────────────────────

        /// <summary>
        /// GameConfig を初期化する。
        /// </summary>
        /// <param name="aiCount">AI対戦相手の人数（1〜4）。</param>
        public GameConfig(int aiCount)
        {
            AICount = aiCount;
        }
    }
}
