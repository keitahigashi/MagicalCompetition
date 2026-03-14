using System.Collections.Generic;
using System.Linq;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Core.Systems
{
    /// <summary>
    /// 勝者の得点を計算する。
    /// 勝者以外のプレイヤーの残カード合計枚数を得点とする。
    /// </summary>
    public class ScoreCalculator
    {
        /// <summary>
        /// 勝者以外のプレイヤーの手札+山札の合計枚数を得点として返す。
        /// </summary>
        public int Calculate(PlayerState winner, List<PlayerState> allPlayers)
        {
            return allPlayers.Where(p => p != winner).Sum(p => p.TotalCardCount);
        }
    }
}
