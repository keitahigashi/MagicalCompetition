using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Views
{
    /// <summary>
    /// 場札エリアの表示コンポーネント。
    /// 現在の場札カードの色・番号を表示し、疑似場札時の特殊表示にも対応する。
    /// Glow演出・コストバッジ・カード名ラベルを持つ。
    /// </summary>
    public class FieldView : MonoBehaviour
    {
        [SerializeField] private CardView _fieldCardView;
        [SerializeField] private Text _fieldInfoText;
        [SerializeField] private GameObject _glowEffect;
        [SerializeField] private Text _costBadgeText;
        [SerializeField] private Text _cardNameText;
        [SerializeField] private CanvasGroup _glowCanvasGroup;

        private Coroutine _glowCoroutine;

        /// <summary>場札の表示を更新する。</summary>
        public void UpdateField(FieldState field, Sprite cardSprite = null)
        {
            if (_fieldCardView != null)
            {
                var card = new Card(field.Color, field.Number);
                _fieldCardView.SetCard(card, cardSprite);
            }

            if (_fieldInfoText != null)
            {
                if (field.IsVirtual)
                {
                    _fieldInfoText.text = field.Color == CardColor.Any
                        ? "好きな色の5"
                        : $"{GetColorName(field.Color)}の5";
                }
                else
                {
                    _fieldInfoText.text = $"{GetColorName(field.Color)} {field.Number}";
                }
            }

            // コストバッジ: 場札の数字を表示
            if (_costBadgeText != null)
                _costBadgeText.text = field.Number.ToString();

            // カード名ラベル
            if (_cardNameText != null)
            {
                if (field.IsVirtual)
                    _cardNameText.text = field.Color == CardColor.Any ? "ニュートラル" : GetColorName(field.Color);
                else
                    _cardNameText.text = $"{GetColorName(field.Color)}{field.Number}";
            }

            // Glow パルスアニメーション
            UpdateGlow(field);
        }

        /// <summary>場札カードのワールド座標を返す（アニメーション先）。</summary>
        public Vector3 FieldWorldPosition
        {
            get
            {
                if (_fieldCardView != null)
                    return _fieldCardView.WorldPosition;
                return transform.position;
            }
        }

        private void UpdateGlow(FieldState field)
        {
            if (_glowEffect == null || _glowCanvasGroup == null) return;

            _glowEffect.SetActive(true);

            if (_glowCoroutine != null)
                StopCoroutine(_glowCoroutine);
            _glowCoroutine = StartCoroutine(GlowPulseCoroutine());
        }

        private IEnumerator GlowPulseCoroutine()
        {
            while (true)
            {
                // alpha 0.4 → 1.0 → 0.4 のパルス
                float elapsed = 0f;
                const float duration = 1.5f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    float alpha = Mathf.Lerp(0.4f, 1.0f, Mathf.PingPong(t * 2f, 1f));
                    if (_glowCanvasGroup != null)
                        _glowCanvasGroup.alpha = alpha;
                    yield return null;
                }
            }
        }

        private string GetColorName(CardColor color)
        {
            switch (color)
            {
                case CardColor.Fire: return "火";
                case CardColor.Water: return "水";
                case CardColor.Light: return "光";
                case CardColor.Earth: return "土";
                case CardColor.Wind: return "風";
                case CardColor.Any: return "任意";
                default: return color.ToString();
            }
        }

        private void OnDestroy()
        {
            if (_glowCoroutine != null)
                StopCoroutine(_glowCoroutine);
        }
    }
}
