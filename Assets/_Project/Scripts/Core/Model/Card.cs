namespace MagicalCompetition.Core.Model
{
    /// <summary>
    /// ゲームで使用する1枚のカードを表す不変クラス。
    /// Unity非依存の純粋C#クラス。
    /// </summary>
    public class Card
    {
        public CardColor Color { get; }
        public int Number { get; }

        public Card(CardColor color, int number)
        {
            Color = color;
            Number = number;
        }

        public override bool Equals(object obj)
        {
            if (obj is Card other)
                return Color == other.Color && Number == other.Number;
            return false;
        }

        public override int GetHashCode()
        {
            // .NET Framework / Unity Mono 互換の手動ハッシュ結合
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Color.GetHashCode();
                hash = hash * 31 + Number.GetHashCode();
                return hash;
            }
        }

        public override string ToString() => $"{Color}:{Number}";
    }
}
