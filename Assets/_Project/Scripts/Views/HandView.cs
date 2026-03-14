using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Views
{
    /// <summary>
    /// プレイヤー手札の表示・選択UI。
    /// CardViewをHorizontalLayoutGroupで横並びに配置する。
    /// </summary>
    public class HandView : MonoBehaviour
    {
        [SerializeField] private GameObject _cardViewPrefab;
        [SerializeField] private Transform _cardContainer;

        private readonly List<CardView> _cardViews = new List<CardView>();
        private readonly HashSet<CardView> _selectedCards = new HashSet<CardView>();

        public event Action<CardView> OnCardClicked;

        /// <summary>手札を更新し、CardViewを再生成する。</summary>
        public void UpdateHand(List<Card> hand, Func<Card, Sprite> spriteResolver = null)
        {
            ClearCardViews();

            foreach (var card in hand)
            {
                var go = Instantiate(_cardViewPrefab, _cardContainer);
                var cardView = go.GetComponent<CardView>();
                var sprite = spriteResolver?.Invoke(card);
                cardView.SetCard(card, sprite);
                cardView.OnClicked += HandleCardClicked;
                _cardViews.Add(cardView);
            }
        }

        /// <summary>選択中のCardViewリストを返す。</summary>
        public List<CardView> GetSelectedCards()
        {
            return _selectedCards.ToList();
        }

        /// <summary>全選択を解除する。</summary>
        public void ClearSelection()
        {
            foreach (var cv in _selectedCards)
                cv.SetSelected(false);
            _selectedCards.Clear();
        }

        /// <summary>出せるカードと出せないカードのグレーアウトを更新する。</summary>
        public void UpdateCardStates(List<PlayAction> validPlays)
        {
            var playableCards = new HashSet<Card>();
            if (validPlays != null)
            {
                foreach (var action in validPlays)
                    foreach (var card in action.Cards)
                        playableCards.Add(card);
            }

            foreach (var cv in _cardViews)
                cv.SetInteractable(playableCards.Contains(cv.CardData));
        }

        public int CardViewCount => _cardViews.Count;

        private void HandleCardClicked(CardView cardView)
        {
            if (_selectedCards.Contains(cardView))
            {
                _selectedCards.Remove(cardView);
                cardView.SetSelected(false);
            }
            else
            {
                _selectedCards.Add(cardView);
                cardView.SetSelected(true);
            }

            OnCardClicked?.Invoke(cardView);
        }

        private void ClearCardViews()
        {
            foreach (var cv in _cardViews)
            {
                cv.OnClicked -= HandleCardClicked;
                Destroy(cv.gameObject);
            }
            _cardViews.Clear();
            _selectedCards.Clear();
        }
    }
}
