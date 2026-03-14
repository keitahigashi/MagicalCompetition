using System;
using System.Collections.Generic;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Core.Systems
{
    /// <summary>
    /// カードデッキの生成、シャッフル、プレイヤーへの配布を行う。
    /// Unity非依存の純粋C#クラス。
    /// </summary>
    public class CardDistributor
    {
        private readonly Random _defaultRandom = new Random();

        /// <summary>
        /// 5色×9番号 = 45枚のカードを生成する。
        /// </summary>
        public List<Card> CreateDeck()
        {
            var deck = new List<Card>(45);
            var colors = new[] { CardColor.Fire, CardColor.Water, CardColor.Light, CardColor.Earth, CardColor.Wind };

            foreach (var color in colors)
            {
                for (int number = 1; number <= 9; number++)
                {
                    deck.Add(new Card(color, number));
                }
            }

            return deck;
        }

        /// <summary>
        /// Fisher-Yatesアルゴリズムでデッキをインプレースシャッフルする。
        /// </summary>
        public void Shuffle(List<Card> deck)
        {
            Shuffle(deck, _defaultRandom);
        }

        /// <summary>
        /// Fisher-Yatesアルゴリズムでデッキをインプレースシャッフルする（Random注入版）。
        /// </summary>
        public void Shuffle(List<Card> deck, Random random)
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = deck[i];
                deck[i] = deck[j];
                deck[j] = temp;
            }
        }

        /// <summary>
        /// デッキを指定人数で均等に配布する。余りは除外カードとして返す。
        /// </summary>
        public DistributeResult Distribute(List<Card> deck, int playerCount)
        {
            int cardsPerPlayer = deck.Count / playerCount;
            int totalDistributed = cardsPerPlayer * playerCount;

            var playerDecks = new List<List<Card>>(playerCount);
            for (int i = 0; i < playerCount; i++)
            {
                int start = i * cardsPerPlayer;
                playerDecks.Add(deck.GetRange(start, cardsPerPlayer));
            }

            var excluded = deck.GetRange(totalDistributed, deck.Count - totalDistributed);

            return new DistributeResult(playerDecks, excluded);
        }
    }

    /// <summary>
    /// カード配布の結果を保持する。
    /// </summary>
    public class DistributeResult
    {
        /// <summary>各プレイヤーに配布されたカード。</summary>
        public List<List<Card>> PlayerDecks { get; }

        /// <summary>配布の余りとなった除外カード。</summary>
        public List<Card> ExcludedCards { get; }

        public DistributeResult(List<List<Card>> playerDecks, List<Card> excludedCards)
        {
            PlayerDecks = playerDecks;
            ExcludedCards = excludedCards;
        }
    }
}
