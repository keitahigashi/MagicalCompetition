using UnityEngine;
using UnityEngine.UI;

namespace MagicalCompetition.Views
{
    /// <summary>
    /// 山札の残り枚数表示コンポーネント。
    /// カード裏面画像と残り枚数テキスト、リーチ状態の表示を行う。
    /// SerializeFieldが未設定の場合はランタイムで自動生成する。
    /// </summary>
    public class DeckView : MonoBehaviour
    {
        [SerializeField] private Image _deckImage;
        [SerializeField] private Text _countText;
        [SerializeField] private GameObject _reachIndicator;
        [SerializeField] private Text _handCountText;

        private void Awake()
        {
            if (_countText == null)
            {
                BuildUI();
            }
            else
            {
                // GameSceneBuilderで作られた既存テキストを修正
                FixExistingText(_countText, new Vector2(0f, 0.5f), Vector2.one);
                EnsureHandCountText();
            }
        }

        private void FixExistingText(Text text, Vector2 anchorMin, Vector2 anchorMax)
        {
            if (text == null) return;
            var rt = text.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            text.fontSize = 14;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            if (text.font == null)
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        /// <summary>残り枚数を更新する。0枚時はリーチ表示。</summary>
        public void UpdateCount(int count)
        {
            if (_countText != null)
                _countText.text = $"山札: {count}枚";

            if (_reachIndicator != null)
                _reachIndicator.SetActive(count == 0);

            if (_deckImage != null)
                _deckImage.enabled = count > 0;
        }

        /// <summary>手札枚数を更新する。</summary>
        public void UpdateHandCount(int count)
        {
            if (_handCountText != null)
                _handCountText.text = $"手札: {count}枚";
        }

        private void EnsureHandCountText()
        {
            if (_handCountText != null) return;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var go = new GameObject("HandCountText", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            _handCountText = go.AddComponent<Text>();
            _handCountText.font = font;
            _handCountText.fontSize = 14;
            _handCountText.alignment = TextAnchor.MiddleCenter;
            _handCountText.color = Color.white;
            _handCountText.raycastTarget = false;
        }

        private void BuildUI()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var rt = GetComponent<RectTransform>();

            // 山札テキスト
            var deckTextGo = new GameObject("DeckCountText", typeof(RectTransform));
            deckTextGo.transform.SetParent(transform, false);
            var deckRT = deckTextGo.GetComponent<RectTransform>();
            deckRT.anchorMin = new Vector2(0f, 0.5f);
            deckRT.anchorMax = new Vector2(1f, 1f);
            deckRT.offsetMin = Vector2.zero;
            deckRT.offsetMax = Vector2.zero;
            _countText = deckTextGo.AddComponent<Text>();
            _countText.font = font;
            _countText.fontSize = 14;
            _countText.alignment = TextAnchor.MiddleCenter;
            _countText.color = Color.white;
            _countText.raycastTarget = false;

            // 手札テキスト
            var handTextGo = new GameObject("HandCountText", typeof(RectTransform));
            handTextGo.transform.SetParent(transform, false);
            var handRT = handTextGo.GetComponent<RectTransform>();
            handRT.anchorMin = new Vector2(0f, 0f);
            handRT.anchorMax = new Vector2(1f, 0.5f);
            handRT.offsetMin = Vector2.zero;
            handRT.offsetMax = Vector2.zero;
            _handCountText = handTextGo.AddComponent<Text>();
            _handCountText.font = font;
            _handCountText.fontSize = 14;
            _handCountText.alignment = TextAnchor.MiddleCenter;
            _handCountText.color = Color.white;
            _handCountText.raycastTarget = false;

            // リーチ表示
            var reachGo = new GameObject("ReachIndicator", typeof(RectTransform));
            reachGo.transform.SetParent(transform, false);
            var reachRT = reachGo.GetComponent<RectTransform>();
            reachRT.anchorMin = new Vector2(0.2f, 0.35f);
            reachRT.anchorMax = new Vector2(0.8f, 0.65f);
            reachRT.offsetMin = Vector2.zero;
            reachRT.offsetMax = Vector2.zero;
            var reachText = reachGo.AddComponent<Text>();
            reachText.text = "REACH!";
            reachText.font = font;
            reachText.fontSize = 14;
            reachText.alignment = TextAnchor.MiddleCenter;
            reachText.color = Color.red;
            reachText.raycastTarget = false;
            _reachIndicator = reachGo;
            _reachIndicator.SetActive(false);
        }
    }
}
