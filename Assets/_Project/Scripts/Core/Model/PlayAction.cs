using System.Collections.Generic;

namespace MagicalCompetition.Core.Model
{
    /// <summary>
    /// プレイヤーが1ターンに行うアクションを表す不変クラス。
    /// ファクトリメソッド CreatePlay / CreatePass でのみ生成可能。
    /// Unity非依存の純粋C#クラス。
    /// </summary>
    public class PlayAction
    {
        public PlayType Type { get; }
        public List<Card> Cards { get; }

        /// <summary>
        /// Cardsの最後の1枚を返す。Cardsが空のときはnullを返す。
        /// </summary>
        public Card LastCard => Cards.Count > 0 ? Cards[Cards.Count - 1] : null;

        // コンストラクタはprivate。ファクトリメソッド経由でのみ生成する。
        private PlayAction(PlayType type, List<Card> cards)
        {
            Type = type;
            Cards = cards;
        }

        /// <summary>カードを出すアクションを生成する。</summary>
        /// <param name="type">プレイ種別（Pass以外）</param>
        /// <param name="cards">出すカードのリスト</param>
        public static PlayAction CreatePlay(PlayType type, List<Card> cards)
        {
            return new PlayAction(type, cards);
        }

        /// <summary>パスアクションを生成する。TypeはPlayType.Passに自動設定される。</summary>
        /// <param name="cardsToReturn">手札に戻す（または捨てる）カードのリスト</param>
        public static PlayAction CreatePass(List<Card> cardsToReturn)
        {
            return new PlayAction(PlayType.Pass, cardsToReturn);
        }
    }
}
