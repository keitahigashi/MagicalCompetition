using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.Systems;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class FieldUpdaterTests
    {
        private FieldUpdater _updater;
        private FieldState _field;

        [SetUp]
        public void SetUp()
        {
            _updater = new FieldUpdater();
            _field = new FieldState();
        }

        // ─── 1. Update: 通常更新 ───────────────────────────────────────────

        [Test]
        public void Update_SetsFieldToLastPlayedCard()
        {
            var card = new Card(CardColor.Water, 7);
            _updater.Update(_field, card);

            Assert.AreEqual(CardColor.Water, _field.Color);
            Assert.AreEqual(7, _field.Number);
            Assert.IsFalse(_field.IsVirtual);
        }

        // ─── 2. Update(同数字複数枚後): 最後のカードの色 ────────────────────

        [Test]
        public void Update_SameNumberMultiple_LastCardColorUsed()
        {
            // 同数字で2枚出した場合、最後のカードの色で更新
            var lastCard = new Card(CardColor.Earth, 5);
            _updater.Update(_field, lastCard);

            Assert.AreEqual(CardColor.Earth, _field.Color);
            Assert.AreEqual(5, _field.Number);
        }

        // ─── 3. Update(同色複数枚後): 最後のカードの番号 ────────────────────

        [Test]
        public void Update_SameColorMultiple_LastCardNumberUsed()
        {
            // 同色で2枚出した場合、最後のカードの番号で更新
            var lastCard = new Card(CardColor.Fire, 9);
            _updater.Update(_field, lastCard);

            Assert.AreEqual(CardColor.Fire, _field.Color);
            Assert.AreEqual(9, _field.Number);
        }

        // ─── 4. Update(計算出し後): 計算結果ではなくカード情報 ───────────────

        [Test]
        public void Update_Arithmetic_UsesLastCardNotCalcResult()
        {
            // 計算出し 3+4=7 → 最後に出したカード(Water:4)で更新（計算結果7ではない）
            var lastCard = new Card(CardColor.Water, 4);
            _updater.Update(_field, lastCard);

            Assert.AreEqual(CardColor.Water, _field.Color);
            Assert.AreEqual(4, _field.Number);
            Assert.IsFalse(_field.IsVirtual);
        }

        // ─── 5. SetVirtualField ─────────────────────────────────────────────

        [Test]
        public void SetVirtualField_SetsColorNumber5AndIsVirtual()
        {
            _updater.SetVirtualField(_field, CardColor.Light);

            Assert.AreEqual(CardColor.Light, _field.Color);
            Assert.AreEqual(5, _field.Number);
            Assert.IsTrue(_field.IsVirtual);
        }

        // ─── 6. ClearField ─────────────────────────────────────────────────

        [Test]
        public void ClearField_ResetsFieldToDefault()
        {
            // まず場札を設定
            _updater.Update(_field, new Card(CardColor.Wind, 8));

            // クリア
            _updater.ClearField(_field);

            Assert.AreEqual(default(CardColor), _field.Color);
            Assert.AreEqual(0, _field.Number);
            Assert.IsFalse(_field.IsVirtual);
        }
    }
}
