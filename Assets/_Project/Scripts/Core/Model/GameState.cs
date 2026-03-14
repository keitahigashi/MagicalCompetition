using System.Collections.Generic;

namespace MagicalCompetition.Core.Model
{
    /// <summary>
    /// ゲーム全体の状態を保持するクラス。
    /// Unity非依存の純粋C#クラス。
    /// </summary>
    public class GameState
    {
        // ─── プロパティ ────────────────────────────────────────────────────────

        /// <summary>全プレイヤーの状態リスト。</summary>
        public List<PlayerState> Players { get; }

        /// <summary>現在のゲームフェーズ。</summary>
        public GamePhase CurrentPhase { get; set; }

        /// <summary>現在の手番プレイヤーのインデックス。</summary>
        public int CurrentPlayerIndex { get; set; }

        /// <summary>場札の状態。</summary>
        public FieldState Field { get; }

        /// <summary>連続パス数。</summary>
        public int ConsecutivePassCount { get; set; }

        /// <summary>捨て札置き場。</summary>
        public List<Card> DiscardPile { get; }

        /// <summary>除外カード置き場。</summary>
        public List<Card> ExcludedCards { get; }

        // ─── 計算プロパティ ───────────────────────────────────────────────────

        /// <summary>現在の手番プレイヤー。</summary>
        public PlayerState CurrentPlayer => Players[CurrentPlayerIndex];

        /// <summary>総プレイヤー数。</summary>
        public int PlayerCount => Players.Count;

        /// <summary>全員がパスしたかどうか。</summary>
        public bool AllPlayersPassed => ConsecutivePassCount >= PlayerCount;

        // ─── コンストラクタ ───────────────────────────────────────────────────

        /// <summary>
        /// GameState を初期化する。
        /// </summary>
        /// <param name="players">プレイヤー状態のリスト。</param>
        public GameState(List<PlayerState> players)
        {
            Players              = players;
            CurrentPhase         = GamePhase.Setup;
            CurrentPlayerIndex   = 0;
            ConsecutivePassCount = 0;
            Field                = new FieldState();
            DiscardPile          = new List<Card>();
            ExcludedCards        = new List<Card>();
        }

        // ─── ターン操作 ───────────────────────────────────────────────────────

        /// <summary>
        /// 手番を次のプレイヤーに進める。
        /// 最後のプレイヤーの次は先頭プレイヤーに循環する。
        /// </summary>
        public void AdvanceTurn()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % PlayerCount;
        }

        /// <summary>連続パス数を0にリセットする。</summary>
        public void ResetPassCount()
        {
            ConsecutivePassCount = 0;
        }
    }
}
