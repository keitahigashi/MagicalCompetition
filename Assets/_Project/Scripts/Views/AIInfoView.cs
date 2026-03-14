using UnityEngine;
using UnityEngine.UI;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Views
{
    /// <summary>
    /// AIプレイヤーの情報表示コンポーネント。
    /// AI名・手札枚数・山札枚数・リーチ状態・思考中表示を管理する。
    /// </summary>
    public class AIInfoView : MonoBehaviour
    {
        [SerializeField] private Text _aiNameText;
        [SerializeField] private Text _handCountText;
        [SerializeField] private Text _deckCountText;
        [SerializeField] private GameObject _reachIcon;
        [SerializeField] private GameObject _thinkingIcon;

        /// <summary>AIプレイヤーの情報を更新する。</summary>
        public void UpdateInfo(PlayerState aiPlayer)
        {
            if (_aiNameText != null)
                _aiNameText.text = $"AI{aiPlayer.PlayerId}";

            if (_handCountText != null)
                _handCountText.text = aiPlayer.Hand.Count.ToString();

            if (_deckCountText != null)
                _deckCountText.text = aiPlayer.Deck.Count.ToString();

            if (_reachIcon != null)
                _reachIcon.SetActive(aiPlayer.IsReach);
        }

        /// <summary>思考中アイコンを表示する。</summary>
        public void ShowThinking()
        {
            if (_thinkingIcon != null)
                _thinkingIcon.SetActive(true);
        }

        /// <summary>思考中アイコンを非表示にする。</summary>
        public void HideThinking()
        {
            if (_thinkingIcon != null)
                _thinkingIcon.SetActive(false);
        }
    }
}
