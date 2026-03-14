using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using MagicalCompetition.UI;

namespace MagicalCompetition.Tests.PlayMode.UI
{
    public class TitleUITests
    {
        private GameObject _canvasGo;
        private TitleUI _titleUI;
        private Button _startButton;
        private Button[] _aiCountButtons;
        private Text _selectedCountText;

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("Canvas");
            _canvasGo.AddComponent<Canvas>();

            var go = new GameObject("TitleUI");
            go.transform.SetParent(_canvasGo.transform);

            // AI人数ボタン（4つ）
            _aiCountButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                var btnGo = new GameObject($"AICountBtn{i + 1}");
                btnGo.transform.SetParent(go.transform);
                btnGo.AddComponent<RectTransform>();
                btnGo.AddComponent<Image>();
                _aiCountButtons[i] = btnGo.AddComponent<Button>();
            }

            // 開始ボタン
            var startGo = new GameObject("StartButton");
            startGo.transform.SetParent(go.transform);
            startGo.AddComponent<RectTransform>();
            startGo.AddComponent<Image>();
            _startButton = startGo.AddComponent<Button>();

            // テキスト
            var textGo = new GameObject("SelectedCountText");
            textGo.transform.SetParent(go.transform);
            textGo.AddComponent<RectTransform>();
            _selectedCountText = textGo.AddComponent<Text>();

            // SerializeFieldセット（Awake前）
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            // AddComponentの前にフィールドセット不可なので、Awake後にセット
            _titleUI = go.AddComponent<TitleUI>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_canvasGo);
        }

        [UnityTest]
        public IEnumerator DefaultAICount_IsOne()
        {
            yield return null;

            Assert.AreEqual(1, _titleUI.SelectedAICount);
        }

        [UnityTest]
        public IEnumerator SelectAICount_UpdatesSelectedCount()
        {
            yield return null;

            _titleUI.SelectAICount(3);

            Assert.AreEqual(3, _titleUI.SelectedAICount);
        }

        [UnityTest]
        public IEnumerator SelectAICount_ClampedToValidRange()
        {
            yield return null;

            _titleUI.SelectAICount(0);
            Assert.AreEqual(1, _titleUI.SelectedAICount);

            _titleUI.SelectAICount(5);
            Assert.AreEqual(4, _titleUI.SelectedAICount);
        }

        [UnityTest]
        public IEnumerator OnGameStart_FiresWithCorrectCount()
        {
            yield return null;

            // SerializeFieldをリフレクションでセット
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            typeof(TitleUI).GetField("_startButton", flags).SetValue(_titleUI, _startButton);

            // Awake後なのでリスナー手動追加
            _startButton.onClick.AddListener(() =>
            {
                // TitleUIのHandleStartButtonを直接呼べないので、OnGameStartを直接テスト
            });

            int receivedCount = -1;
            _titleUI.OnGameStart += (count) => receivedCount = count;
            _titleUI.SelectAICount(2);

            // HandleStartButtonはprivateなので直接テスト
            // StartButtonのonClickは Awake時にバインドされるが、
            // SerializeFieldがnullだったためバインドされていない
            // 代わりに直接メソッドをテスト
            var method = typeof(TitleUI).GetMethod("HandleStartButton",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(_titleUI, null);

            Assert.AreEqual(2, receivedCount);
        }
    }
}
