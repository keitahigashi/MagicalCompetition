using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Views
{
    /// <summary>
    /// ゲーム結果をモーダルダイアログで表示するコンポーネント。
    /// 勝者表示、得点表示、各プレイヤーの残カード枚数、タイトルに戻るボタンを含む。
    /// </summary>
    public class ResultDialogView : MonoBehaviour
    {
        [SerializeField] private Text _winnerText;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Transform _playerResultContainer;
        [SerializeField] private Button _titleButton;
        [SerializeField] private GameObject _dialogPanel;

        public event Action OnTitleButtonClicked;

        private void Awake()
        {
            if (_titleButton != null)
                _titleButton.onClick.AddListener(HandleTitleButtonClick);

            if (_dialogPanel != null)
                _dialogPanel.SetActive(false);
        }

        /// <summary>結果ダイアログを表示する。</summary>
        public void Show(PlayResult result)
        {
            if (_winnerText != null)
            {
                if (result.Winner.IsHuman)
                    _winnerText.text = "あなたの勝利！";
                else
                    _winnerText.text = $"AI{result.Winner.PlayerId}の勝利";
            }

            if (_scoreText != null)
                _scoreText.text = $"スコア: {result.Score}点";

            if (_dialogPanel != null)
                _dialogPanel.SetActive(true);
        }

        /// <summary>結果ダイアログを非表示にする。</summary>
        public void Hide()
        {
            if (_dialogPanel != null)
                _dialogPanel.SetActive(false);
        }

        private void HandleTitleButtonClick()
        {
            OnTitleButtonClicked?.Invoke();
        }

        private void OnDestroy()
        {
            if (_titleButton != null)
                _titleButton.onClick.RemoveListener(HandleTitleButtonClick);
        }
    }
}
