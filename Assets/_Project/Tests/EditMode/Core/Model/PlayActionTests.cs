using System.Collections.Generic;
using NUnit.Framework;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class PlayActionTests
    {
        // ─── CreatePlay ───────────────────────────────────────────────────

        [Test]
        public void CreatePlay_SetsTypeAndCards()
        {
            var cards = new List<Card>
            {
                new Card(CardColor.Fire, 5),
                new Card(CardColor.Water, 5)
            };

            var action = PlayAction.CreatePlay(PlayType.SameNumber, cards);

            Assert.AreEqual(PlayType.SameNumber, action.Type);
            Assert.AreEqual(2, action.Cards.Count);
            Assert.AreEqual(new Card(CardColor.Fire, 5), action.Cards[0]);
            Assert.AreEqual(new Card(CardColor.Water, 5), action.Cards[1]);
        }

        // ─── CreatePass ───────────────────────────────────────────────────

        [Test]
        public void CreatePass_TypeIsPass_AndCardsAreSet()
        {
            var cardsToReturn = new List<Card>
            {
                new Card(CardColor.Earth, 3)
            };

            var action = PlayAction.CreatePass(cardsToReturn);

            Assert.AreEqual(PlayType.Pass, action.Type);
            Assert.AreEqual(1, action.Cards.Count);
            Assert.AreEqual(new Card(CardColor.Earth, 3), action.Cards[0]);
        }

        // ─── LastCard ─────────────────────────────────────────────────────

        [Test]
        public void LastCard_MultipleCards_ReturnsLastCard()
        {
            var cards = new List<Card>
            {
                new Card(CardColor.Fire, 1),
                new Card(CardColor.Wind, 7),
                new Card(CardColor.Light, 9)
            };

            var action = PlayAction.CreatePlay(PlayType.Arithmetic, cards);

            Assert.AreEqual(new Card(CardColor.Light, 9), action.LastCard);
        }

        [Test]
        public void LastCard_EmptyList_ReturnsNull()
        {
            var action = PlayAction.CreatePass(new List<Card>());

            Assert.IsNull(action.LastCard);
        }
    }
}
