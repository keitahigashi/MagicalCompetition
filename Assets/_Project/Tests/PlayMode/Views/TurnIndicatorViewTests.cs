using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Views;

namespace MagicalCompetition.Tests.PlayMode.Views
{
    public class TurnIndicatorViewTests
    {
        private GameObject _canvasGo;
        private TurnIndicatorView _turnView;
        private Text _turnText;

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("Canvas");
            _canvasGo.AddComponent<Canvas>();

            var go = new GameObject("TurnIndicator");
            go.transform.SetParent(_canvasGo.transform);
            _turnView = go.AddComponent<TurnIndicatorView>();

            var textGo = new GameObject("TurnText");
            textGo.transform.SetParent(go.transform);
            textGo.AddComponent<RectTransform>();
            _turnText = textGo.AddComponent<Text>();

            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            typeof(TurnIndicatorView).GetField("_turnText", flags).SetValue(_turnView, _turnText);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_canvasGo);
        }

        [UnityTest]
        public IEnumerator UpdateTurn_HumanPlayer_ShowsYourTurn()
        {
            yield return null;

            var player = new PlayerState(0, true);
            _turnView.UpdateTurn(player);

            Assert.AreEqual("あなたのターン", _turnText.text);
        }

        [UnityTest]
        public IEnumerator UpdateTurn_AIPlayer_ShowsAITurn()
        {
            yield return null;

            var ai = new PlayerState(1, false);
            _turnView.UpdateTurn(ai);

            Assert.AreEqual("AI1のターン", _turnText.text);
        }
    }
}
