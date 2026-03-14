using System.Collections.Generic;
using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Controllers;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class GameControllerDrawTests
    {
        private GameController _controller;

        private void SetupToPlayPhase()
        {
            _controller = new GameController();
            var config = new GameConfig(1); // 2人
            _controller.ExecuteSetup(config);
            _controller.ExecuteSelectFieldCard(CardColor.Fire);
        }

        // ─── 1. Draw: 手札3枚まで補充 ──────────────────────────────────────

        [Test]
        public void ExecuteDraw_RefillsHandToThree()
        {
            SetupToPlayPhase();

            var player = _controller.State.CurrentPlayer;
            var card = player.Hand[0];
            _controller.ExecutePlayCards(
                PlayAction.CreatePlay(PlayType.SameNumber, new List<Card> { card }));

            // Play後はDrawフェーズ。手札2枚
            Assert.AreEqual(2, player.Hand.Count);

            _controller.ExecuteDraw();

            Assert.AreEqual(3, player.Hand.Count);
            Assert.AreEqual(GamePhase.CheckWin, _controller.StateMachine.CurrentPhase);
        }

        // ─── 2. Draw(山札空): IsReach=true ──────────────────────────────────

        [Test]
        public void ExecuteDraw_EmptyDeck_SetsIsReach()
        {
            SetupToPlayPhase();

            var player = _controller.State.CurrentPlayer;

            // 山札を空にする
            while (player.Deck.Count > 0)
                player.DrawFromDeck();

            // 手札を1枚出す
            var card = player.Hand[0];
            _controller.ExecutePlayCards(
                PlayAction.CreatePlay(PlayType.SameNumber, new List<Card> { card }));

            _controller.ExecuteDraw();

            Assert.IsTrue(player.IsReach);
        }

        // ─── 3. CheckWin(勝利): スコア計算・End遷移 ─────────────────────────

        [Test]
        public void ExecuteCheckWin_Winner_TransitionsToEnd()
        {
            SetupToPlayPhase();

            var player = _controller.State.CurrentPlayer;

            // 勝利状態を作る: 手札・山札を全て空にする
            player.Hand.Clear();
            player.Deck.Clear();

            // Drawフェーズにする（Playからカード出し）
            // 直接 CheckWin を呼ぶために状態を調整
            _controller.StateMachine.TransitionTo(GamePhase.Draw);
            _controller.StateMachine.TransitionTo(GamePhase.CheckWin);

            _controller.ExecuteCheckWin();

            Assert.AreEqual(GamePhase.End, _controller.StateMachine.CurrentPhase);
            Assert.IsNotNull(_controller.Result);
            Assert.AreEqual(player, _controller.Result.Winner);
        }

        // ─── 4. CheckWin(継続): NextTurn遷移 ───────────────────────────────

        [Test]
        public void ExecuteCheckWin_NoWinner_TransitionsToNextTurn()
        {
            SetupToPlayPhase();

            var player = _controller.State.CurrentPlayer;
            var card = player.Hand[0];
            _controller.ExecutePlayCards(
                PlayAction.CreatePlay(PlayType.SameNumber, new List<Card> { card }));

            _controller.ExecuteDraw();
            // CheckWinフェーズ

            _controller.ExecuteCheckWin();

            Assert.AreEqual(GamePhase.NextTurn, _controller.StateMachine.CurrentPhase);
        }

        // ─── 5. NextTurn: 次プレイヤー切替・Play遷移 ────────────────────────

        [Test]
        public void ExecuteNextTurn_AdvancesPlayer_TransitionsToPlay()
        {
            SetupToPlayPhase();

            Assert.AreEqual(0, _controller.State.CurrentPlayerIndex);

            // パス → NextTurn
            _controller.ExecutePass(PlayAction.CreatePass(new List<Card>()));
            // NextTurnフェーズ

            _controller.ExecuteNextTurn();

            Assert.AreEqual(1, _controller.State.CurrentPlayerIndex);
            Assert.AreEqual(GamePhase.Play, _controller.StateMachine.CurrentPhase);
        }

        // ─── 6. AllPassReset: リセット・SelectFieldCard遷移 ─────────────────

        [Test]
        public void ExecuteAllPassReset_ResetsAndTransitions()
        {
            SetupToPlayPhase();

            // 全員パスにする
            _controller.ExecutePass(PlayAction.CreatePass(new List<Card>()));
            _controller.ExecuteNextTurn();
            _controller.ExecutePass(PlayAction.CreatePass(new List<Card>()));
            // AllPassResetフェーズ

            _controller.ExecuteAllPassReset();

            Assert.AreEqual(0, _controller.State.ConsecutivePassCount);
            Assert.AreEqual(GamePhase.SelectFieldCard, _controller.StateMachine.CurrentPhase);
        }
    }
}
