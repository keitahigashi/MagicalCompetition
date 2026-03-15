using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Core.Systems
{
    /// <summary>
    /// カードプレイの妥当性を判定する。
    /// 同数字・同色・計算出しの判定と、手札からの全有効手探索を行う。
    /// </summary>
    public class PlayValidator
    {
        private readonly ArithmeticValidator _arithmeticValidator = new ArithmeticValidator();

        /// <summary>
        /// 選択カードが合法なプレイかを判定し、結果を返す。
        /// 同数字 → 同色 → 計算出し の優先順で判定する。
        /// </summary>
        public PlayValidateResult Validate(List<Card> selectedCards, FieldState field)
        {
            if (selectedCards == null || selectedCards.Count == 0)
                return PlayValidateResult.Invalid;

            if (IsSameNumberPlay(selectedCards, field))
                return new PlayValidateResult(true, PlayType.SameNumber);

            if (IsSameColorPlay(selectedCards, field))
                return new PlayValidateResult(true, PlayType.SameColor);

            if (selectedCards.Count >= 2 && selectedCards.Count <= 3)
            {
                var expressions = _arithmeticValidator.FindValidExpressions(selectedCards, field.Number);
                if (expressions.Count > 0)
                    return new PlayValidateResult(true, PlayType.Arithmetic, expressions[0].Cards);
            }

            return PlayValidateResult.Invalid;
        }

        /// <summary>
        /// 全カードの番号が場札の番号と一致するかを判定する（同数字プレイ）。
        /// </summary>
        public bool IsSameNumberPlay(List<Card> cards, FieldState field)
        {
            return cards.Count > 0 && cards.All(c => c.Number == field.Number);
        }

        /// <summary>
        /// 全カードの色が場札の色と一致するかを判定する（同色プレイ）。
        /// 疑似場札（Color == Any）の場合は、全カードが同じ色であれば有効。
        /// </summary>
        public bool IsSameColorPlay(List<Card> cards, FieldState field)
        {
            if (cards.Count == 0)
                return false;

            if (field.Color == CardColor.Any)
            {
                var firstColor = cards[0].Color;
                return cards.All(c => c.Color == firstColor);
            }

            return cards.All(c => c.Color == field.Color);
        }

        /// <summary>
        /// 2〜3枚のカードの加減算で場札数字と一致するかを判定する（計算出し）。
        /// </summary>
        public bool IsArithmeticPlay(List<Card> cards, FieldState field)
        {
            if (cards.Count < 2 || cards.Count > 3)
                return false;

            return _arithmeticValidator.Validate(cards, field.Number);
        }

        /// <summary>
        /// 手札から出せる全ての有効な組み合わせを返す。
        /// 同数字・同色・計算出しの全パターンを網羅し、同一PlayType内の重複を排除する。
        /// </summary>
        public List<PlayAction> GetAllValidPlays(List<Card> hand, FieldState field)
        {
            var results = new List<PlayAction>();
            var seen = new HashSet<string>();

            // 1〜3枚の全組み合わせを生成して判定
            for (int size = 1; size <= 3 && size <= hand.Count; size++)
            {
                foreach (var combo in GetCombinations(hand, size))
                {
                    // 同数字判定
                    if (IsSameNumberPlay(combo, field))
                        AddIfNotDuplicate(results, seen, PlayType.SameNumber, combo);

                    // 同色判定
                    if (IsSameColorPlay(combo, field))
                        AddIfNotDuplicate(results, seen, PlayType.SameColor, combo);

                    // 計算出し判定（2〜3枚のみ）
                    if (size >= 2)
                    {
                        var expressions = _arithmeticValidator.FindValidExpressions(combo, field.Number);
                        if (expressions.Count > 0)
                            AddIfNotDuplicate(results, seen, PlayType.Arithmetic, expressions[0].Cards);
                    }
                }
            }

            return results;
        }

        private void AddIfNotDuplicate(List<PlayAction> results, HashSet<string> seen,
            PlayType type, List<Card> cards)
        {
            var key = type + ":" + string.Join(",",
                cards.OrderBy(c => c.Color).ThenBy(c => c.Number).Select(c => c.ToString()));

            if (seen.Add(key))
                results.Add(PlayAction.CreatePlay(type, new List<Card>(cards)));
        }

        /// <summary>
        /// リストからsize枚の組み合わせを全て生成する。
        /// </summary>
        private IEnumerable<List<Card>> GetCombinations(List<Card> source, int size)
        {
            if (size == 0)
            {
                yield return new List<Card>();
                yield break;
            }

            for (int i = 0; i <= source.Count - size; i++)
            {
                var remaining = source.GetRange(i + 1, source.Count - i - 1);
                foreach (var combo in GetCombinations(remaining, size - 1))
                {
                    combo.Insert(0, source[i]);
                    yield return combo;
                }
            }
        }
    }

    /// <summary>
    /// プレイ判定の結果を保持する。
    /// </summary>
    public class PlayValidateResult
    {
        public bool IsValid { get; }
        public PlayType Type { get; }

        /// <summary>
        /// Arithmetic の場合、計算式の順序に並んだカードリスト。
        /// それ以外の場合は null。
        /// </summary>
        public ReadOnlyCollection<Card> OrderedCards { get; }

        public PlayValidateResult(bool isValid, PlayType type, List<Card> orderedCards = null)
        {
            IsValid = isValid;
            Type = type;
            OrderedCards = orderedCards?.AsReadOnly();
        }

        public static readonly PlayValidateResult Invalid = new PlayValidateResult(false, PlayType.Pass);
    }
}
