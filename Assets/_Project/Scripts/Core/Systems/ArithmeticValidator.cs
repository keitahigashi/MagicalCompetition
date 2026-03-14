using System.Collections.Generic;
using System.Text;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Core.Systems
{
    /// <summary>
    /// 計算出し（加減算）の妥当性を判定する。
    /// 2〜3枚のカードの全順列×全演算子パターンを探索し、
    /// 場札数字（一の位）と一致する式を見つける。
    /// 計算過程でマイナスになる式は無効とする。0は許容。
    /// </summary>
    public class ArithmeticValidator
    {
        /// <summary>
        /// 2〜3枚のカードで場札数字と一致する計算式が存在するかを返す。
        /// </summary>
        public bool Validate(List<Card> cards, int targetNumber)
        {
            return FindValidExpressions(cards, targetNumber).Count > 0;
        }

        /// <summary>
        /// 全ての有効な計算式を返す。
        /// </summary>
        public List<ArithmeticExpression> FindValidExpressions(List<Card> cards, int targetNumber)
        {
            var results = new List<ArithmeticExpression>();
            var permutations = GetPermutations(cards);

            foreach (var perm in permutations)
            {
                var operatorPatterns = GetOperatorPatterns(perm.Count - 1);

                foreach (var ops in operatorPatterns)
                {
                    if (TryEvaluate(perm, ops, out int result) && result % 10 == targetNumber)
                    {
                        results.Add(new ArithmeticExpression(
                            new List<Card>(perm),
                            new List<char>(ops),
                            result,
                            BuildExpressionString(perm, ops, result)
                        ));
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// 左から順に計算し、途中でマイナスになったら false を返す。
        /// </summary>
        private bool TryEvaluate(List<Card> cards, List<char> operators, out int result)
        {
            result = cards[0].Number;

            for (int i = 0; i < operators.Count; i++)
            {
                if (operators[i] == '+')
                    result += cards[i + 1].Number;
                else
                    result -= cards[i + 1].Number;

                if (result < 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// カードリストの全順列を生成する。
        /// </summary>
        private List<List<Card>> GetPermutations(List<Card> cards)
        {
            var result = new List<List<Card>>();

            if (cards.Count <= 1)
            {
                result.Add(new List<Card>(cards));
                return result;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                var remaining = new List<Card>();
                for (int j = 0; j < cards.Count; j++)
                {
                    if (j != i) remaining.Add(cards[j]);
                }

                foreach (var perm in GetPermutations(remaining))
                {
                    perm.Insert(0, cards[i]);
                    result.Add(perm);
                }
            }

            return result;
        }

        /// <summary>
        /// 指定数の演算子(+/-)の全パターンを生成する。
        /// </summary>
        private List<List<char>> GetOperatorPatterns(int count)
        {
            var result = new List<List<char>>();

            int total = 1 << count; // 2^count
            for (int mask = 0; mask < total; mask++)
            {
                var pattern = new List<char>(count);
                for (int i = 0; i < count; i++)
                {
                    pattern.Add((mask & (1 << i)) == 0 ? '+' : '-');
                }
                result.Add(pattern);
            }

            return result;
        }

        private string BuildExpressionString(List<Card> cards, List<char> operators, int result)
        {
            var sb = new StringBuilder();
            sb.Append(cards[0].Number);

            for (int i = 0; i < operators.Count; i++)
            {
                sb.Append(' ');
                sb.Append(operators[i]);
                sb.Append(' ');
                sb.Append(cards[i + 1].Number);
            }

            sb.Append(" = ");
            sb.Append(result);
            return sb.ToString();
        }
    }

    /// <summary>
    /// 計算式の結果を表現するデータクラス。
    /// </summary>
    public class ArithmeticExpression
    {
        public List<Card> Cards { get; }
        public List<char> Operators { get; }
        public int Result { get; }
        public string Expression { get; }

        public ArithmeticExpression(List<Card> cards, List<char> operators, int result, string expression)
        {
            Cards = cards;
            Operators = operators;
            Result = result;
            Expression = expression;
        }
    }
}
