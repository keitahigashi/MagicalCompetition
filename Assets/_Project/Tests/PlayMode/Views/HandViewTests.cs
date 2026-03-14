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
    public class HandViewTests
    {
        private GameObject _canvasGo;
        private GameObject _handGo;
        private HandView _handView;
        private GameObject _cardPrefab;

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("Canvas");
            _canvasGo.AddComponent<Canvas>();

            // CardViewプレハブ代替
            _cardPrefab = new GameObject("CardViewPrefab");
            _cardPrefab.AddComponent<RectTransform>();
            _cardPrefab.AddComponent<Image>();
            _cardPrefab.AddComponent<CardView>();
            _cardPrefab.SetActive(false); // プレハブとして非アクティブに

            _handGo = new GameObject("HandView");
            _handGo.transform.SetParent(_canvasGo.transform);
            _handGo.AddComponent<RectTransform>();

            var container = new GameObject("Container");
            container.transform.SetParent(_handGo.transform);
            container.AddComponent<RectTransform>();
            container.AddComponent<HorizontalLayoutGroup>();

            _handView = _handGo.AddComponent<HandView>();

            // SerializeFieldにリフレクションでセット
            var prefabField = typeof(HandView).GetField("_cardViewPrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prefabField.SetValue(_handView, _cardPrefab);

            var containerField = typeof(HandView).GetField("_cardContainer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            containerField.SetValue(_handView, container.transform);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_canvasGo);
            Object.Destroy(_cardPrefab);
        }

        [UnityTest]
        public IEnumerator UpdateHand_CreatesCorrectNumberOfCardViews()
        {
            yield return null;

            var hand = new List<Card>
            {
                new Card(CardColor.Fire, 1),
                new Card(CardColor.Water, 2),
                new Card(CardColor.Light, 3)
            };

            _handView.UpdateHand(hand);
            yield return null;

            Assert.AreEqual(3, _handView.CardViewCount);
        }

        [UnityTest]
        public IEnumerator GetSelectedCards_InitiallyEmpty()
        {
            yield return null;

            var selected = _handView.GetSelectedCards();
            Assert.AreEqual(0, selected.Count);
        }

        [UnityTest]
        public IEnumerator ClearSelection_ClearsAllSelections()
        {
            yield return null;

            var hand = new List<Card> { new Card(CardColor.Fire, 1) };
            _handView.UpdateHand(hand);
            yield return null;

            _handView.ClearSelection();
            var selected = _handView.GetSelectedCards();
            Assert.AreEqual(0, selected.Count);
        }

        [UnityTest]
        public IEnumerator UpdateHand_TwiceClearsOldCards()
        {
            yield return null;

            var hand1 = new List<Card> { new Card(CardColor.Fire, 1), new Card(CardColor.Water, 2) };
            _handView.UpdateHand(hand1);
            yield return null;

            var hand2 = new List<Card> { new Card(CardColor.Light, 3) };
            _handView.UpdateHand(hand2);
            yield return null;

            Assert.AreEqual(1, _handView.CardViewCount);
        }
    }
}
