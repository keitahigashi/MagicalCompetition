using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Views;

namespace MagicalCompetition.Tests.PlayMode.Views
{
    public class AIInfoViewTests
    {
        private GameObject _canvasGo;
        private AIInfoView _aiInfoView;
        private Text _handCountText;
        private Text _deckCountText;
        private GameObject _reachIcon;
        private GameObject _thinkingIcon;

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("Canvas");
            _canvasGo.AddComponent<Canvas>();

            var go = new GameObject("AIInfoView");
            go.transform.SetParent(_canvasGo.transform);
            _aiInfoView = go.AddComponent<AIInfoView>();

            var handTextGo = new GameObject("HandCountText");
            handTextGo.transform.SetParent(go.transform);
            handTextGo.AddComponent<RectTransform>();
            _handCountText = handTextGo.AddComponent<Text>();

            var deckTextGo = new GameObject("DeckCountText");
            deckTextGo.transform.SetParent(go.transform);
            deckTextGo.AddComponent<RectTransform>();
            _deckCountText = deckTextGo.AddComponent<Text>();

            _reachIcon = new GameObject("ReachIcon");
            _reachIcon.transform.SetParent(go.transform);
            _reachIcon.SetActive(false);

            _thinkingIcon = new GameObject("ThinkingIcon");
            _thinkingIcon.transform.SetParent(go.transform);
            _thinkingIcon.SetActive(false);

            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            typeof(AIInfoView).GetField("_handCountText", flags).SetValue(_aiInfoView, _handCountText);
            typeof(AIInfoView).GetField("_deckCountText", flags).SetValue(_aiInfoView, _deckCountText);
            typeof(AIInfoView).GetField("_reachIcon", flags).SetValue(_aiInfoView, _reachIcon);
            typeof(AIInfoView).GetField("_thinkingIcon", flags).SetValue(_aiInfoView, _thinkingIcon);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_canvasGo);
        }

        [UnityTest]
        public IEnumerator UpdateInfo_ShowsCorrectHandCount()
        {
            yield return null;

            var ai = new PlayerState(1, false);
            ai.AddToHand(new Card(CardColor.Fire, 1));
            ai.AddToHand(new Card(CardColor.Water, 2));

            _aiInfoView.UpdateInfo(ai);

            Assert.AreEqual("2", _handCountText.text);
        }

        [UnityTest]
        public IEnumerator UpdateInfo_ShowsCorrectDeckCount()
        {
            yield return null;

            var ai = new PlayerState(1, false);
            ai.Deck.Add(new Card(CardColor.Fire, 1));
            ai.Deck.Add(new Card(CardColor.Water, 2));
            ai.Deck.Add(new Card(CardColor.Light, 3));

            _aiInfoView.UpdateInfo(ai);

            Assert.AreEqual("3", _deckCountText.text);
        }

        [UnityTest]
        public IEnumerator UpdateInfo_ReachState_ShowsReachIcon()
        {
            yield return null;

            var ai = new PlayerState(1, false);
            ai.IsReach = true;

            _aiInfoView.UpdateInfo(ai);

            Assert.IsTrue(_reachIcon.activeSelf);
        }

        [UnityTest]
        public IEnumerator ShowThinking_ShowsThinkingIcon()
        {
            yield return null;

            _aiInfoView.ShowThinking();

            Assert.IsTrue(_thinkingIcon.activeSelf);
        }

        [UnityTest]
        public IEnumerator HideThinking_HidesThinkingIcon()
        {
            yield return null;

            _aiInfoView.ShowThinking();
            _aiInfoView.HideThinking();

            Assert.IsFalse(_thinkingIcon.activeSelf);
        }
    }
}
