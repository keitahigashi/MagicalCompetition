using NUnit.Framework;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class GameConfigTests
    {
        // ─── 1. AICount=1 → TotalPlayerCount=2 ───────────────────────────────

        [Test]
        public void TotalPlayerCount_WithOneAI_ReturnsTwo()
        {
            var config = new GameConfig(aiCount: 1);

            Assert.AreEqual(2, config.TotalPlayerCount);
        }

        // ─── 2. AICount=4 → TotalPlayerCount=5 ───────────────────────────────

        [Test]
        public void TotalPlayerCount_WithFourAIs_ReturnsFive()
        {
            var config = new GameConfig(aiCount: 4);

            Assert.AreEqual(5, config.TotalPlayerCount);
        }
    }
}
