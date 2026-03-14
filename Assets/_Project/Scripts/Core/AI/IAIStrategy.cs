using System.Collections.Generic;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Core.AI
{
    /// <summary>
    /// AI戦略の共通インターフェース。
    /// 各難易度はこのインターフェースを実装する。
    /// </summary>
    public interface IAIStrategy
    {
        /// <summary>
        /// 全有効手のリストから最適な手を選択して返す。
        /// validPlaysが空の場合はパスを返す。
        /// </summary>
        PlayAction DecideAction(GameState state, List<PlayAction> validPlays);

        /// <summary>
        /// 場札リセット時の色選択を行う。
        /// </summary>
        CardColor SelectFieldColor(GameState state);
    }
}
