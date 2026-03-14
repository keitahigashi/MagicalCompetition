using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.Systems;
using MagicalCompetition.Controllers;

namespace MagicalCompetition.Tests.PlayMode.Integration
{
    /// <summary>
    /// ゲームフロー全体の統合テスト。
    /// GameControllerを使用してSetup→Play→Draw→CheckWin→NextTurnの一連フローを検証する。
    /// </summary>
    public class GameFlowIntegrationTests
    {
        private GameController _gameController;
        private GameConfig _config;

        [SetUp]
        public void SetUp()
        {
            _gameController = new GameController();
            _config = new GameConfig(aiCount: 1);
        }

        [UnityTest]
        public IEnumerator GameInit_AllPlayersHave3Cards_FieldSet_PhaseIsSelectFieldCard()
        {
            yield return null;

            _gameController.ExecuteSetup(_config);

            var state = _gameController.State;
            Assert.IsNotNull(state);
            Assert.AreEqual(2, state.Players.Count);

            foreach (var player in state.Players)
            {
                Assert.AreEqual(3, player.Hand.Count, $"Player {player.PlayerId} should have 3 cards");
                Assert.IsTrue(player.Deck.Count > 0, $"Player {player.PlayerId} should have deck cards");
            }

            Assert.AreEqual(GamePhase.SelectFieldCard, _gameController.StateMachine.CurrentPhase);
        }

        [UnityTest]
        public IEnumerator PlayCardFlow_HandDecreases_FieldUpdates_DrawRefills()
        {
            yield return null;

            _gameController.ExecuteSetup(_config);
            _gameController.ExecuteSelectFieldCard(CardColor.Fire);

            Assert.AreEqual(GamePhase.Play, _gameController.StateMachine.CurrentPhase);

            var currentPlayer = _gameController.State.CurrentPlayer;
            int handCountBefore = currentPlayer.Hand.Count;

            var validator = new PlayValidator();
            var validPlays = validator.GetAllValidPlays(currentPlayer.Hand, _gameController.State.Field);

            if (validPlays.Count > 0)
            {
                var action = validPlays[0];
                int cardsPlayed = action.Cards.Count;

                _gameController.ExecutePlayCards(action);

                Assert.AreEqual(handCountBefore - cardsPlayed, currentPlayer.Hand.Count);
                Assert.AreEqual(GamePhase.Draw, _gameController.StateMachine.CurrentPhase);

                _gameController.ExecuteDraw();
                Assert.LessOrEqual(currentPlayer.Hand.Count, 3);
                Assert.AreEqual(GamePhase.CheckWin, _gameController.StateMachine.CurrentPhase);
            }
            else
            {
                Assert.Pass("No valid plays available, skipping play test");
            }
        }

        [UnityTest]
        public IEnumerator PassFlow_NextPlayerTurn()
        {
            yield return null;

            _gameController.ExecuteSetup(_config);
            _gameController.ExecuteSelectFieldCard(CardColor.Fire);

            Assert.AreEqual(GamePhase.Play, _gameController.StateMachine.CurrentPhase);

            var currentPlayer = _gameController.State.CurrentPlayer;
            int currentPlayerIndex = _gameController.State.CurrentPlayerIndex;

            // パスアクション（手札1枚を山札に戻す）
            var cardToReturn = currentPlayer.Hand[0];
            var passAction = PlayAction.CreatePass(new List<Card> { cardToReturn });

            _gameController.ExecutePass(passAction);

            // パス時はDraw/CheckWinをスキップし、直接NextTurnへ
            Assert.AreEqual(GamePhase.NextTurn, _gameController.StateMachine.CurrentPhase);

            _gameController.ExecuteNextTurn();

            // 次のプレイヤーに切り替わる
            Assert.AreNotEqual(currentPlayerIndex, _gameController.State.CurrentPlayerIndex);
            Assert.AreEqual(GamePhase.Play, _gameController.StateMachine.CurrentPhase);
        }

        [UnityTest]
        public IEnumerator AllPassReset_FieldResets_SelectFieldCardRestart()
        {
            yield return null;

            _gameController.ExecuteSetup(_config);
            _gameController.ExecuteSelectFieldCard(CardColor.Water);

            // 全プレイヤーをパスさせる（2プレイヤー）
            for (int i = 0; i < _gameController.State.Players.Count; i++)
            {
                Assert.AreEqual(GamePhase.Play, _gameController.StateMachine.CurrentPhase);

                var player = _gameController.State.CurrentPlayer;
                var cardToReturn = player.Hand[0];
                var passAction = PlayAction.CreatePass(new List<Card> { cardToReturn });

                _gameController.ExecutePass(passAction);

                if (_gameController.StateMachine.CurrentPhase == GamePhase.AllPassReset)
                {
                    // 全員パス → リセット
                    _gameController.ExecuteAllPassReset();
                    Assert.AreEqual(GamePhase.SelectFieldCard, _gameController.StateMachine.CurrentPhase);
                    yield break;
                }

                // まだ全員パスでなければNextTurn
                Assert.AreEqual(GamePhase.NextTurn, _gameController.StateMachine.CurrentPhase);
                _gameController.ExecuteNextTurn();
            }

            Assert.Fail("Expected AllPassReset but did not reach it");
        }

        [UnityTest]
        public IEnumerator WinFlow_AllCardsUsed_EndPhase_ResultAvailable()
        {
            yield return null;

            _gameController.ExecuteSetup(_config);
            _gameController.ExecuteSelectFieldCard(CardColor.Fire);

            // プレイヤーの手札と山札を手動で空にして勝利状態をシミュレート
            var player = _gameController.State.Players[0];
            player.Hand.Clear();
            player.Deck.Clear();

            // Play→Draw→CheckWinへ手動遷移
            _gameController.StateMachine.TransitionTo(GamePhase.Draw);
            _gameController.StateMachine.TransitionTo(GamePhase.CheckWin);

            _gameController.ExecuteCheckWin();

            Assert.AreEqual(GamePhase.End, _gameController.StateMachine.CurrentPhase);

            var result = _gameController.Result;
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Winner);
            Assert.AreEqual(player, result.Winner);
            Assert.IsTrue(result.Score >= 0);
        }
    }
}
