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

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private float _originalY;
        private bool _isSelected;
        private bool _isInteractable = true;

        public Card CardData { get; private set; }
        public bool IsSelected => _isSelected;
        public bool IsInteractable => _isInteractable;

        public event Action<CardView> OnClicked;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            _originalY = _rectTransform.anchoredPosition.y;
        }

        /// <summary>カードデータとスプライトを設定する。</summary>
        public void SetCard(Card card, Sprite sprite)
        {
            CardData = card;
            if (_cardImage != null && sprite != null)
                _cardImage.sprite = sprite;
        }

        /// <summary>選択状態を切り替える。選択時はカードが上に浮く。</summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            var pos = _rectTransform.anchoredPosition;
            pos.y = selected ? _originalY + SelectedOffsetY : _originalY;
            _rectTransform.anchoredPosition = pos;
        }

        /// <summary>操作可否を切り替える。無効時はグレーアウト表示。</summary>
        public void SetInteractable(bool enabled)
        {
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
    }
}
