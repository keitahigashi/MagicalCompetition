using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.Systems;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class CardDistributorTests
    {
        private CardDistributor _distributor;

        [SetUp]
        public void SetUp()
        {
            _distributor = new CardDistributor();
        }

        // ─── CreateDeck ─────────────────────────────────────────────────────

        [Test]
        public void CreateDeck_Returns45Cards_AllColorsAndNumbers()
        {
            var deck = _distributor.CreateDeck();

            Assert.AreEqual(45, deck.Count);

            // 5色（Any除く）× 9番号 = 45枚
            var colors = new[] { CardColor.Fire, CardColor.Water, CardColor.Light, CardColor.Earth, CardColor.Wind };
            foreach (var color in colors)
            {
                for (int number = 1; number <= 9; number++)
                {
                    var target = new Card(color, number);
                    Assert.IsTrue(deck.Contains(target),
                        $"Deck should contain {color}:{number}");
                }
            }
        }

        // ─── Shuffle ────────────────────────────────────────────────────────

        [Test]
        public void Shuffle_PreservesAllCards_CountAndElements()
        {
            var deck = _distributor.CreateDeck();
            var original = new List<Card>(deck);

            _distributor.Shuffle(deck, new System.Random(42));

            Assert.AreEqual(45, deck.Count);

            // 全要素が保持されていることを確認
            var sortedOriginal = original.OrderBy(c => c.Color).ThenBy(c => c.Number).ToList();
            var sortedShuffled = deck.OrderBy(c => c.Color).ThenBy(c => c.Number).ToList();
            CollectionAssert.AreEqual(sortedOriginal, sortedShuffled);
        }

        // ─── Distribute(2人) ────────────────────────────────────────────────

        [Test]
        public void Distribute_2Players_22CardsEach_1Excluded()
        {
            var deck = _distributor.CreateDeck();
            var result = _distributor.Distribute(deck, 2);

            Assert.AreEqual(2, result.PlayerDecks.Count);
            Assert.AreEqual(22, result.PlayerDecks[0].Count);
            Assert.AreEqual(22, result.PlayerDecks[1].Count);
            Assert.AreEqual(1, result.ExcludedCards.Count);
        }

        // ─── Distribute(3人) ────────────────────────────────────────────────

        [Test]
        public void Distribute_3Players_15CardsEach_0Excluded()
        {
            var deck = _distributor.CreateDeck();
            var result = _distributor.Distribute(deck, 3);

            Assert.AreEqual(3, result.PlayerDecks.Count);
            Assert.AreEqual(15, result.PlayerDecks[0].Count);
            Assert.AreEqual(15, result.PlayerDecks[1].Count);
            Assert.AreEqual(15, result.PlayerDecks[2].Count);
            Assert.AreEqual(0, result.ExcludedCards.Count);
        }

        // ─── Distribute(4人) ────────────────────────────────────────────────

        [Test]
        public void Distribute_4Players_11CardsEach_1Excluded()
        {
            var deck = _distributor.CreateDeck();
            var result = _distributor.Distribute(deck, 4);

            Assert.AreEqual(4, result.PlayerDecks.Count);
            foreach (var playerDeck in result.PlayerDecks)
                Assert.AreEqual(11, playerDeck.Count);
            Assert.AreEqual(1, result.ExcludedCards.Count);
        }

        // ─── Distribute(5人) ────────────────────────────────────────────────

        [Test]
        public void Distribute_5Players_9CardsEach_0Excluded()
        {
            var deck = _distributor.CreateDeck();
            var result = _distributor.Distribute(deck, 5);

            Assert.AreEqual(5, result.PlayerDecks.Count);
            foreach (var playerDeck in result.PlayerDecks)
                Assert.AreEqual(9, playerDeck.Count);
            Assert.AreEqual(0, result.ExcludedCards.Count);
        }
    }
}
