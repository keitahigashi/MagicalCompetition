using System;
using System.Collections.Generic;
using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Controllers;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class GameStateMachineTests
    {
        private GameStateMachine _sm;

        [SetUp]
        public void SetUp()
        {
            _sm = new GameStateMachine();
        }

        // ─── 1. 初期フェーズ: Setup ────────────────────────────────────────

        [Test]
        public void InitialPhase_IsSetup()
        {
            Assert.AreEqual(GamePhase.Setup, _sm.CurrentPhase);
        }

        // ─── 2. 正常遷移: 基本ゲームフロー ─────────────────────────────────

        [Test]
        public void NormalFlow_Setup_To_NextTurn()
        {
            _sm.TransitionTo(GamePhase.SelectFieldCard);
            Assert.AreEqual(GamePhase.SelectFieldCard, _sm.CurrentPhase);

            _sm.TransitionTo(GamePhase.Play);
            Assert.AreEqual(GamePhase.Play, _sm.CurrentPhase);

            _sm.TransitionTo(GamePhase.Draw);
            Assert.AreEqual(GamePhase.Draw, _sm.CurrentPhase);

            _sm.TransitionTo(GamePhase.CheckWin);
            Assert.AreEqual(GamePhase.CheckWin, _sm.CurrentPhase);

            _sm.TransitionTo(GamePhase.NextTurn);
            Assert.AreEqual(GamePhase.NextTurn, _sm.CurrentPhase);
        }

        // ─── 3. パス遷移: Play→NextTurn ────────────────────────────────────

        [Test]
        public void PassTransition_Play_To_NextTurn()
        {
            _sm.TransitionTo(GamePhase.SelectFieldCard);
            _sm.TransitionTo(GamePhase.Play);

            _sm.TransitionTo(GamePhase.NextTurn);
            Assert.AreEqual(GamePhase.NextTurn, _sm.CurrentPhase);
        }

        // ─── 4. 全員パス: Play→AllPassReset→SelectFieldCard ────────────────

        [Test]
        public void AllPass_Play_To_AllPassReset_To_SelectFieldCard()
        {
            _sm.TransitionTo(GamePhase.SelectFieldCard);
            _sm.TransitionTo(GamePhase.Play);

            _sm.TransitionTo(GamePhase.AllPassReset);
            Assert.AreEqual(GamePhase.AllPassReset, _sm.CurrentPhase);

            _sm.TransitionTo(GamePhase.SelectFieldCard);
            Assert.AreEqual(GamePhase.SelectFieldCard, _sm.CurrentPhase);
        }

        // ─── 5. 勝利: CheckWin→End ─────────────────────────────────────────

        [Test]
        public void Win_CheckWin_To_End()
        {
            _sm.TransitionTo(GamePhase.SelectFieldCard);
            _sm.TransitionTo(GamePhase.Play);
            _sm.TransitionTo(GamePhase.Draw);
            _sm.TransitionTo(GamePhase.CheckWin);

            _sm.TransitionTo(GamePhase.End);
            Assert.AreEqual(GamePhase.End, _sm.CurrentPhase);
        }

        // ─── 6. 不正遷移: Setup→Play → エラー ──────────────────────────────

        [Test]
        public void InvalidTransition_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _sm.TransitionTo(GamePhase.Play));
        }

        // ─── 7. OnPhaseEnter/Exit イベント ──────────────────────────────────

        [Test]
        public void Events_FireInCorrectOrder()
        {
            var log = new List<string>();

            _sm.OnPhaseExit += phase => log.Add($"Exit:{phase}");
            _sm.OnPhaseEnter += phase => log.Add($"Enter:{phase}");

            _sm.TransitionTo(GamePhase.SelectFieldCard);

            Assert.AreEqual(2, log.Count);
            Assert.AreEqual("Exit:Setup", log[0]);
            Assert.AreEqual("Enter:SelectFieldCard", log[1]);
        }
    }
}
