using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Views
{
    /// <summary>
    /// 個別カードの表示コンポーネント（UI Imageベース）。
    /// 選択状態、グレーアウト表示、移動・フェードアニメーションを提供する。
    /// </summary>
    public class CardView : MonoBehaviour
    {
        private const float SelectedOffsetY = 20f;
        private const float DisabledAlpha = 0.4f;

        [SerializeField] private Image _cardImage;
        [SerializeField] private Text _cardText;

        private Image _bgImage;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Button _button;
        private float _originalY;
        private bool _isSelected;
        private bool _isInteractable = true;
        private bool _initialized;

        public Card CardData { get; private set; }
        public bool IsSelected => _isSelected;
        public bool IsInteractable => _isInteractable;

        public event Action<CardView> OnClicked;

        private void Awake()
        {
            EnsureInit();
        }

        private void EnsureInit()
        {
            if (_initialized) return;
            _initialized = true;

            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // 背景 Image（クリック判定用）
            _bgImage = GetComponent<Image>();
            if (_bgImage == null)
                _bgImage = gameObject.AddComponent<Image>();
            _bgImage.color = new Color(0.15f, 0.15f, 0.3f);

            // Button がなければ追加（クリック用）
            _button = GetComponent<Button>();
            if (_button == null)
            {
                _button = gameObject.AddComponent<Button>();
                _button.targetGraphic = _bgImage;
            }
            _button.onClick.AddListener(OnClick);

            _originalY = _rectTransform.anchoredPosition.y;
        }

        /// <summary>カードデータとスプライトを設定する。</summary>
        public void SetCard(Card card, Sprite sprite)
        {
            EnsureInit();
            CardData = card;
            EnsureLayout();

            if (_cardImage != null)
            {
                if (sprite != null)
                {
                    _cardImage.sprite = sprite;
                    _cardImage.color = Color.white;
                }
                else
                {
                    _cardImage.color = GetCardColor(card.Color);
                }
            }

            if (_cardText != null)
                _cardText.text = $"{GetColorLabel(card.Color)} {card.Number}";

            gameObject.SetActive(true);
        }

        /// <summary>画像（上部82%）とテキスト（下部18%）のレイアウトを構築する。</summary>
        private void EnsureLayout()
        {
            if (_cardText != null) return;

            // カード画像を子オブジェクトとして上部に配置
            var imgGo = new GameObject("CardImage", typeof(RectTransform));
            imgGo.transform.SetParent(transform, false);
            var imgRT = imgGo.GetComponent<RectTransform>();
            imgRT.anchorMin = new Vector2(0f, 0.18f);
            imgRT.anchorMax = Vector2.one;
            imgRT.offsetMin = Vector2.zero;
            imgRT.offsetMax = Vector2.zero;
            _cardImage = imgGo.AddComponent<Image>();
            _cardImage.preserveAspect = true;
            _cardImage.raycastTarget = false;

            // テキスト背景を下部に配置
            var bgGo = new GameObject("CardLabelBg", typeof(RectTransform));
            bgGo.transform.SetParent(transform, false);
            var bgRT = bgGo.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = new Vector2(1f, 0.18f);
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            var bg = bgGo.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.5f);
            bg.raycastTarget = false;

            // テキストを背景の子として配置
            var textGo = new GameObject("CardLabel", typeof(RectTransform));
            textGo.transform.SetParent(bgGo.transform, false);
            var rt = textGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _cardText = textGo.AddComponent<Text>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _cardText.font = font;
            _cardText.fontSize = 10;
            _cardText.alignment = TextAnchor.MiddleCenter;
            _cardText.color = Color.white;
            _cardText.raycastTarget = false;
            _cardText.resizeTextForBestFit = true;
            _cardText.resizeTextMinSize = 6;
            _cardText.resizeTextMaxSize = 12;
        }

        private static Color GetCardColor(CardColor color)
        {
            switch (color)
            {
                case CardColor.Fire:  return new Color(0.8f, 0.2f, 0.2f);
                case CardColor.Water: return new Color(0.2f, 0.4f, 0.8f);
                case CardColor.Light: return new Color(0.8f, 0.7f, 0.2f);
                case CardColor.Earth: return new Color(0.3f, 0.6f, 0.2f);
                case CardColor.Wind:  return new Color(0.5f, 0.3f, 0.7f);
                default:              return new Color(0.5f, 0.5f, 0.5f);
            }
        }

        private static string GetColorLabel(CardColor color)
        {
            switch (color)
            {
                case CardColor.Fire:  return "火";
                case CardColor.Water: return "水";
                case CardColor.Light: return "光";
                case CardColor.Earth: return "土";
                case CardColor.Wind:  return "風";
                default:              return "?";
            }
        }

        /// <summary>選択状態を切り替える。選択時はカードが上に浮く。</summary>
        public void SetSelected(bool selected)
        {
            EnsureInit();
            _isSelected = selected;
            var pos = _rectTransform.anchoredPosition;
            pos.y = selected ? _originalY + SelectedOffsetY : _originalY;
            _rectTransform.anchoredPosition = pos;
        }

        /// <summary>操作可否を切り替える。無効時はグレーアウト表示。</summary>
        public void SetInteractable(bool enabled)
        {
            EnsureInit();
            _isInteractable = enabled;
            _canvasGroup.alpha = enabled ? 1f : DisabledAlpha;
            _canvasGroup.blocksRaycasts = enabled;
        }

        /// <summary>移動アニメーションを再生する。</summary>
        public void PlayMoveAnimation(Vector3 target, float duration)
        {
            StartCoroutine(MoveCoroutine(target, duration));
        }

        /// <summary>フェードアニメーションを再生する。</summary>
        public void PlayFadeAnimation(float targetAlpha, float duration)
        {
            StartCoroutine(FadeCoroutine(targetAlpha, duration));
        }

        /// <summary>UIクリック時に呼ばれる（Button or EventTrigger経由）。</summary>
        public void OnClick()
        {
            if (_isInteractable)
                OnClicked?.Invoke(this);
        }

        private IEnumerator MoveCoroutine(Vector3 target, float duration)
        {
            var start = _rectTransform.anchoredPosition;
            var end = (Vector2)target;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _rectTransform.anchoredPosition = Vector2.Lerp(start, end, t);
                yield return null;
            }

            _rectTransform.anchoredPosition = end;
        }

        private IEnumerator FadeCoroutine(float targetAlpha, float duration)
        {
            float startAlpha = _canvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClick);
        }
    }
}
