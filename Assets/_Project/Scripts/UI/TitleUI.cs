using System;
using UnityEngine;
using UnityEngine.UI;

namespace MagicalCompetition.UI
{
    /// <summary>
    /// タイトル画面のUIコンポーネント。
    /// AI人数選択（1~4体）とゲーム開始機能を提供する。
    /// </summary>
    public class TitleUI : MonoBehaviour
    {
        private const int MinAICount = 1;
        private const int MaxAICount = 4;

        [SerializeField] private Button[] _aiCountButtons;
        [SerializeField] private Button _startButton;
        [SerializeField] private Text _selectedCountText;

        private int _selectedAICount = 1;

        public event Action<int> OnGameStart;

        public int SelectedAICount => _selectedAICount;

        private void Awake()
        {
            if (_aiCountButtons != null)
            {
                for (int i = 0; i < _aiCountButtons.Length; i++)
                {
                    int count = i + 1;
                    if (_aiCountButtons[i] != null)
                        _aiCountButtons[i].onClick.AddListener(() => SelectAICount(count));
                }
            }

            if (_startButton != null)
                _startButton.onClick.AddListener(HandleStartButton);

            UpdateSelectedCountText();
        }

        /// <summary>AI人数を選択する（1~4の範囲）。</summary>
        public void SelectAICount(int count)
        {
            _selectedAICount = Mathf.Clamp(count, MinAICount, MaxAICount);
            UpdateSelectedCountText();
        }

        private void HandleStartButton()
        {
            OnGameStart?.Invoke(_selectedAICount);
        }

        private void UpdateSelectedCountText()
        {
            if (_selectedCountText != null)
                _selectedCountText.text = $"AI {_selectedAICount}体";
        }

        private void OnDestroy()
        {
            if (_aiCountButtons != null)
            {
                foreach (var button in _aiCountButtons)
                {
                    if (button != null)
                        button.onClick.RemoveAllListeners();
                }
            }

            if (_startButton != null)
                _startButton.onClick.RemoveListener(HandleStartButton);
        }
    }
}
