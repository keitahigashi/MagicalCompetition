using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.Systems;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class PlayValidatorGetAllValidPlaysTests
    {
        private PlayValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new PlayValidator();
        }

        // ─── 1. IsArithmeticPlay(2枚) ──────────────────────────────────────

        [Test]
        public void IsArithmeticPlay_TwoCards_3Plus4_Target7_ReturnsTrue()
        {
            var field = new FieldState();
            field.Update(new Card(CardColor.Fire, 7));

            var cards = new List<Card>
            {
                new Card(CardColor.Fire, 3),
                new Card(CardColor.Water, 4)
            };
            Assert.IsTrue(_validator.IsArithmeticPlay(cards, field));
        }

        // ─── 2. GetAllValidPlays: 全パターン返却 ────────────────────────────

        [Test]
        public void GetAllValidPlays_ReturnsSameNumber_SameColor_And_Arithmetic()
        {
            var field = new FieldState();
            field.Update(new Card(CardColor.Fire, 5));

            // 手札: Fire:5(同数字+同色), Fire:3(同色), Water:2(計算出し2+3=5), Earth:9
            var hand = new List<Card>
            {
                new Card(CardColor.Fire, 5),
                new Card(CardColor.Fire, 3),
                new Card(CardColor.Water, 2),
                new Card(CardColor.Earth, 9)
            };

            var plays = _validator.GetAllValidPlays(hand, field);

            Assert.IsTrue(plays.Any(p => p.Type == PlayType.SameNumber),
                "Should contain SameNumber plays");
            Assert.IsTrue(plays.Any(p => p.Type == PlayType.SameColor),
                "Should contain SameColor plays");
            Assert.IsTrue(plays.Any(p => p.Type == PlayType.Arithmetic),
                "Should contain Arithmetic plays");
        }

        // ─── 3. GetAllValidPlays: 出せるカードなし → 空リスト ────────────────

        [Test]
        public void GetAllValidPlays_NoValidPlays_ReturnsEmptyList()
        {
            var field = new FieldState();
            field.Update(new Card(CardColor.Fire, 5));

            // 場札Fire:5に対し、色も番号も合わず計算出しもできない手札
            var hand = new List<Card>
            {
                new Card(CardColor.Water, 2),
                new Card(CardColor.Earth, 4)
            };
            // 2+4=6≠5, |2-4|=2≠5 → 計算出しも不可

            var plays = _validator.GetAllValidPlays(hand, field);
            // 同色・同数字は明らかにないが、計算出しも確認
            var hasValid = plays.Count > 0;
            // 2+4=6, 4-2=2 どちらも5にならないので空のはず
            Assert.AreEqual(0, plays.Count, "Should return empty list when no valid plays exist");
        }

        // ─── 4. 重複排除 ───────────────────────────────────────────────────

        [Test]
        public void GetAllValidPlays_NoDuplicatesWithinSamePlayType()
        {
            var field = new FieldState();
            field.Update(new Card(CardColor.Fire, 5));

            var hand = new List<Card>
            {
                new Card(CardColor.Fire, 5),
                new Card(CardColor.Water, 5),
                new Card(CardColor.Earth, 5)
            };

            var plays = _validator.GetAllValidPlays(hand, field);

            // 同一PlayType内で重複カードセットがないことを確認
            var sameNumberPlays = plays.Where(p => p.Type == PlayType.SameNumber).ToList();
            for (int i = 0; i < sameNumberPlays.Count; i++)
            {
                for (int j = i + 1; j < sameNumberPlays.Count; j++)
                {
                    var set1 = new HashSet<Card>(sameNumberPlays[i].Cards);
                    var set2 = new HashSet<Card>(sameNumberPlays[j].Cards);
                    Assert.IsFalse(set1.SetEquals(set2),
                        "Should not have duplicate card sets within SameNumber plays");
                }
            }
        }

        // ─── 5. 計算出し2枚 ────────────────────────────────────────────────

        [Test]
        public void GetAllValidPlays_Arithmetic_TwoCards_3Plus4Equals7()
        {
            var field = new FieldState();
            field.Update(new Card(CardColor.Fire, 7));

            var hand = new List<Card>
            {
                new Card(CardColor.Water, 3),
                new Card(CardColor.Earth, 4),
                new Card(CardColor.Light, 1)
            };

            var plays = _validator.GetAllValidPlays(hand, field);
            var arithmeticPlays = plays.Where(p => p.Type == PlayType.Arithmetic).ToList();

            // [3,4] → 3+4=7 のパターンが含まれる
            Assert.IsTrue(arithmeticPlays.Any(p =>
                p.Cards.Count == 2 &&
                p.Cards.Any(c => c.Number == 3) &&
                p.Cards.Any(c => c.Number == 4)),
                "Should contain arithmetic play [3,4] for target 7");
        }

        // ─── 6. 計算出し3枚 ────────────────────────────────────────────────

        [Test]
        public void GetAllValidPlays_Arithmetic_ThreeCards_7Minus5Plus1Equals3()
        {
            var field = new FieldState();
            field.Update(new Card(CardColor.Fire, 3));

            var hand = new List<Card>
            {
                new Card(CardColor.Fire, 7),
                new Card(CardColor.Water, 5),
                new Card(CardColor.Earth, 1)
            };

            var plays = _validator.GetAllValidPlays(hand, field);
            var arithmeticPlays = plays.Where(p => p.Type == PlayType.Arithmetic).ToList();

            // [7,5,1] → 7-5+1=3 のパターンが含まれる
            Assert.IsTrue(arithmeticPlays.Any(p =>
                p.Cards.Count == 3 &&
                p.Cards.Any(c => c.Number == 7) &&
                p.Cards.Any(c => c.Number == 5) &&
                p.Cards.Any(c => c.Number == 1)),
                "Should contain arithmetic play [7,5,1] for target 3");
        }
    }
}
