using System.Collections.Generic;
using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.Systems;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class PlayValidatorTests
    {
        private PlayValidator _validator;
        private FieldState _field;

        [SetUp]
        public void SetUp()
        {
            _validator = new PlayValidator();
            _field = new FieldState();
            _field.Update(new Card(CardColor.Fire, 5)); // 場札: Fire:5
        }

        // ─── 同数字判定 ─────────────────────────────────────────────────────

        [Test]
        public void IsSameNumberPlay_OneCard_SameNumber_ReturnsTrue()
        {
            var cards = new List<Card> { new Card(CardColor.Water, 5) };
            Assert.IsTrue(_validator.IsSameNumberPlay(cards, _field));
        }

        [Test]
        public void IsSameNumberPlay_TwoCards_SameNumber_ReturnsTrue()
        {
            var cards = new List<Card>
            {
                new Card(CardColor.Water, 5),
                new Card(CardColor.Earth, 5)
            };
            Assert.IsTrue(_validator.IsSameNumberPlay(cards, _field));
        }

        [Test]
        public void IsSameNumberPlay_ThreeCards_SameNumber_ReturnsTrue()
        {
            var cards = new List<Card>
            {
                new Card(CardColor.Water, 5),
                new Card(CardColor.Earth, 5),
                new Card(CardColor.Light, 5)
            };
            Assert.IsTrue(_validator.IsSameNumberPlay(cards, _field));
        }

        [Test]
        public void IsSameNumberPlay_DifferentNumber_ReturnsFalse()
        {
            var cards = new List<Card> { new Card(CardColor.Water, 3) };
            Assert.IsFalse(_validator.IsSameNumberPlay(cards, _field));
        }

        // ─── 同色判定 ───────────────────────────────────────────────────────

        [Test]
        public void IsSameColorPlay_OneCard_SameColor_ReturnsTrue()
        {
            var cards = new List<Card> { new Card(CardColor.Fire, 3) };
            Assert.IsTrue(_validator.IsSameColorPlay(cards, _field));
        }

        [Test]
        public void IsSameColorPlay_TwoCards_SameColor_ReturnsTrue()
        {
            var cards = new List<Card>
            {
                new Card(CardColor.Fire, 3),
                new Card(CardColor.Fire, 7)
            };
            Assert.IsTrue(_validator.IsSameColorPlay(cards, _field));
        }

        [Test]
        public void IsSameColorPlay_MixedColors_ReturnsFalse()
        {
            var cards = new List<Card>
            {
                new Card(CardColor.Fire, 3),
                new Card(CardColor.Water, 7)
            };
            Assert.IsFalse(_validator.IsSameColorPlay(cards, _field));
        }

        // ─── 疑似場札 ──────────────────────────────────────────────────────

        [Test]
        public void VirtualField_SameNumber_And_UnifiedColor_BothValid()
        {
            // 疑似場札: Any色、番号5
            var virtualField = new FieldState();
            virtualField.SetVirtual(CardColor.Any);

            // 同数字: 番号5で一致 → valid
            var sameNumCards = new List<Card> { new Card(CardColor.Water, 5) };
            Assert.IsTrue(_validator.IsSameNumberPlay(sameNumCards, virtualField));

            // 同色: Anyの場合、全カードが同色であれば valid
            var sameColorCards = new List<Card>
            {
                new Card(CardColor.Water, 3),
                new Card(CardColor.Water, 7)
            };
            Assert.IsTrue(_validator.IsSameColorPlay(sameColorCards, virtualField));
        }
    }
}
