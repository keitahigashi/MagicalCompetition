using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MagicalCompetition.UI;

namespace MagicalCompetition.Tests.PlayMode.UI
{
    public class TitleUITests
    {
        private GameObject _go;
        private TitleUI _titleUI;

        [SetUp]
        public void SetUp()
        {
            // TitleUI が Awake で Canvas + UI全体を自動生成する
            _go = new GameObject("TitleUI", typeof(RectTransform));
            _titleUI = _go.AddComponent<TitleUI>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_go);
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

            int receivedCount = -1;
            _titleUI.OnGameStart += count => receivedCount = count;
            _titleUI.SelectAICount(2);

            var method = typeof(TitleUI).GetMethod("HandleStartButton",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(_titleUI, null);

            Assert.AreEqual(2, receivedCount);
        }
    }
}
