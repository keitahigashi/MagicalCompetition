using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Core.Systems
{
    /// <summary>
    /// プレイヤーの勝利判定を行う。
    /// 手札と山札が共に0枚であれば勝利とする。
    /// </summary>
    public class WinChecker
    {
        /// <summary>
        /// プレイヤーが勝利したかを判定する。
        /// 手札+山札の合計が0枚の場合にtrueを返す。
        /// </summary>
        public bool Check(PlayerState player)
        {
            return player.HasWon;
        }
    }
}
