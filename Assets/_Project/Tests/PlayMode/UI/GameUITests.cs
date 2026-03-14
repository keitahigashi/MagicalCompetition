using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using MagicalCompetition.UI;

namespace MagicalCompetition.Tests.PlayMode.UI
{
    public class GameUITests
    {
        private GameObject _canvasGo;
        private GameUI _gameUI;
        private Button _playButton;
        private Button _passButton;
        private Button _confirmButton;
        private GameObject _returnCardPanel;

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("Canvas");
            _canvasGo.AddComponent<Canvas>();

            var go = new GameObject("GameUI");
            go.transform.SetParent(_canvasGo.transform);

            // ボタン生成
            _playButton = CreateButton("PlayButton", go.transform);
            _passButton = CreateButton("PassButton", go.transform);
            _confirmButton = CreateButton("ConfirmButton", go.transform);

            _returnCardPanel = new GameObject("ReturnCardPanel");
            _returnCardPanel.transform.SetParent(go.transform);

            // SerializeFieldをセット（AddComponent前にGameObjectに仕込めないためリフレクション）
            _gameUI = go.AddComponent<GameUI>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_canvasGo);
        }

        [UnityTest]
        public IEnumerator SetPlayButtonEnabled_True_EnablesButton()
        {
            yield return null;

            SetFields();
            _gameUI.SetPlayButtonEnabled(true);

            Assert.IsTrue(_playButton.interactable);
        }

        [UnityTest]
        public IEnumerator SetPlayButtonEnabled_False_DisablesButton()
        {
            yield return null;

            SetFields();
            _gameUI.SetPlayButtonEnabled(false);

            Assert.IsFalse(_playButton.interactable);
        }

        [UnityTest]
        public IEnumerator SetPassButtonEnabled_True_EnablesButton()
        {
            yield return null;

            SetFields();
            _gameUI.SetPassButtonEnabled(true);

            Assert.IsTrue(_passButton.interactable);
        }

        [UnityTest]
        public IEnumerator SetPassButtonEnabled_False_DisablesButton()
        {
            yield return null;

            SetFields();
            _gameUI.SetPassButtonEnabled(false);

            Assert.IsFalse(_passButton.interactable);
        }

        [UnityTest]
        public IEnumerator ShowReturnCardSelection_ShowsPanel()
        {
            yield return null;

            SetFields();
            _gameUI.ShowReturnCardSelection();

            Assert.IsTrue(_returnCardPanel.activeSelf);
        }

        [UnityTest]
        public IEnumerator HideReturnCardSelection_HidesPanel()
        {
            yield return null;

            SetFields();
            _gameUI.ShowReturnCardSelection();
            _gameUI.HideReturnCardSelection();

            Assert.IsFalse(_returnCardPanel.activeSelf);
        }

        [UnityTest]
        public IEnumerator SetAllButtonsDisabled_DisablesAll()
        {
            yield return null;

            SetFields();
            _gameUI.SetPlayButtonEnabled(false);
            _gameUI.SetPassButtonEnabled(false);

            Assert.IsFalse(_playButton.interactable);
            Assert.IsFalse(_passButton.interactable);
        }

        private Button CreateButton(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.AddComponent<RectTransform>();
            go.AddComponent<Image>();
            return go.AddComponent<Button>();
        }

        private void SetFields()
        {
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            typeof(GameUI).GetField("_playButton", flags).SetValue(_gameUI, _playButton);
            typeof(GameUI).GetField("_passButton", flags).SetValue(_gameUI, _passButton);
            typeof(GameUI).GetField("_confirmButton", flags).SetValue(_gameUI, _confirmButton);
            typeof(GameUI).GetField("_returnCardPanel", flags).SetValue(_gameUI, _returnCardPanel);
        }
    }
}
