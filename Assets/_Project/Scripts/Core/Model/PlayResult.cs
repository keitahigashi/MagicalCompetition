using System.Collections.Generic;

namespace MagicalCompetition.Core.Model
{
    /// <summary>
    /// 1ゲームの結果を表す不変クラス。
    /// Unity非依存の純粋C#クラス。
    /// TODO: リファクタ時に AllPlayers を IReadOnlyList&lt;PlayerState&gt; に変更することを検討する。
    /// </summary>
    public class PlayResult
    {
        /// <summary>勝者のプレイヤー状態。</summary>
        public PlayerState Winner { get; }

        /// <summary>勝者のスコア。</summary>
        public int Score { get; }

        /// <summary>全プレイヤーの状態リスト。</summary>
        public List<PlayerState> AllPlayers { get; }

        /// <summary>
        /// PlayResult を初期化する。
        /// </summary>
        /// <param name="winner">勝者の PlayerState。</param>
        /// <param name="score">勝者のスコア。</param>
        /// <param name="allPlayers">全プレイヤーの状態リスト。</param>
        public PlayResult(PlayerState winner, int score, List<PlayerState> allPlayers)
        {
            Winner = winner;
            Score = score;
            AllPlayers = allPlayers;
        }
    }
}
