using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Views;

namespace MagicalCompetition.Tests.PlayMode.Views
{
    public class ResultDialogViewTests
    {
        private GameObject _canvasGo;
        private ResultDialogView _resultView;
        private Text _winnerText;
        private Text _scoreText;
        private GameObject _dialogPanel;
        private Button _titleButton;

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("Canvas");
            _canvasGo.AddComponent<Canvas>();

            var go = new GameObject("ResultDialog");
            go.transform.SetParent(_canvasGo.transform);

            _dialogPanel = new GameObject("DialogPanel");
            _dialogPanel.transform.SetParent(go.transform);

            var winnerGo = new GameObject("WinnerText");
            winnerGo.transform.SetParent(go.transform);
            winnerGo.AddComponent<RectTransform>();
            _winnerText = winnerGo.AddComponent<Text>();

            var scoreGo = new GameObject("ScoreText");
            scoreGo.transform.SetParent(go.transform);
            scoreGo.AddComponent<RectTransform>();
            _scoreText = scoreGo.AddComponent<Text>();

            var btnGo = new GameObject("TitleButton");
            btnGo.transform.SetParent(go.transform);
            btnGo.AddComponent<RectTransform>();
            btnGo.AddComponent<Image>();
            _titleButton = btnGo.AddComponent<Button>();

            _resultView = go.AddComponent<ResultDialogView>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_canvasGo);
        }

        [UnityTest]
        public IEnumerator Show_ActivatesDialogPanel()
        {
            yield return null;

            // Awake後にSerializeFieldをセット
            SetFields();

            var winner = new PlayerState(0, true);
            var result = new PlayResult(winner, 10, new List<PlayerState> { winner });

            _resultView.Show(result);

            Assert.IsTrue(_dialogPanel.activeSelf);
        }

        [UnityTest]
        public IEnumerator Show_DisplaysWinnerName()
        {
            yield return null;

            SetFields();

            var winner = new PlayerState(0, true);
            var result = new PlayResult(winner, 10, new List<PlayerState> { winner });

            _resultView.Show(result);

            Assert.IsTrue(_winnerText.text.Contains("あなた"));
        }

        [UnityTest]
        public IEnumerator Show_DisplaysScore()
        {
            yield return null;

            SetFields();

            var winner = new PlayerState(0, true);
            var result = new PlayResult(winner, 15, new List<PlayerState> { winner });

            _resultView.Show(result);

            Assert.IsTrue(_scoreText.text.Contains("15"));
        }

        [UnityTest]
        public IEnumerator TitleButton_Click_FiresEvent()
        {
            yield return null;

            SetFields();

            // Awake時にフィールドがnullだったため、手動でリスナーを登録
            var method = typeof(ResultDialogView).GetMethod("HandleTitleButtonClick",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            _titleButton.onClick.AddListener(() => method.Invoke(_resultView, null));

            bool fired = false;
            _resultView.OnTitleButtonClicked += () => fired = true;

            _titleButton.onClick.Invoke();

            Assert.IsTrue(fired);
        }

        [UnityTest]
        public IEnumerator Hide_DeactivatesDialogPanel()
        {
            yield return null;

            SetFields();

            var winner = new PlayerState(0, true);
            var result = new PlayResult(winner, 10, new List<PlayerState> { winner });

            _resultView.Show(result);
            _resultView.Hide();

            Assert.IsFalse(_dialogPanel.activeSelf);
        }

        private void SetFields()
        {
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            typeof(ResultDialogView).GetField("_winnerText", flags).SetValue(_resultView, _winnerText);
            typeof(ResultDialogView).GetField("_scoreText", flags).SetValue(_resultView, _scoreText);
            typeof(ResultDialogView).GetField("_dialogPanel", flags).SetValue(_resultView, _dialogPanel);
            typeof(ResultDialogView).GetField("_titleButton", flags).SetValue(_resultView, _titleButton);
        }
    }
}
