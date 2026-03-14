using UnityEngine;
using UnityEngine.UI;

namespace MagicalCompetition.Views
{
    /// <summary>
    /// 山札の残り枚数表示コンポーネント。
    /// カード裏面画像と残り枚数テキスト、リーチ状態の表示を行う。
    /// </summary>
    public class DeckView : MonoBehaviour
    {
        [SerializeField] private Image _deckImage;
        [SerializeField] private Text _countText;
        [SerializeField] private GameObject _reachIndicator;

        /// <summary>残り枚数を更新する。0枚時はリーチ表示。</summary>
        public void UpdateCount(int count)
        {
            if (_countText != null)
                _countText.text = count.ToString();

            if (_reachIndicator != null)
                _reachIndicator.SetActive(count == 0);

            if (_deckImage != null)
                _deckImage.enabled = count > 0;
        }
    }
}
