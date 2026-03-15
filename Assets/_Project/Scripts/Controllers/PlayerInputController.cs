using System;
using System.Collections.Generic;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.Systems;

namespace MagicalCompetition.Controllers
{
    /// <summary>
    /// プレイヤーの入力処理を管理する。
    /// カード選択、妥当性チェック、確定操作、パス時のカード戻し選択を提供する。
    /// </summary>
    public class PlayerInputController
    {
        private readonly PlayValidator _playValidator = new PlayValidator();
        private readonly List<Card> _selectedCards = new List<Card>();
        private readonly List<Card> _cardsToReturn = new List<Card>();
        private FieldState _field;
        private bool _isPassMode;

        public IReadOnlyList<Card> SelectedCards => _selectedCards;
        public IReadOnlyList<Card> CardsToReturn => _cardsToReturn;
        public bool IsPassMode => _isPassMode;

        public event Action<Card> OnCardSelected;
        public event Action<Card> OnCardDeselected;
        public event Action<PlayAction> OnPlayConfirmed;
        public event Action<PlayAction> OnPassConfirmed;
        public event Action<bool> OnPlayValidityChanged;

        /// <summary>
        /// 入力待ちを開始する。場札を設定し、選択状態をクリアする。
        /// </summary>
        public void WaitForInput(FieldState field)
        {
            _field = field;
            _selectedCards.Clear();
            _cardsToReturn.Clear();
            _isPassMode = false;
        }

        /// <summary>カードを選択状態にする。</summary>
        public void SelectCard(Card card)
        {
            if (!_selectedCards.Contains(card))
            {
                _selectedCards.Add(card);
                OnCardSelected?.Invoke(card);
                OnPlayValidityChanged?.Invoke(CanConfirmPlay());
            }
        }

        /// <summary>カードの選択を解除する。</summary>
        public void DeselectCard(Card card)
        {
            if (_selectedCards.Remove(card))
            {
                OnCardDeselected?.Invoke(card);
                OnPlayValidityChanged?.Invoke(CanConfirmPlay());
            }
        }

        /// <summary>選択中のカードが合法なプレイかどうかを返す。</summary>
        public bool CanConfirmPlay()
        {
            if (_selectedCards.Count == 0 || _field == null)
                return false;

            var result = _playValidator.Validate(_selectedCards, _field);
            return result.IsValid;
        }

        /// <summary>カード出しを確定し、PlayActionを返す。</summary>
        public PlayAction ConfirmPlay()
        {
            var result = _playValidator.Validate(_selectedCards, _field);
            // Arithmetic の場合は式の順序に並んだカードを使用（LastCard が場札になる）
            var cards = result.OrderedCards != null
                ? new List<Card>(result.OrderedCards)
                : new List<Card>(_selectedCards);
            var action = PlayAction.CreatePlay(result.Type, cards);
            _selectedCards.Clear();
            OnPlayConfirmed?.Invoke(action);
            return action;
        }

        /// <summary>パスモードに切り替える。</summary>
        public void StartPassSelection()
        {
            _isPassMode = true;
            _selectedCards.Clear();
            _cardsToReturn.Clear();
        }

        /// <summary>パス時に山札に戻すカードを選択する。</summary>
        public void SelectCardToReturn(Card card)
        {
            if (!_cardsToReturn.Contains(card))
                _cardsToReturn.Add(card);
        }

        /// <summary>パス時に山札に戻すカードの選択を解除する。</summary>
        public void DeselectCardToReturn(Card card)
        {
            _cardsToReturn.Remove(card);
        }

        /// <summary>パスを確定し、PlayActionを返す。</summary>
        public PlayAction ConfirmPass()
        {
            var action = PlayAction.CreatePass(new List<Card>(_cardsToReturn));
            _cardsToReturn.Clear();
            _isPassMode = false;
            OnPassConfirmed?.Invoke(action);
            return action;
        }
    }
}
