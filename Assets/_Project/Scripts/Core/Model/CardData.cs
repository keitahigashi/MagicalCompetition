using UnityEngine;

namespace MagicalCompetition.Core.Model
{
    [CreateAssetMenu(fileName = "NewCardData", menuName = "MagicalCompetition/CardData")]
    public class CardData : ScriptableObject
    {
        public CardColor color;
        public int number;        // 1~9
        public Sprite cardSprite; // カード表面
        public Sprite backSprite; // カード裏面（共通）
    }
}
