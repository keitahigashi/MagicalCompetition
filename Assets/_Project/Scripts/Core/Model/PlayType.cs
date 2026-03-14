namespace MagicalCompetition.Core.Model
{
    /// <summary>カードのプレイ種別を表す列挙型。</summary>
    public enum PlayType
    {
        SameNumber,    // 同じ数字（REQ-101）
        SameColor,     // 同じ色（REQ-102）
        Arithmetic,    // 計算出し（REQ-103）
        Pass           // パス（REQ-106）
    }
}
