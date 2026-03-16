using System.Collections.Generic;
using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.Systems;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class DrawSystemTests
    {
        private DrawSystem _drawSystem;

        [SetUp]
        public void SetUp()
        {
            _drawSystem = new DrawSystem();
        }

        private PlayerState CreatePlayerWithDeck(int deckSize)
        {
            var player = new PlayerState(0, true);
            for (int i = 1; i <= deckSize; i++)
                player.Deck.Add(new Card(CardColor.Fire, (i - 1) % 9 + 1));
            return player;
        }

        // ─── 1. InitialDraw: 手札3枚 ───────────────────────────────────────

        [Test]
        public void InitialDraw_EachPlayerGets3Cards()
        {
            var players = new List<PlayerState>
            {
                CreatePlayerWithDeck(10),
                CreatePlayerWithDeck(10)
            };

            _drawSystem.InitialDraw(players);

            Assert.AreEqual(3, players[0].Hand.Count);
            Assert.AreEqual(3, players[1].Hand.Count);
        }

        // ─── 2. InitialDraw: 山札が3枚減る ─────────────────────────────────

        [Test]
        public void InitialDraw_DeckDecreasesByThree()
        {
            var players = new List<PlayerState> { CreatePlayerWithDeck(10) };

            _drawSystem.InitialDraw(players);

            Assert.AreEqual(7, players[0].Deck.Count);
        }

        // ─── 3. Refill(手札0枚): 3枚引く ───────────────────────────────────

        [Test]
        public void Refill_EmptyHand_Draws3Cards()
        {
            var player = CreatePlayerWithDeck(10);

            _drawSystem.Refill(player);

            Assert.AreEqual(3, player.Hand.Count);
            Assert.AreEqual(7, player.Deck.Count);
        }

        // ─── 4. Refill(手札1枚): 2枚引く ───────────────────────────────────

        [Test]
        public void Refill_OneCardInHand_Draws2Cards()
        {
            var player = CreatePlayerWithDeck(10);
            player.AddToHand(new Card(CardColor.Water, 1));

            _drawSystem.Refill(player);

            Assert.AreEqual(3, player.Hand.Count);
            Assert.AreEqual(8, player.Deck.Count);
        }

        // ─── 5. Refill(手札3枚): 引かない ──────────────────────────────────

        [Test]
        public void Refill_FullHand_DrawsNothing()
        {
            var player = CreatePlayerWithDeck(10);
            player.AddToHand(new Card(CardColor.Water, 1));
            player.AddToHand(new Card(CardColor.Water, 2));
            player.AddToHand(new Card(CardColor.Water, 3));

            _drawSystem.Refill(player);

            Assert.AreEqual(3, player.Hand.Count);
            Assert.AreEqual(10, player.Deck.Count);
        }

        // ─── 6. Refill(山札不足): 残り全部引く ──────────────────────────────

        [Test]
        public void Refill_InsufficientDeck_DrawsAllRemaining()
        {
            var player = CreatePlayerWithDeck(2); // 山札2枚、手札0枚

            _drawSystem.Refill(player);

            Assert.AreEqual(2, player.Hand.Count);
            Assert.AreEqual(0, player.Deck.Count);
        }

        // ─── 7. Refill(山札空): IsReach=true ───────────────────────────────

        [Test]
        public void Refill_DeckBecomesEmpty_SetsIsReachTrue()
        {
            var player = CreatePlayerWithDeck(2); // 山札2枚

            _drawSystem.Refill(player);

            Assert.IsTrue(player.IsReach);
        }

        // ─── 8. Refill(既にリーチ・山札あり): 手札を補充する ──────────────

        [Test]
        public void Refill_AlreadyReach_WithDeck_StillRefills()
        {
            var player = CreatePlayerWithDeck(2);
            player.IsReach = true;
            // 山札2枚・手札0枚 → 2枚引いて手札2枚になる

            _drawSystem.Refill(player);

            Assert.AreEqual(2, player.Hand.Count);
            Assert.AreEqual(0, player.Deck.Count);
        }

        // ─── 9. Refill(既にリーチ・山札なし): 何もしない ─────────────────

        [Test]
        public void Refill_AlreadyReach_EmptyDeck_DoesNothing()
        {
            var player = CreatePlayerWithDeck(0);
            player.IsReach = true;
            player.AddToHand(new Card(CardColor.Fire, 1));

            _drawSystem.Refill(player);

            Assert.AreEqual(1, player.Hand.Count);
            Assert.AreEqual(0, player.Deck.Count);
        }
    }
}
