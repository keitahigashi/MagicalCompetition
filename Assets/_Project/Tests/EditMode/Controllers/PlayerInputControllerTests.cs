using System.Collections.Generic;
using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Controllers;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class PlayerInputControllerTests
    {
        private PlayerInputController _input;
        private FieldState _field;

        [SetUp]
        public void SetUp()
        {
            _input = new PlayerInputController();
            _field = new FieldState();
            _field.Update(new Card(CardColor.Fire, 5));
            _input.WaitForInput(_field);
        }

        // ─── 1. カード選択・解除 ────────────────────────────────────────────

        [Test]
        public void SelectCard_And_DeselectCard_TogglesState()
        {
            var card = new Card(CardColor.Fire, 5);
            _input.SelectCard(card);

            Assert.AreEqual(1, _input.SelectedCards.Count);

            _input.DeselectCard(card);
            Assert.AreEqual(0, _input.SelectedCards.Count);
        }

        // ─── 2. 複数選択 ────────────────────────────────────────────────────

        [Test]
        public void SelectCard_MultipleCards()
        {
            _input.SelectCard(new Card(CardColor.Fire, 5));
            _input.SelectCard(new Card(CardColor.Water, 5));

            Assert.AreEqual(2, _input.SelectedCards.Count);
        }

        // ─── 3. 有効な組み合わせで CanConfirmPlay = true ─────────────────────

        [Test]
        public void CanConfirmPlay_ValidCombination_ReturnsTrue()
        {
            // 場札 Fire:5 に対して同数字 Water:5 を選択
            _input.SelectCard(new Card(CardColor.Water, 5));

            Assert.IsTrue(_input.CanConfirmPlay());
        }

        // ─── 4. 無効な組み合わせで CanConfirmPlay = false ────────────────────

        [Test]
        public void CanConfirmPlay_InvalidCombination_ReturnsFalse()
        {
            // 場札 Fire:5 に対して Water:3 は無効
            _input.SelectCard(new Card(CardColor.Water, 3));

            Assert.IsFalse(_input.CanConfirmPlay());
        }

        // ─── 5. パス: 戻すカード0枚でOK ─────────────────────────────────────

        [Test]
        public void ConfirmPass_ZeroCardsToReturn_ReturnsValidPassAction()
        {
            _input.StartPassSelection();
            var action = _input.ConfirmPass();

            Assert.AreEqual(PlayType.Pass, action.Type);
            Assert.AreEqual(0, action.Cards.Count);
        }

        // ─── 6. パス: 全手札を戻すことも可能 ────────────────────────────────

        [Test]
        public void ConfirmPass_AllCardsToReturn_ReturnsValidPassAction()
        {
            _input.StartPassSelection();
            _input.SelectCardToReturn(new Card(CardColor.Fire, 1));
            _input.SelectCardToReturn(new Card(CardColor.Fire, 2));
            _input.SelectCardToReturn(new Card(CardColor.Fire, 3));

            var action = _input.ConfirmPass();

            Assert.AreEqual(PlayType.Pass, action.Type);
            Assert.AreEqual(3, action.Cards.Count);
        }
    }
}
