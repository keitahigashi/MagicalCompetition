using System;
using NUnit.Framework;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class CardTests
    {
        // ─── CardColor enum ───────────────────────────────────────────────

        [Test]
        public void CardColor_HasSixValues()
        {
            var values = Enum.GetValues(typeof(CardColor));
            Assert.AreEqual(6, values.Length);
        }

        [Test]
        public void CardColor_AnyExists()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(CardColor), CardColor.Any));
        }

        // ─── GamePhase enum ───────────────────────────────────────────────

        [Test]
        public void GamePhase_HasEightValues()
        {
            var values = Enum.GetValues(typeof(GamePhase));
            Assert.AreEqual(8, values.Length);
        }

        // ─── PlayType enum ────────────────────────────────────────────────

        [Test]
        public void PlayType_HasFourValues()
        {
            var values = Enum.GetValues(typeof(PlayType));
            Assert.AreEqual(4, values.Length);
        }

        // ─── Card 生成 ────────────────────────────────────────────────────

        [Test]
        public void Card_Create_StoresColorAndNumber()
        {
            var card = new Card(CardColor.Fire, 5);
            Assert.AreEqual(CardColor.Fire, card.Color);
            Assert.AreEqual(5, card.Number);
        }

        // ─── Card 等値 ────────────────────────────────────────────────────

        [Test]
        public void Card_Equals_SameColorAndNumber_ReturnsTrue()
        {
            var a = new Card(CardColor.Fire, 5);
            var b = new Card(CardColor.Fire, 5);
            Assert.AreEqual(a, b);
        }

        [Test]
        public void Card_Equals_DifferentColor_ReturnsFalse()
        {
            var a = new Card(CardColor.Fire, 5);
            var b = new Card(CardColor.Water, 5);
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void Card_Equals_DifferentNumber_ReturnsFalse()
        {
            var a = new Card(CardColor.Fire, 5);
            var b = new Card(CardColor.Fire, 3);
            Assert.AreNotEqual(a, b);
        }

        // ─── Card.ToString ────────────────────────────────────────────────

        [Test]
        public void Card_ToString_ReturnsColorColonNumber()
        {
            var card = new Card(CardColor.Fire, 5);
            Assert.AreEqual("Fire:5", card.ToString());
        }

        // ─── Card.GetHashCode ─────────────────────────────────────────────

        [Test]
        public void Card_GetHashCode_SameCard_ReturnsSameHash()
        {
            var a = new Card(CardColor.Fire, 5);
            var b = new Card(CardColor.Fire, 5);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        // ─── Card 番号範囲テスト ──────────────────────────────────────────

        [Test]
        public void Card_Create_EachColor_Numbers1To9()
        {
            var colors = (CardColor[])Enum.GetValues(typeof(CardColor));
            foreach (var color in colors)
            {
                for (int number = 1; number <= 9; number++)
                {
                    var card = new Card(color, number);
                    Assert.AreEqual(color, card.Color,
                        $"Color mismatch for {color}:{number}");
                    Assert.AreEqual(number, card.Number,
                        $"Number mismatch for {color}:{number}");
                }
            }
        }
    }
}
