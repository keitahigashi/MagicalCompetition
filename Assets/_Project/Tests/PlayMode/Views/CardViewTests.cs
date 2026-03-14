using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Views;

namespace MagicalCompetition.Tests.PlayMode.Views
{
    public class CardViewTests
    {
        private GameObject _go;
        private CardView _cardView;
        private Image _image;

        [SetUp]
        public void SetUp()
        {
            // Canvas必須（UI要素のため）
            var canvas = new GameObject("Canvas").AddComponent<Canvas>();
            _go = new GameObject("CardView");
            _go.transform.SetParent(canvas.transform);
            _go.AddComponent<RectTransform>();
            _image = _go.AddComponent<Image>();
            _cardView = _go.AddComponent<CardView>();
        }

        [TearDown]
        public void TearDown()
        {
            // Canvas含め全破棄
            if (_go != null && _go.transform.parent != null)
                Object.Destroy(_go.transform.parent.gameObject);
            else if (_go != null)
                Object.Destroy(_go);
        }

        [UnityTest]
        public IEnumerator SetCard_SetsCardData()
        {
            yield return null; // Awake実行待ち

            var card = new Card(CardColor.Fire, 5);
            _cardView.SetCard(card, null);

            Assert.AreEqual(card, _cardView.CardData);
        }

        [UnityTest]
        public IEnumerator SetSelected_True_MovesCardUp()
        {
            yield return null;

            var rt = _go.GetComponent<RectTransform>();
            float originalY = rt.anchoredPosition.y;

            _cardView.SetSelected(true);

            Assert.IsTrue(_cardView.IsSelected);
            Assert.Greater(rt.anchoredPosition.y, originalY);
        }

        [UnityTest]
        public IEnumerator SetSelected_False_RestoresPosition()
        {
            yield return null;

            _cardView.SetSelected(true);
            _cardView.SetSelected(false);

            Assert.IsFalse(_cardView.IsSelected);
        }

        [UnityTest]
        public IEnumerator SetInteractable_False_ReducesAlpha()
        {
            yield return null;

            _cardView.SetInteractable(false);

            var cg = _go.GetComponent<CanvasGroup>();
            Assert.Less(cg.alpha, 1f);
            Assert.IsFalse(_cardView.IsInteractable);
        }

        [UnityTest]
        public IEnumerator SetInteractable_True_RestoresAlpha()
        {
            yield return null;

            _cardView.SetInteractable(false);
            _cardView.SetInteractable(true);

            var cg = _go.GetComponent<CanvasGroup>();
            Assert.AreEqual(1f, cg.alpha);
            Assert.IsTrue(_cardView.IsInteractable);
        }

        [UnityTest]
        public IEnumerator OnClick_WhenInteractable_InvokesEvent()
        {
            yield return null;

            bool clicked = false;
            _cardView.OnClicked += (cv) => clicked = true;
            _cardView.SetCard(new Card(CardColor.Fire, 1), null);

            _cardView.OnClick();

            Assert.IsTrue(clicked);
        }

        [UnityTest]
        public IEnumerator OnClick_WhenNotInteractable_DoesNotInvokeEvent()
        {
            yield return null;

            bool clicked = false;
            _cardView.OnClicked += (cv) => clicked = true;
            _cardView.SetInteractable(false);

            _cardView.OnClick();

            Assert.IsFalse(clicked);
        }
    }
}
