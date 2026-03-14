using System.Collections.Generic;
using NUnit.Framework;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class GameStateTests
    {
        // ─── ヘルパー ────────────────────────────────────────────────────────

        /// <summary>指定人数分の PlayerState リストを生成する。</summary>
        private static List<PlayerState> MakePlayers(int count)
        {
            var players = new List<PlayerState>();
            for (int i = 0; i < count; i++)
                players.Add(new PlayerState(i, isHuman: i == 0));
            return players;
        }

        // ─── 3. AdvanceTurn: index 0→1 ───────────────────────────────────────

        [Test]
        public void AdvanceTurn_FromFirst_MovesToSecondPlayer()
        {
            var state = new GameState(MakePlayers(3));
            state.CurrentPlayerIndex = 0;

            state.AdvanceTurn();

            Assert.AreEqual(1, state.CurrentPlayerIndex);
        }

        // ─── 4. AdvanceTurn: 最後→先頭に循環（index 2→0 for 3 players） ─────

        [Test]
        public void AdvanceTurn_FromLast_WrapsAroundToFirst()
        {
            var state = new GameState(MakePlayers(3));
            state.CurrentPlayerIndex = 2;

            state.AdvanceTurn();

            Assert.AreEqual(0, state.CurrentPlayerIndex);
        }

        // ─── 5. AllPlayersPassed: ConsecutivePassCount >= PlayerCount で true ─

        [Test]
        public void AllPlayersPassed_WhenPassCountReachesPlayerCount_ReturnsTrue()
        {
            var state = new GameState(MakePlayers(3));
            state.ConsecutivePassCount = 3;

            Assert.IsTrue(state.AllPlayersPassed);
        }

        // ─── 6. ResetPassCount: カウントが0にリセット ─────────────────────────

        [Test]
        public void ResetPassCount_SetsConsecutivePassCountToZero()
        {
            var state = new GameState(MakePlayers(2));
            state.ConsecutivePassCount = 5;

            state.ResetPassCount();

            Assert.AreEqual(0, state.ConsecutivePassCount);
        }
    }
}
