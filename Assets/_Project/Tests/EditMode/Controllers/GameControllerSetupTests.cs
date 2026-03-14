using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Controllers;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class GameControllerSetupTests
    {
        private GameController _controller;

        [SetUp]
        public void SetUp()
        {
            _controller = new GameController();
        }

        // ─── 1. Setup: デッキ生成・配布・初期ドロー・場札設定 ────────────────

        [Test]
        public void ExecuteSetup_PlayersHaveHandsAndDecks()
        {
            var config = new GameConfig(1); // 人間1 + AI1 = 2人
            _controller.ExecuteSetup(config);

            var state = _controller.State;
            Assert.AreEqual(2, state.Players.Count);

            // 各プレイヤーが手札3枚を持っている
            foreach (var player in state.Players)
                Assert.AreEqual(3, player.Hand.Count, $"Player {player.PlayerId} should have 3 cards in hand");

            // 場札が設定されている（疑似場札 Any:5）
            Assert.IsTrue(state.Field.IsVirtual);
            Assert.AreEqual(5, state.Field.Number);

            // SelectFieldCardフェーズに遷移
            Assert.AreEqual(GamePhase.SelectFieldCard, _controller.StateMachine.CurrentPhase);
        }

        // ─── 2. Setup: 余りカードがExcludedCardsに入る ──────────────────────

        [Test]
        public void ExecuteSetup_ExcludedCardsStored()
        {
            var config = new GameConfig(1); // 2人: 45÷2=22余り1
            _controller.ExecuteSetup(config);

            Assert.AreEqual(1, _controller.State.ExcludedCards.Count);
        }

        // ─── 3. SelectFieldCard: 色選択後にPlayフェーズ遷移 ──────────────────

        [Test]
        public void ExecuteSelectFieldCard_TransitionsToPlay()
        {
            var config = new GameConfig(1);
            _controller.ExecuteSetup(config);

            _controller.ExecuteSelectFieldCard(CardColor.Fire);

            Assert.AreEqual(GamePhase.Play, _controller.StateMachine.CurrentPhase);
            Assert.AreEqual(CardColor.Fire, _controller.State.Field.Color);
            Assert.AreEqual(5, _controller.State.Field.Number);
        }

        // ─── 4. Play(カード出し): 手札削除・場札更新・Draw遷移 ──────────────

        [Test]
        public void ExecutePlayCards_RemovesFromHand_UpdatesField_TransitionsToDraw()
        {
            var config = new GameConfig(1);
            _controller.ExecuteSetup(config);
            _controller.ExecuteSelectFieldCard(CardColor.Fire);

            var player = _controller.State.CurrentPlayer;
            var cardToPlay = player.Hand[0];
            var action = PlayAction.CreatePlay(PlayType.SameNumber,
                new List<Card> { cardToPlay });

            int handBefore = player.Hand.Count;
            _controller.ExecutePlayCards(action);

            Assert.AreEqual(handBefore - 1, player.Hand.Count);
            Assert.AreEqual(cardToPlay.Color, _controller.State.Field.Color);
            Assert.AreEqual(cardToPlay.Number, _controller.State.Field.Number);
            Assert.AreEqual(GamePhase.Draw, _controller.StateMachine.CurrentPhase);
        }

        // ─── 5. Play(パス): パスカウント増加・NextTurn遷移 ──────────────────

        [Test]
        public void ExecutePass_IncrementsPassCount_TransitionsToNextTurn()
        {
            var config = new GameConfig(1); // 2人
            _controller.ExecuteSetup(config);
            _controller.ExecuteSelectFieldCard(CardColor.Fire);

            Assert.AreEqual(0, _controller.State.ConsecutivePassCount);

            var action = PlayAction.CreatePass(new List<Card>());
            _controller.ExecutePass(action);

            Assert.AreEqual(1, _controller.State.ConsecutivePassCount);
            Assert.AreEqual(GamePhase.NextTurn, _controller.StateMachine.CurrentPhase);
        }

        // ─── 6. Play(全員パス): AllPassReset遷移 ────────────────────────────

        [Test]
        public void ExecutePass_AllPassed_TransitionsToAllPassReset()
        {
            var config = new GameConfig(1); // 2人
            _controller.ExecuteSetup(config);
            _controller.ExecuteSelectFieldCard(CardColor.Fire);

            // プレイヤー0がパス
            _controller.ExecutePass(PlayAction.CreatePass(new List<Card>()));
            // → NextTurnへ遷移

            _controller.ExecuteNextTurn(); // プレイヤー1へ
            // → Playフェーズ

            // プレイヤー1がパス（全員パス）
            _controller.ExecutePass(PlayAction.CreatePass(new List<Card>()));

            Assert.AreEqual(GamePhase.AllPassReset, _controller.StateMachine.CurrentPhase);
            Assert.AreEqual(2, _controller.State.ConsecutivePassCount);
        }
    }
}
