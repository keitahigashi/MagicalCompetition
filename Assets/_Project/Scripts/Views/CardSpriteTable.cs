using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicalCompetition.Views
{
    /// <summary>
    /// カードスプライトのキーとスプライトの対応テーブル。
    /// エディタスクリプトで自動生成される。
    /// </summary>
    [CreateAssetMenu(fileName = "CardSpriteTable", menuName = "MagicalCompetition/CardSpriteTable")]
    public class CardSpriteTable : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string key;   // "Fire_1", "Water_3" 等
            public Sprite sprite;
        }

        public List<Entry> entries = new List<Entry>();
        public Sprite backSprite;
        public Sprite neutralSprite;

        private Dictionary<string, Sprite> _dict;

        public Sprite Get(string key)
        {
            if (_dict == null)
            {
                _dict = new Dictionary<string, Sprite>();
                foreach (var e in entries)
                    if (e.sprite != null)
                        _dict[e.key] = e.sprite;
            }
            _dict.TryGetValue(key, out var s);
            return s;
        }
    }
}
