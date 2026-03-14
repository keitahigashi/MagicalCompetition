using NUnit.Framework;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class FieldStateTests
    {
        // ─── 1. Update(Card): 場札の色・番号がカードのものに更新される ─────────

        [Test]
        public void Update_SetsColorAndNumberFromCard()
        {
            var fieldState = new FieldState();
            var card = new Card(CardColor.Water, 7);

            fieldState.Update(card);

            Assert.AreEqual(CardColor.Water, fieldState.Color);
            Assert.AreEqual(7, fieldState.Number);
        }

        // ─── 2. Update後のIsVirtual: falseになる ────────────────────────────

        [Test]
        public void Update_AfterSetVirtual_IsVirtualReturnsFalse()
        {
            var fieldState = new FieldState();
            fieldState.SetVirtual(CardColor.Earth);

            var card = new Card(CardColor.Fire, 3);
            fieldState.Update(card);

            Assert.IsFalse(fieldState.IsVirtual);
        }

        // ─── 3. SetVirtual(CardColor): 指定色・番号5で場札が設定される ────────

        [Test]
        public void SetVirtual_SetsColorAndNumber5()
        {
            var fieldState = new FieldState();

            fieldState.SetVirtual(CardColor.Wind);

            Assert.AreEqual(CardColor.Wind, fieldState.Color);
            Assert.AreEqual(5, fieldState.Number);
        }

        // ─── 4. SetVirtual後のIsVirtual: trueになる ─────────────────────────

        [Test]
        public void SetVirtual_IsVirtualReturnsTrue()
        {
            var fieldState = new FieldState();

            fieldState.SetVirtual(CardColor.Light);

            Assert.IsTrue(fieldState.IsVirtual);
        }

        // ─── 5. Clear: Color=Fire, Number=0, IsVirtual=false にリセット ──────

        [Test]
        public void Clear_ResetsToDefaultValues()
        {
            var fieldState = new FieldState();
            fieldState.SetVirtual(CardColor.Wind);

            fieldState.Clear();

            Assert.AreEqual(default(CardColor), fieldState.Color);
            Assert.AreEqual(0, fieldState.Number);
            Assert.IsFalse(fieldState.IsVirtual);
        }
    }
}
