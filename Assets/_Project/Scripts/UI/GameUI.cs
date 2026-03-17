using System;
using UnityEngine;
using UnityEngine.UI;

namespace MagicalCompetition.UI
{
    /// <summary>
    /// ゲーム画面の操作パネルUIコンポーネント。
    /// 「出す」「パス」「確定」ボタンとパス時カード戻し選択UIを管理する。
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _passButton;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private GameObject _returnCardPanel;

        public event Action OnPlayButtonClicked;
        public event Action OnPassButtonClicked;
        public event Action OnReturnConfirmClicked;

        /// <summary>出すボタンへの参照（スタイリング用）。</summary>
        internal Button _PlayButtonRef => _playButton;
        /// <summary>パスボタンへの参照（スタイリング用）。</summary>
        internal Button _PassButtonRef => _passButton;

        private void Awake()
        {
            if (_playButton != null)
                _playButton.onClick.AddListener(() => OnPlayButtonClicked?.Invoke());

            if (_passButton != null)
                _passButton.onClick.AddListener(() => OnPassButtonClicked?.Invoke());

            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(() => OnReturnConfirmClicked?.Invoke());

            HideReturnCardSelection();
        }

        /// <summary>「出す」ボタンの活性化/非活性化を制御する。</summary>
        public void SetPlayButtonEnabled(bool enabled)
        {
            if (_playButton != null)
                _playButton.interactable = enabled;
        }

        /// <summary>「パス」ボタンの活性化/非活性化を制御する。</summary>
        public void SetPassButtonEnabled(bool enabled)
        {
            if (_passButton != null)
                _passButton.interactable = enabled;
        }

        /// <summary>パス時カード戻し選択UIを表示する。</summary>
        public void ShowReturnCardSelection()
        {
            if (_returnCardPanel != null)
                _returnCardPanel.SetActive(true);
        }

        /// <summary>パス時カード戻し選択UIを非表示にする。</summary>
        public void HideReturnCardSelection()
        {
            if (_returnCardPanel != null)
                _returnCardPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_playButton != null)
                _playButton.onClick.RemoveAllListeners();

            if (_passButton != null)
                _passButton.onClick.RemoveAllListeners();

            if (_confirmButton != null)
                _confirmButton.onClick.RemoveAllListeners();
        }
    }
}
