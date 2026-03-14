using System.Collections.Generic;
using System.Linq;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Core.AI
{
    /// <summary>
    /// Normal難易度のAI戦略。
    /// 多く出せる手を優先し、同数字 > 同色 > 計算出し の優先順位で選択する。
    /// </summary>
    public class NormalStrategy : IAIStrategy
    {
        /// <summary>
        /// 全有効手から最適な手を選択する。
        /// 出せない場合はパスを返す。
        /// </summary>
        public PlayAction DecideAction(GameState state, List<PlayAction> validPlays)
        {
            if (validPlays == null || validPlays.Count == 0)
                return CreatePassAction(state);

            // 枚数が多い手を優先 → 同枚数なら PlayType 優先順（SameNumber > SameColor > Arithmetic）
            var best = validPlays
                .OrderByDescending(p => p.Cards.Count)
                .ThenBy(p => GetTypePriority(p.Type))
                .First();

            return best;
        }

        /// <summary>
        /// 手札に最も多い色を返す。
        /// </summary>
        public CardColor SelectFieldColor(GameState state)
        {
            var hand = state.CurrentPlayer.Hand;
            if (hand.Count == 0)
                return CardColor.Fire;

            return hand
                .GroupBy(c => c.Color)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key) // 同数なら enum 順で決定的に
                .First()
                .Key;
        }

        private int GetTypePriority(PlayType type)
        {
            switch (type)
            {
                case PlayType.SameNumber: return 0;
                case PlayType.SameColor: return 1;
                case PlayType.Arithmetic: return 2;
                default: return 3;
            }
        }

        private PlayAction CreatePassAction(GameState state)
        {
            // パス時は手札から1枚選んで山札に戻す（使いにくいカードを優先）
            var hand = state.CurrentPlayer.Hand;
            var field = state.Field;

            if (hand.Count == 0)
                return PlayAction.CreatePass(new List<Card>());

            // 場札と数字・色が離れているカードを「使いにくい」と判定
            var cardToReturn = hand
                .OrderBy(c => ScoreCardUsefulness(c, field))
                .First();

            return PlayAction.CreatePass(new List<Card> { cardToReturn });
        }

        private int ScoreCardUsefulness(Card card, FieldState field)
        {
            int score = 0;

            // 同色ボーナス
            if (card.Color == field.Color)
                score += 10;

            // 同数字ボーナス
            if (card.Number == field.Number)
                score += 10;

            // 数字が近いほど有用
            int diff = card.Number > field.Number
                ? card.Number - field.Number
                : field.Number - card.Number;
            score += (9 - diff);

            return score;
        }
    }
}
