using NUnit.Framework;
using MagicalCompetition.Controllers;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class SceneControllerTests
    {
        private SceneController _sceneController;

        [SetUp]
        public void SetUp()
        {
            _sceneController = new SceneController();
        }

        // ─── 1. LoadGameScene: AI人数が正しく保持される ──────────────────────

        [Test]
        public void LoadGameScene_StoresAICount()
        {
            _sceneController.LoadGameScene(3);
            Assert.AreEqual(3, SceneController.AICount);
        }

        // ─── 2. LoadTitleScene: エラーが発生しない ───────────────────────────

        [Test]
        public void LoadTitleScene_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _sceneController.LoadTitleScene());
        }

        // ─── 3. データ受け渡し: AI人数が正しく伝達される ────────────────────

        [Test]
        public void DataPassing_AICountCorrectlyTransferred()
        {
            _sceneController.LoadGameScene(2);
            Assert.AreEqual(2, SceneController.AICount);

            _sceneController.LoadGameScene(4);
            Assert.AreEqual(4, SceneController.AICount);
        }
    }
}
