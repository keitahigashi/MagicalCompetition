using System.Collections.Generic;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Core.Systems
{
    /// <summary>
    /// 手札補充処理を行う。
    /// 初期ドロー（3枚）とターンごとの手札補充（3枚まで）を担当する。
    /// </summary>
    public class DrawSystem
    {
        private const int HandSize = 3;

        /// <summary>
        /// ゲーム開始時に各プレイヤーが山札から3枚引いて手札にする。
        /// </summary>
        public void InitialDraw(List<PlayerState> players)
        {
            foreach (var player in players)
            {
                for (int i = 0; i < HandSize; i++)
                    DrawOne(player);
            }
        }

        /// <summary>
        /// 手札が3枚になるまで山札から補充する。
        /// 山札が空になったらリーチ状態に移行する。
        /// </summary>
        public void Refill(PlayerState player)
        {
            while (player.Hand.Count < HandSize && player.Deck.Count > 0)
                DrawOne(player);

            if (player.Deck.Count == 0)
                player.IsReach = true;
        }

        private void DrawOne(PlayerState player)
        {
            var card = player.DrawFromDeck();
            if (card != null)
                player.AddToHand(card);
        }
    }
}
