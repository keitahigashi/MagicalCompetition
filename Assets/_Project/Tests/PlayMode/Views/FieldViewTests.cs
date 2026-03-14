using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Views;

namespace MagicalCompetition.Tests.PlayMode.Views
{
    public class FieldViewTests
    {
        private GameObject _canvasGo;
        private FieldView _fieldView;
        private Text _infoText;

        [SetUp]
        public void SetUp()
        {
            _canvasGo = new GameObject("Canvas");
            _canvasGo.AddComponent<Canvas>();

            var go = new GameObject("FieldView");
            go.transform.SetParent(_canvasGo.transform);
            go.AddComponent<RectTransform>();
            _fieldView = go.AddComponent<FieldView>();

            // InfoText
            var textGo = new GameObject("InfoText");
            textGo.transform.SetParent(go.transform);
            textGo.AddComponent<RectTransform>();
            _infoText = textGo.AddComponent<Text>();

            var infoField = typeof(FieldView).GetField("_fieldInfoText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            infoField.SetValue(_fieldView, _infoText);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_canvasGo);
        }

        [UnityTest]
        public IEnumerator UpdateField_NormalField_ShowsColorAndNumber()
        {
            yield return null;

            var field = new FieldState();
            field.Update(new Card(CardColor.Fire, 7));

            _fieldView.UpdateField(field);

            Assert.IsTrue(_infoText.text.Contains("火"));
            Assert.IsTrue(_infoText.text.Contains("7"));
        }

        [UnityTest]
        public IEnumerator UpdateField_VirtualField_ShowsSpecialText()
        {
            yield return null;

            var field = new FieldState();
            field.SetVirtual(CardColor.Water);

            _fieldView.UpdateField(field);

            Assert.IsTrue(_infoText.text.Contains("水"));
            Assert.IsTrue(_infoText.text.Contains("5"));
        }

        [UnityTest]
        public IEnumerator UpdateField_VirtualAnyColor_ShowsAnyColorText()
        {
            yield return null;

            var field = new FieldState();
            field.SetVirtual(CardColor.Any);

            _fieldView.UpdateField(field);

            Assert.IsTrue(_infoText.text.Contains("好きな色"));
        }
    }
}
