using System.Collections.Generic;

namespace MagicalCompetition.Core.Model
{
    /// <summary>
    /// プレイヤー1人分の状態を保持するクラス。
    /// Unity非依存の純粋C#クラス。
    /// </summary>
    public class PlayerState
    {
        // ─── プロパティ ───────────────────────────────────────────────────────

        /// <summary>プレイヤーID（0始まり）。</summary>
        public int PlayerId { get; }

        /// <summary>人間プレイヤーかどうか。</summary>
        public bool IsHuman { get; }

        /// <summary>手札。最大3枚（リーチ時は3枚未満になり得る）。</summary>
        public List<Card> Hand { get; }

        /// <summary>山札。インデックス0が先頭（トップ）。</summary>
        public List<Card> Deck { get; }

        /// <summary>リーチ状態（山札が空になったとき true にセットする）。</summary>
        public bool IsReach { get; set; }

        // ─── 計算プロパティ ───────────────────────────────────────────────────

        /// <summary>手札と山札の合計枚数。</summary>
        public int TotalCardCount => Hand.Count + Deck.Count;

        /// <summary>全カードが0枚のとき true（勝利判定用）。</summary>
        public bool HasWon => TotalCardCount == 0;

        // ─── コンストラクタ ───────────────────────────────────────────────────

        /// <summary>
        /// PlayerState を初期化する。
        /// Hand / Deck は空リストで生成され、IsReach は false。
        /// </summary>
        /// <param name="playerId">プレイヤーID。</param>
        /// <param name="isHuman">人間プレイヤーかどうか。</param>
        public PlayerState(int playerId, bool isHuman)
        {
            PlayerId = playerId;
            IsHuman  = isHuman;
            Hand     = new List<Card>();
            Deck     = new List<Card>();
            IsReach  = false;
        }

        // ─── 手札操作 ──────────────────────────────────────────────────────────

        /// <summary>手札にカードを1枚追加する。</summary>
        public void AddToHand(Card card)
        {
            Hand.Add(card);
        }

        /// <summary>手札から指定カードを1枚削除する。</summary>
        public void RemoveFromHand(Card card)
        {
            Hand.Remove(card);
        }

        /// <summary>手札から複数枚のカードをまとめて削除する。</summary>
        public void RemoveFromHand(List<Card> cards)
        {
            foreach (var card in cards)
                Hand.Remove(card);
        }

        // ─── 山札操作 ──────────────────────────────────────────────────────────

        /// <summary>
        /// 山札の先頭からカードを1枚引く。
        /// 山札が空の場合は null を返す（例外は投げない）。
        /// </summary>
        public Card DrawFromDeck()
        {
            if (Deck.Count == 0)
                return null;

            var top = Deck[0];
            Deck.RemoveAt(0);
            return top;
        }

        /// <summary>カード1枚を山札の末尾に戻す。</summary>
        public void ReturnToDeckBottom(Card card)
        {
            Deck.Add(card);
        }

        /// <summary>複数枚のカードを山札の末尾に順番通り追加する。</summary>
        public void ReturnToDeckBottom(List<Card> cards)
        {
            foreach (var card in cards)
                Deck.Add(card);
        }
    }
}
