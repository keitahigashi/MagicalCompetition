using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Core.Systems
{
    /// <summary>
    /// ターン進行の管理を行う。
    /// プレイヤー切り替え、パス記録、全員パス判定、ラウンドリセットを担当する。
    /// </summary>
    public class TurnManager
    {
        /// <summary>
        /// 次のプレイヤーに手番を切り替える。
        /// </summary>
        public void AdvanceTurn(GameState state)
        {
            state.AdvanceTurn();
        }

        /// <summary>
        /// パスを記録し、連続パスカウントを増加させる。
        /// </summary>
        public void RecordPass(GameState state)
        {
            state.ConsecutivePassCount++;
        }

        /// <summary>
        /// カードプレイ時に連続パスカウントをリセットする。
        /// </summary>
        public void RecordPlay(GameState state)
        {
            state.ResetPassCount();
        }

        /// <summary>
        /// 全員がパスしたかどうかを判定する。
        /// </summary>
        public bool IsAllPlayersPassed(GameState state)
        {
            return state.AllPlayersPassed;
        }

        /// <summary>
        /// 新ラウンド開始時にパスカウントをリセットする。
        /// </summary>
        public void ResetForNewRound(GameState state)
        {
            state.ResetPassCount();
        }
    }
}
