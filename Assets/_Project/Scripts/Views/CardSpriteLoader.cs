using UnityEngine;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Views
{
    /// <summary>
    /// Card の Color と Number から対応するスプライトを解決する。
    /// Resources/CardSpriteTable をロードして使用する。
    /// </summary>
    public static class CardSpriteLoader
    {
        private static CardSpriteTable _table;

        /// <summary>Card に対応するスプライトを返す。見つからなければ null。</summary>
        public static Sprite GetSprite(Card card)
        {
            EnsureLoaded();
            if (_table == null) return null;
            return _table.Get($"{card.Color}_{card.Number}");
        }

        /// <summary>カード裏面スプライトを返す。</summary>
        public static Sprite GetBackSprite()
        {
            EnsureLoaded();
            return _table != null ? _table.backSprite : null;
        }

        /// <summary>ニュートラル（初手・パス流れ）スプライトを返す。</summary>
        public static Sprite GetNeutralSprite()
        {
            EnsureLoaded();
            return _table != null ? _table.neutralSprite : null;
        }

        private static void EnsureLoaded()
        {
            if (_table != null) return;
            _table = Resources.Load<CardSpriteTable>("CardSpriteTable");
        }
    }
}
