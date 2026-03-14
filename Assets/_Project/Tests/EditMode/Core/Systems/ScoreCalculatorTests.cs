using System.Collections.Generic;
using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.Systems;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class ScoreCalculatorTests
    {
        private ScoreCalculator _calculator;

        [SetUp]
        public void SetUp()
        {
            _calculator = new ScoreCalculator();
        }

        // ─── 5. 他PL2人の残カード合計 ──────────────────────────────────────

        [Test]
        public void Calculate_SumsOtherPlayersRemainingCards()
        {
            var winner = new PlayerState(0, true); // 0枚

            var loser1 = new PlayerState(1, false);
            loser1.AddToHand(new Card(CardColor.Fire, 1));
            loser1.AddToHand(new Card(CardColor.Fire, 2));
            loser1.Deck.Add(new Card(CardColor.Water, 3));
            // loser1: 手札2 + 山札1 = 3枚

            var loser2 = new PlayerState(2, false);
            loser2.AddToHand(new Card(CardColor.Earth, 5));
            loser2.Deck.Add(new Card(CardColor.Light, 7));
            loser2.Deck.Add(new Card(CardColor.Light, 8));
            // loser2: 手札1 + 山札2 = 3枚

            var allPlayers = new List<PlayerState> { winner, loser1, loser2 };

            Assert.AreEqual(6, _calculator.Calculate(winner, allPlayers));
        }

        // ─── 6. 勝者のカードは含まれない ────────────────────────────────────

        [Test]
        public void Calculate_ExcludesWinnerCards()
        {
            var winner = new PlayerState(0, true);
            // 通常ありえないが、安全のため勝者にカードが残っている場合
            winner.Deck.Add(new Card(CardColor.Fire, 9));

            var loser = new PlayerState(1, false);
            loser.AddToHand(new Card(CardColor.Water, 5));
            // loser: 1枚

            var allPlayers = new List<PlayerState> { winner, loser };

            // 勝者の1枚は含まれず、敗者の1枚のみ
            Assert.AreEqual(1, _calculator.Calculate(winner, allPlayers));
        }
    }
}
