using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.AI;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class AIActionEvaluatorTests
    {
        private AIActionEvaluator _evaluator;

        [SetUp]
        public void SetUp()
        {
            _evaluator = new AIActionEvaluator();
        }

        // ─── 1. 同数字の有効手が含まれる ────────────────────────────────────

        [Test]
        public void FindAllValidPlays_ContainsSameNumberPlays()
        {
            var field = new FieldState();
            field.Update(new Card(CardColor.Fire, 5));

            var hand = new List<Card>
            {
                new Card(CardColor.Water, 5),
                new Card(CardColor.Earth, 3)
            };

            var plays = _evaluator.FindAllValidPlays(hand, field);
            Assert.IsTrue(plays.Any(p => p.Type == PlayType.SameNumber));
        }

        // ─── 2. 同色の有効手が含まれる ──────────────────────────────────────

        [Test]
        public void FindAllValidPlays_ContainsSameColorPlays()
        {
            var field = new FieldState();
            field.Update(new Card(CardColor.Fire, 5));

            var hand = new List<Card>
            {
                new Card(CardColor.Fire, 3),
                new Card(CardColor.Water, 7)
            };

            var plays = _evaluator.FindAllValidPlays(hand, field);
            Assert.IsTrue(plays.Any(p => p.Type == PlayType.SameColor));
        }

        // ─── 3. 計算出しの有効手が含まれる ──────────────────────────────────

        [Test]
        public void FindAllValidPlays_ContainsArithmeticPlays()
        {
            var field = new FieldState();
            field.Update(new Card(CardColor.Fire, 7));

            var hand = new List<Card>
            {
                new Card(CardColor.Water, 3),
                new Card(CardColor.Earth, 4),
                new Card(CardColor.Light, 9)
            };

            var plays = _evaluator.FindAllValidPlays(hand, field);
            Assert.IsTrue(plays.Any(p => p.Type == PlayType.Arithmetic));
        }

        // ─── 4. 出せるカードなし → 空リスト ─────────────────────────────────

        [Test]
        public void FindAllValidPlays_NoValidPlays_ReturnsEmptyList()
        {
            var field = new FieldState();
            field.Update(new Card(CardColor.Fire, 5));

            var hand = new List<Card>
            {
                new Card(CardColor.Water, 2),
                new Card(CardColor.Earth, 4)
            };

            var plays = _evaluator.FindAllValidPlays(hand, field);
            Assert.AreEqual(0, plays.Count);
        }

        // ─── 5. 全探索で漏れなく列挙 ───────────────────────────────────────

        [Test]
        public void FindAllValidPlays_ComprehensiveEnumeration()
        {
            var field = new FieldState();
            field.Update(new Card(CardColor.Fire, 5));

            // 手札: Fire:5(同数字+同色), Fire:3(同色), Water:2(計算出し2+3=5可能)
            var hand = new List<Card>
            {
                new Card(CardColor.Fire, 5),
                new Card(CardColor.Fire, 3),
                new Card(CardColor.Water, 2)
            };

            var plays = _evaluator.FindAllValidPlays(hand, field);

            Assert.IsTrue(plays.Any(p => p.Type == PlayType.SameNumber),
                "Should contain SameNumber plays");
            Assert.IsTrue(plays.Any(p => p.Type == PlayType.SameColor),
                "Should contain SameColor plays");
            Assert.IsTrue(plays.Any(p => p.Type == PlayType.Arithmetic),
                "Should contain Arithmetic plays");

            // 全ての有効手が含まれている（少なくとも3種類）
            Assert.GreaterOrEqual(plays.Count, 3,
                "Should enumerate all valid plays without omission");
        }
    }
}
