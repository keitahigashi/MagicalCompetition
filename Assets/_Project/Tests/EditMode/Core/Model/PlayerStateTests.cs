using System.Collections.Generic;
using NUnit.Framework;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class PlayerStateTests
    {
        // ─── ヘルパー ────────────────────────────────────────────────────────

        private static Card MakeCard(CardColor color = CardColor.Fire, int number = 1)
            => new Card(color, number);

        // ─── 1. 初期状態 ─────────────────────────────────────────────────────

        [Test]
        public void PlayerState_Initial_HandAndDeckAreEmpty_IsReachFalse()
        {
            var player = new PlayerState(0, isHuman: true);

            Assert.IsNotNull(player.Hand);
            Assert.IsNotNull(player.Deck);
            Assert.AreEqual(0, player.Hand.Count);
            Assert.AreEqual(0, player.Deck.Count);
            Assert.IsFalse(player.IsReach);
        }

        // ─── 2. AddToHand ─────────────────────────────────────────────────────

        [Test]
        public void AddToHand_CardIsInHand()
        {
            var player = new PlayerState(1, isHuman: false);
            var card = MakeCard(CardColor.Water, 3);

            player.AddToHand(card);

            Assert.AreEqual(1, player.Hand.Count);
            Assert.IsTrue(player.Hand.Contains(card));
        }

        // ─── 3. RemoveFromHand（1枚） ─────────────────────────────────────────

        [Test]
        public void RemoveFromHand_Single_CardIsRemovedFromHand()
        {
            var player = new PlayerState(0, isHuman: true);
            var cardA = MakeCard(CardColor.Fire, 1);
            var cardB = MakeCard(CardColor.Wind, 7);
            player.AddToHand(cardA);
            player.AddToHand(cardB);

            player.RemoveFromHand(cardA);

            Assert.AreEqual(1, player.Hand.Count);
            Assert.IsFalse(player.Hand.Contains(cardA));
            Assert.IsTrue(player.Hand.Contains(cardB));
        }

        // ─── 4. RemoveFromHand（複数枚） ──────────────────────────────────────

        [Test]
        public void RemoveFromHand_Multiple_AllSpecifiedCardsRemoved()
        {
            var player = new PlayerState(0, isHuman: true);
            var cardA = MakeCard(CardColor.Fire, 2);
            var cardB = MakeCard(CardColor.Water, 4);
            var cardC = MakeCard(CardColor.Earth, 6);
            player.AddToHand(cardA);
            player.AddToHand(cardB);
            player.AddToHand(cardC);

            player.RemoveFromHand(new List<Card> { cardA, cardC });

            Assert.AreEqual(1, player.Hand.Count);
            Assert.IsTrue(player.Hand.Contains(cardB));
            Assert.IsFalse(player.Hand.Contains(cardA));
            Assert.IsFalse(player.Hand.Contains(cardC));
        }

        // ─── 5. DrawFromDeck（山札あり） ──────────────────────────────────────

        [Test]
        public void DrawFromDeck_ReturnsTopCard_AndRemovesItFromDeck()
        {
            var player = new PlayerState(0, isHuman: false);
            var topCard    = MakeCard(CardColor.Light, 1);
            var bottomCard = MakeCard(CardColor.Wind,  9);
            // Deck[0] が先頭（インデックス 0 = 山札トップ）
            player.Deck.Add(topCard);
            player.Deck.Add(bottomCard);

            var drawn = player.DrawFromDeck();

            Assert.AreEqual(topCard, drawn);
            Assert.AreEqual(1, player.Deck.Count);
            Assert.IsFalse(player.Deck.Contains(topCard));
        }

        // ─── 6. DrawFromDeck（山札空） ────────────────────────────────────────

        [Test]
        public void DrawFromDeck_EmptyDeck_ReturnsNull()
        {
            var player = new PlayerState(0, isHuman: false);

            var drawn = player.DrawFromDeck();

            Assert.IsNull(drawn);
        }

        // ─── 7. ReturnToDeckBottom ────────────────────────────────────────────

        [Test]
        public void ReturnToDeckBottom_CardsAppendedAtBottom()
        {
            var player = new PlayerState(0, isHuman: true);
            var existing = MakeCard(CardColor.Fire, 5);
            player.Deck.Add(existing);

            var returnA = MakeCard(CardColor.Water, 2);
            var returnB = MakeCard(CardColor.Earth, 8);
            player.ReturnToDeckBottom(new List<Card> { returnA, returnB });

            Assert.AreEqual(3, player.Deck.Count);
            // 末尾に順序通り追加されていること
            Assert.AreEqual(returnA, player.Deck[1]);
            Assert.AreEqual(returnB, player.Deck[2]);
        }

        // ─── 8. TotalCardCount ────────────────────────────────────────────────

        [Test]
        public void TotalCardCount_IsHandPlusDeck()
        {
            var player = new PlayerState(0, isHuman: true);
            player.AddToHand(MakeCard(CardColor.Fire,  1));
            player.AddToHand(MakeCard(CardColor.Water, 2));
            player.Deck.Add(MakeCard(CardColor.Wind,   3));
            player.Deck.Add(MakeCard(CardColor.Earth,  4));
            player.Deck.Add(MakeCard(CardColor.Light,  5));

            Assert.AreEqual(5, player.TotalCardCount);
        }

        // ─── 9. HasWon ───────────────────────────────────────────────────────

        [Test]
        public void HasWon_WhenAllCardsGone_ReturnsTrue()
        {
            var player = new PlayerState(0, isHuman: true);
            // カードを1枚ずつ追加してから全て除去
            var card = MakeCard();
            player.AddToHand(card);
            player.RemoveFromHand(card);

            Assert.IsTrue(player.HasWon);
        }
    }
}
