using UnityEngine;
using UnityEngine.UI;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Views
{
    /// <summary>
    /// 場札エリアの表示コンポーネント。
    /// 現在の場札カードの色・番号を表示し、疑似場札時の特殊表示にも対応する。
    /// </summary>
    public class FieldView : MonoBehaviour
    {
        [SerializeField] private CardView _fieldCardView;
        [SerializeField] private Text _fieldInfoText;

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
    }
}
