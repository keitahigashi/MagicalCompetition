using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using MagicalCompetition.Views;

namespace MagicalCompetition.Tests.PlayMode.Views
{
    public class DeckViewTests
    {
        private GameObject _canvasGo;
        private DeckView _deckView;
        private Text _countText;
        private GameObject _reachIndicator;

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("Canvas");
            _canvasGo.AddComponent<Canvas>();

            var go = new GameObject("DeckView");
            go.transform.SetParent(_canvasGo.transform);
            go.AddComponent<RectTransform>();
            _deckView = go.AddComponent<DeckView>();

            var textGo = new GameObject("CountText");
            textGo.transform.SetParent(go.transform);
            textGo.AddComponent<RectTransform>();
            _countText = textGo.AddComponent<Text>();

            _reachIndicator = new GameObject("ReachIndicator");
            _reachIndicator.transform.SetParent(go.transform);

            // SerializeFieldセット
            var countField = typeof(DeckView).GetField("_countText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            countField.SetValue(_deckView, _countText);

            var reachField = typeof(DeckView).GetField("_reachIndicator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            reachField.SetValue(_deckView, _reachIndicator);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_canvasGo);
        }

        [UnityTest]
        public IEnumerator UpdateCount_ShowsCorrectNumber()
        {
            yield return null;

            _deckView.UpdateCount(10);

            Assert.AreEqual("10", _countText.text);
        }

        [UnityTest]
        public IEnumerator UpdateCount_Zero_ShowsReachIndicator()
        {
            yield return null;

            _deckView.UpdateCount(0);

            Assert.IsTrue(_reachIndicator.activeSelf);
        }

        [UnityTest]
        public IEnumerator UpdateCount_NonZero_HidesReachIndicator()
        {
            yield return null;

            _deckView.UpdateCount(5);

            Assert.IsFalse(_reachIndicator.activeSelf);
        }
    }
}
