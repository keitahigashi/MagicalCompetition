using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.AI;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class NormalStrategyTests
    {
        private NormalStrategy _strategy;

        [SetUp]
        public void SetUp()
        {
            _strategy = new NormalStrategy();
        }

        private GameState CreateGameStateWithHand(List<Card> hand, Card fieldCard)
        {
            var player = new PlayerState(0, false);
            foreach (var card in hand)
                player.AddToHand(card);

            var state = new GameState(new List<PlayerState> { player });
            state.Field.Update(fieldCard);
            return state;
        }

        // ─── 1. 出せる手あり: カードプレイを返す ────────────────────────────

        [Test]
        public void DecideAction_ValidPlaysExist_ReturnsCardPlay()
        {
            var hand = new List<Card> { new Card(CardColor.Fire, 5) };
            var state = CreateGameStateWithHand(hand, new Card(CardColor.Fire, 5));

            var validPlays = new List<PlayAction>
            {
                PlayAction.CreatePlay(PlayType.SameNumber, new List<Card> { new Card(CardColor.Fire, 5) })
            };

            var result = _strategy.DecideAction(state, validPlays);
            Assert.AreNotEqual(PlayType.Pass, result.Type);
        }

        // ─── 2. 枚数が多い手を優先 ─────────────────────────────────────────

        [Test]
        public void DecideAction_PrefersMoreCards()
        {
            var hand = new List<Card>
            {
                new Card(CardColor.Fire, 5),
                new Card(CardColor.Water, 5),
                new Card(CardColor.Fire, 3)
            };
            var state = CreateGameStateWithHand(hand, new Card(CardColor.Fire, 5));

            var validPlays = new List<PlayAction>
            {
                PlayAction.CreatePlay(PlayType.SameColor, new List<Card> { new Card(CardColor.Fire, 3) }),
                PlayAction.CreatePlay(PlayType.SameNumber, new List<Card>
                {
                    new Card(CardColor.Fire, 5),
                    new Card(CardColor.Water, 5)
                })
            };

            var result = _strategy.DecideAction(state, validPlays);
            Assert.AreEqual(2, result.Cards.Count);
        }

        // ─── 3. 同枚数: 同数字>同色>計算出し ───────────────────────────────

        [Test]
        public void DecideAction_SameCardCount_PrefersTypeOrder()
        {
            var hand = new List<Card>
            {
                new Card(CardColor.Fire, 5),
                new Card(CardColor.Fire, 3)
            };
            var state = CreateGameStateWithHand(hand, new Card(CardColor.Fire, 5));

            var validPlays = new List<PlayAction>
            {
                PlayAction.CreatePlay(PlayType.SameColor, new List<Card> { new Card(CardColor.Fire, 3) }),
                PlayAction.CreatePlay(PlayType.SameNumber, new List<Card> { new Card(CardColor.Water, 5) }),
                PlayAction.CreatePlay(PlayType.Arithmetic, new List<Card>
                {
                    new Card(CardColor.Water, 2),
                    new Card(CardColor.Earth, 3)
                })
            };

            // 1枚同士なら SameNumber が優先
            var singlePlays = validPlays.Where(p => p.Cards.Count == 1).ToList();
            if (singlePlays.Count > 0)
            {
                var result = _strategy.DecideAction(state, singlePlays);
                Assert.AreEqual(PlayType.SameNumber, result.Type);
            }
        }

        // ─── 4. 出せない: パスを返す ────────────────────────────────────────

        [Test]
        public void DecideAction_NoValidPlays_ReturnsPass()
        {
            var hand = new List<Card> { new Card(CardColor.Water, 2) };
            var state = CreateGameStateWithHand(hand, new Card(CardColor.Fire, 5));

            var result = _strategy.DecideAction(state, new List<PlayAction>());
            Assert.AreEqual(PlayType.Pass, result.Type);
        }

        // ─── 5. パス時: 不要カードを山札に戻す ──────────────────────────────

        [Test]
        public void DecideAction_Pass_ReturnsCardToReturn()
        {
            var hand = new List<Card>
            {
                new Card(CardColor.Water, 2),
                new Card(CardColor.Earth, 9)
            };
            var state = CreateGameStateWithHand(hand, new Card(CardColor.Fire, 5));

            var result = _strategy.DecideAction(state, new List<PlayAction>());
            Assert.AreEqual(PlayType.Pass, result.Type);
            Assert.IsTrue(result.Cards.Count > 0, "Pass should include cards to return");
        }

        // ─── 6. SelectFieldColor: 最も多い色 ───────────────────────────────

        [Test]
        public void SelectFieldColor_ReturnsMostFrequentColor()
        {
            var hand = new List<Card>
            {
                new Card(CardColor.Fire, 1),
                new Card(CardColor.Water, 2),
                new Card(CardColor.Water, 3)
            };
            var state = CreateGameStateWithHand(hand, new Card(CardColor.Fire, 5));

            var color = _strategy.SelectFieldColor(state);
            Assert.AreEqual(CardColor.Water, color);
        }

        // ─── 7. SelectFieldColor(同数): 有効な色を返す ──────────────────────

        [Test]
        public void SelectFieldColor_TiedColors_ReturnsValidColor()
        {
            var hand = new List<Card>
            {
                new Card(CardColor.Fire, 1),
                new Card(CardColor.Water, 2)
            };
            var state = CreateGameStateWithHand(hand, new Card(CardColor.Fire, 5));

            var color = _strategy.SelectFieldColor(state);
            // Fire or Water のいずれか（不正値でないこと）
            Assert.IsTrue(color == CardColor.Fire || color == CardColor.Water,
                $"Expected Fire or Water, but got {color}");
        }
    }
}
