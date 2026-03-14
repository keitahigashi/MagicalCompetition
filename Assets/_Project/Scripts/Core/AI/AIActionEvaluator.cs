using System.Collections.Generic;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.Systems;

namespace MagicalCompetition.Core.AI
{
    /// <summary>
    /// 手札と場札の状態から出せる全ての有効手を探索する。
    /// PlayValidatorに委譲して全有効手を取得する。
    /// </summary>
    public class AIActionEvaluator
    {
        private readonly PlayValidator _playValidator = new PlayValidator();

        /// <summary>
        /// 手札から出せる全ての有効な手を返す。
        /// </summary>
        public List<PlayAction> FindAllValidPlays(List<Card> hand, FieldState field)
        {
            return _playValidator.GetAllValidPlays(hand, field);
        }
    }
}
