using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Utils;

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
        [SerializeField] private GameObject _selectHighlight;

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
            _bgImage.color = new Color(0f, 0f, 0f, 0f);

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
            bg.color = new Color(0f, 0f, 0f, 0f);
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
            _cardText.font = FontProvider.Regular;
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
                default:              return new Color(0.25f, 0.18f, 0.40f);
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

        /// <summary>選択状態を切り替える。選択時はカードが上に浮き、黄色ボーダーを表示する。</summary>
        public void SetSelected(bool selected)
        {
            EnsureInit();
            var pos = _rectTransform.anchoredPosition;
            if (selected && !_isSelected)
            {
                // 選択開始: 現在位置を記録してから上へ移動
                _originalY = pos.y;
                pos.y += SelectedOffsetY;
            }
            else if (!selected && _isSelected)
            {
                // 選択解除: 記録した元の位置に戻す
                pos.y = _originalY;
            }
            _isSelected = selected;
            _rectTransform.anchoredPosition = pos;

            // 黄色ボーダーハイライト表示切替
            if (_selectHighlight != null)
                _selectHighlight.SetActive(selected);
        }

        /// <summary>操作可否を切り替える。無効時はグレーアウト表示。</summary>
        public void SetInteractable(bool enabled)
        {
            EnsureInit();
            _isInteractable = enabled;
            _canvasGroup.alpha = enabled ? 1f : DisabledAlpha;
            _canvasGroup.blocksRaycasts = enabled;
        }

        /// <summary>背景Imageを透過にする（アニメーション用）。</summary>
        public void SetBackgroundTransparent()
        {
            if (_bgImage != null)
                _bgImage.color = new Color(0f, 0f, 0f, 0f);
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
            var start = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }

            transform.position = target;
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

        /// <summary>カード裏面を表示する（AIカード用）。</summary>
        public void SetBackSprite(Sprite backSprite)
        {
            EnsureInit();
            EnsureLayout();

            if (_cardImage != null)
            {
                if (backSprite != null)
                {
                    _cardImage.sprite = backSprite;
                    _cardImage.color = Color.white;
                }
                else
                {
                    _cardImage.color = new Color(0f, 0f, 0f, 0f);
                }
            }

            if (_cardText != null)
                _cardText.text = "";
        }

        /// <summary>裏面→表面のフリップアニメーションを再生する。</summary>
        public void PlayFlipAnimation(Card card, Sprite faceSprite, float duration)
        {
            StartCoroutine(FlipCoroutine(card, faceSprite, duration));
        }

        /// <summary>ワールド座標での位置を返す。</summary>
        public Vector3 WorldPosition => _rectTransform != null ? _rectTransform.position : transform.position;

        private IEnumerator FlipCoroutine(Card card, Sprite faceSprite, float duration)
        {
            float half = duration * 0.5f;
            float elapsed = 0f;
            Vector3 baseScale = transform.localScale;

            // 前半: X スケール 1 → 0（裏面が縮む）
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / half);
                float scaleX = Mathf.Lerp(1f, 0f, t);
                transform.localScale = new Vector3(baseScale.x * scaleX, baseScale.y, baseScale.z);
                yield return null;
            }

            // 中間: カード表面に切り替え
            SetCard(card, faceSprite);

            // 後半: X スケール 0 → 1（表面が開く）
            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / half);
                float scaleX = Mathf.Lerp(0f, 1f, t);
                transform.localScale = new Vector3(baseScale.x * scaleX, baseScale.y, baseScale.z);
                yield return null;
            }

            transform.localScale = baseScale;
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClick);
        }
    }
}
