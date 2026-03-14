using UnityEngine;
using UnityEngine.UI;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Views
{
    /// <summary>
    /// 現在のターンプレイヤーを表示するコンポーネント。
    /// プレイヤーターンとAIターンで表示テキストを切り替える。
    /// </summary>
    public class TurnIndicatorView : MonoBehaviour
    {
        [SerializeField] private Text _turnText;

        /// <summary>ターン表示を更新する。</summary>
        public void UpdateTurn(PlayerState currentPlayer)
        {
            if (_turnText == null) return;

            if (currentPlayer.IsHuman)
            {
                _turnText.text = "あなたのターン";
            }
            else
            {
                _turnText.text = $"AI{currentPlayer.PlayerId}のターン";
            }
        }
    }
}
