using System.Collections.Generic;
using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.Systems;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class TurnManagerTests
    {
        private TurnManager _turnManager;

        [SetUp]
        public void SetUp()
        {
            _turnManager = new TurnManager();
        }

        private GameState CreateGameState(int playerCount)
        {
            var players = new List<PlayerState>();
            for (int i = 0; i < playerCount; i++)
                players.Add(new PlayerState(i, i == 0));
            return new GameState(players);
        }

        // ─── 1. AdvanceTurn: 次のプレイヤーへ ──────────────────────────────

        [Test]
        public void AdvanceTurn_MovesToNextPlayer()
        {
            var state = CreateGameState(3);
            Assert.AreEqual(0, state.CurrentPlayerIndex);

            _turnManager.AdvanceTurn(state);

            Assert.AreEqual(1, state.CurrentPlayerIndex);
        }

        // ─── 2. AdvanceTurn: 末尾→先頭 ─────────────────────────────────────

        [Test]
        public void AdvanceTurn_WrapsAroundToFirst()
        {
            var state = CreateGameState(2);
            state.CurrentPlayerIndex = 1;

            _turnManager.AdvanceTurn(state);

            Assert.AreEqual(0, state.CurrentPlayerIndex);
        }

        // ─── 3. RecordPass: カウント増加 ────────────────────────────────────

        [Test]
        public void RecordPass_IncrementsPassCount()
        {
            var state = CreateGameState(2);
            Assert.AreEqual(0, state.ConsecutivePassCount);

            _turnManager.RecordPass(state);

            Assert.AreEqual(1, state.ConsecutivePassCount);
        }

        // ─── 4. RecordPlay: カウントリセット ────────────────────────────────

        [Test]
        public void RecordPlay_ResetsPassCountToZero()
        {
            var state = CreateGameState(2);
            state.ConsecutivePassCount = 2;

            _turnManager.RecordPlay(state);

            Assert.AreEqual(0, state.ConsecutivePassCount);
        }

        // ─── 5. IsAllPlayersPassed(2人,2パス): true ─────────────────────────

        [Test]
        public void IsAllPlayersPassed_2Players2Passes_ReturnsTrue()
        {
            var state = CreateGameState(2);
            state.ConsecutivePassCount = 2;

            Assert.IsTrue(_turnManager.IsAllPlayersPassed(state));
        }

        // ─── 6. IsAllPlayersPassed(3人,2パス): false ────────────────────────

        [Test]
        public void IsAllPlayersPassed_3Players2Passes_ReturnsFalse()
        {
            var state = CreateGameState(3);
            state.ConsecutivePassCount = 2;

            Assert.IsFalse(_turnManager.IsAllPlayersPassed(state));
        }

        // ─── 7. IsAllPlayersPassed(5人,5パス): true ─────────────────────────

        [Test]
        public void IsAllPlayersPassed_5Players5Passes_ReturnsTrue()
        {
            var state = CreateGameState(5);
            state.ConsecutivePassCount = 5;

            Assert.IsTrue(_turnManager.IsAllPlayersPassed(state));
        }
    }
}
