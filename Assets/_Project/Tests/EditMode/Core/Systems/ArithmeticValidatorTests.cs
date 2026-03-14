using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.Systems;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class ArithmeticValidatorTests
    {
        private ArithmeticValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new ArithmeticValidator();
        }

        // ─── 1. 2枚足し算: [3,4] target=7 ─────────────────────────────────

        [Test]
        public void Validate_TwoCards_Addition_3Plus4Equals7_ReturnsTrue()
        {
            var cards = new List<Card> { new Card(CardColor.Fire, 3), new Card(CardColor.Water, 4) };
            Assert.IsTrue(_validator.Validate(cards, 7));
        }

        // ─── 2. 2枚引き算: [9,2] target=7 ──────────────────────────────────

        [Test]
        public void Validate_TwoCards_Subtraction_9Minus2Equals7_ReturnsTrue()
        {
            var cards = new List<Card> { new Card(CardColor.Fire, 9), new Card(CardColor.Water, 2) };
            Assert.IsTrue(_validator.Validate(cards, 7));
        }

        // ─── 3. 2枚引き算(逆順): [2,9] target=7 ────────────────────────────

        [Test]
        public void Validate_TwoCards_ReverseOrder_2And9_Target7_ReturnsTrue()
        {
            var cards = new List<Card> { new Card(CardColor.Fire, 2), new Card(CardColor.Water, 9) };
            Assert.IsTrue(_validator.Validate(cards, 7));
        }

        // ─── 4. 2枚マイナス禁止: [2,5] target=3 ────────────────────────────

        [Test]
        public void Validate_TwoCards_NegativeExcluded_5Minus2Valid_2Minus5Invalid()
        {
            var cards = new List<Card> { new Card(CardColor.Fire, 2), new Card(CardColor.Water, 5) };
            // 5-2=3 は有効なので Validate は true
            Assert.IsTrue(_validator.Validate(cards, 3));

            // FindValidExpressions で 2-5=-3 のパターンが含まれないことを確認
            var expressions = _validator.FindValidExpressions(cards, 3);
            foreach (var expr in expressions)
            {
                Assert.GreaterOrEqual(expr.Result, 0,
                    $"Expression '{expr.Expression}' has negative result {expr.Result}");
            }
        }

        // ─── 5. 3枚混合: [7,5,1] target=3 ──────────────────────────────────

        [Test]
        public void Validate_ThreeCards_Mixed_7Minus5Plus1Equals3_ReturnsTrue()
        {
            var cards = new List<Card> { new Card(CardColor.Fire, 7), new Card(CardColor.Water, 5), new Card(CardColor.Light, 1) };
            Assert.IsTrue(_validator.Validate(cards, 3));
        }

        // ─── 6. 3枚マイナス禁止: [5,1,7] target=3 ──────────────────────────

        [Test]
        public void Validate_ThreeCards_NegativeResult_StillValidIfOtherPermutationWorks()
        {
            // [5,1,7] は並び替えれば 7-5+1=3 が成立するので true
            var cards = new List<Card> { new Card(CardColor.Fire, 5), new Card(CardColor.Water, 1), new Card(CardColor.Light, 7) };
            Assert.IsTrue(_validator.Validate(cards, 3));

            // ただし 5+1-7=-1 のような無効式は FindValidExpressions に含まれない
            var expressions = _validator.FindValidExpressions(cards, 3);
            foreach (var expr in expressions)
            {
                Assert.GreaterOrEqual(expr.Result, 0,
                    $"Expression '{expr.Expression}' has negative result {expr.Result}");
            }
        }

        // ─── 7. 計算途中の0許容: [5,5] target=0 ────────────────────────────

        [Test]
        public void Validate_TwoCards_ZeroResult_5Minus5Equals0_ReturnsTrue()
        {
            var cards = new List<Card> { new Card(CardColor.Fire, 5), new Card(CardColor.Water, 5) };
            Assert.IsTrue(_validator.Validate(cards, 0));
        }

        // ─── 8. 一の位判定: [8,9] target=7 ──────────────────────────────────

        [Test]
        public void Validate_TwoCards_OnesDigit_8Plus9Equals17_Target7_ReturnsTrue()
        {
            var cards = new List<Card> { new Card(CardColor.Fire, 8), new Card(CardColor.Water, 9) };
            Assert.IsTrue(_validator.Validate(cards, 7));
        }

        // ─── 9. FindValidExpressions: 複数の有効式 ──────────────────────────

        [Test]
        public void FindValidExpressions_ReturnsAllValidExpressions()
        {
            // [3,4] target=7: 3+4=7 と 4+3=7 は同値だが順列として両方返る可能性がある
            // 少なくとも1つは返ること
            var cards = new List<Card> { new Card(CardColor.Fire, 3), new Card(CardColor.Water, 4) };
            var expressions = _validator.FindValidExpressions(cards, 7);
            Assert.IsTrue(expressions.Count >= 1,
                "Should return at least one valid expression for [3,4] target=7");

            // 全式の結果が一の位で target と一致すること
            foreach (var expr in expressions)
            {
                Assert.AreEqual(7, expr.Result % 10,
                    $"Expression '{expr.Expression}' result {expr.Result} ones digit should be 7");
            }

            // Expression 文字列が空でないこと
            foreach (var expr in expressions)
            {
                Assert.IsNotEmpty(expr.Expression);
            }
        }
    }
}
