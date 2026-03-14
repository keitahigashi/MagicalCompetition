namespace MagicalCompetition.Controllers
{
    /// <summary>
    /// シーン遷移とシーン間データ受け渡しを管理する。
    /// AI人数をstatic変数で保持し、シーン間で共有する。
    /// </summary>
    public class SceneController
    {
        /// <summary>AI対戦相手の人数（1〜4）。</summary>
        public static int AICount { get; private set; }

        /// <summary>
        /// AI人数を設定してGameSceneへの遷移準備を行う。
        /// 実際のSceneManager.LoadSceneはMonoBehaviourから呼び出す。
        /// </summary>
        public void LoadGameScene(int aiCount)
        {
            AICount = aiCount;
            // SceneManager.LoadScene("GameScene") は MonoBehaviour から呼び出す
        }

        /// <summary>
        /// TitleSceneへの遷移準備を行う。
        /// </summary>
        public void LoadTitleScene()
        {
            // SceneManager.LoadScene("TitleScene") は MonoBehaviour から呼び出す
        }
    }
}
