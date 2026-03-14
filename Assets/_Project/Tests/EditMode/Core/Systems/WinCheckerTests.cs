using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.Systems;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class WinCheckerTests
    {
        private WinChecker _checker;

        [SetUp]
        public void SetUp()
        {
            _checker = new WinChecker();
        }

        // ─── 1. 手札0・山札0 → true ────────────────────────────────────────

        [Test]
        public void Check_NoHandNoDeck_ReturnsTrue()
        {
            var player = new PlayerState(0, true);
            Assert.IsTrue(_checker.Check(player));
        }

        // ─── 2. 手札1・山札0 → false ───────────────────────────────────────

        [Test]
        public void Check_HandRemaining_ReturnsFalse()
        {
            var player = new PlayerState(0, true);
            player.AddToHand(new Card(CardColor.Fire, 5));
            Assert.IsFalse(_checker.Check(player));
        }

        // ─── 3. 手札0・山札1 → false ───────────────────────────────────────

        [Test]
        public void Check_DeckRemaining_ReturnsFalse()
        {
            var player = new PlayerState(0, true);
            player.Deck.Add(new Card(CardColor.Fire, 5));
            Assert.IsFalse(_checker.Check(player));
        }

        // ─── 4. リーチ後に手札全出し → true ────────────────────────────────

        [Test]
        public void Check_ReachThenPlayAllHand_ReturnsTrue()
        {
            var player = new PlayerState(0, true);
            // リーチ状態にして手札を3枚持たせる
            player.IsReach = true;
            player.AddToHand(new Card(CardColor.Fire, 1));
            player.AddToHand(new Card(CardColor.Fire, 2));
            player.AddToHand(new Card(CardColor.Fire, 3));

            // 手札を全て出す
            player.RemoveFromHand(new Card(CardColor.Fire, 1));
            player.RemoveFromHand(new Card(CardColor.Fire, 2));
            player.RemoveFromHand(new Card(CardColor.Fire, 3));

            Assert.IsTrue(_checker.Check(player));
        }
    }
}
