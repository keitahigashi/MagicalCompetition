using System.Collections.Generic;
using NUnit.Framework;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class PlayResultTests
    {
        // ─── コンストラクタ ───────────────────────────────────────────────

        [Test]
        public void Constructor_SetsWinnerScoreAndAllPlayers()
        {
            var winner = new PlayerState(0, true);
            var player2 = new PlayerState(1, false);
            var allPlayers = new List<PlayerState> { winner, player2 };
            int score = 42;

            var result = new PlayResult(winner, score, allPlayers);

            Assert.AreSame(winner, result.Winner);
            Assert.AreEqual(42, result.Score);
            Assert.AreEqual(2, result.AllPlayers.Count);
            Assert.AreSame(winner, result.AllPlayers[0]);
            Assert.AreSame(player2, result.AllPlayers[1]);
        }
    }
}
