namespace MagicalCompetition.Core.Model
{
    /// <summary>
    /// 場札の状態を表すクラス。Unity非依存の純粋C#クラス。
    /// </summary>
    public class FieldState
    {
        // TODO: GameConfig.InitialFieldNumber = 5 が確定したら定数を参照に切り替える
        private const int VirtualFieldNumber = 5;

        private CardColor _color;
        private int _number;
        private bool _isVirtual;

        /// <summary>現在の場札の色。</summary>
        public CardColor Color => _color;

        /// <summary>現在の場札の番号。</summary>
        public int Number => _number;

        /// <summary>疑似場札か（好きな色の5）。</summary>
        public bool IsVirtual => _isVirtual;

        /// <summary>
        /// デフォルトコンストラクタ。
        /// Color = Fire（enum先頭 = 0）, Number = 0, IsVirtual = false で初期化。
        /// </summary>
        public FieldState()
        {
            _color = default(CardColor);
            _number = 0;
            _isVirtual = false;
        }

        /// <summary>
        /// 場札を最後にプレイされたカードの内容で更新する。
        /// IsVirtual は false にリセットされる。
        /// </summary>
        /// <param name="lastPlayedCard">最後にプレイされたカード。</param>
        public void Update(Card lastPlayedCard)
        {
            _color = lastPlayedCard.Color;
            _number = lastPlayedCard.Number;
            _isVirtual = false;
        }

        /// <summary>
        /// 疑似場札（好きな色の5）を設定する。
        /// </summary>
        /// <param name="color">指定する色。</param>
        public void SetVirtual(CardColor color)
        {
            _color = color;
            _number = VirtualFieldNumber;
            _isVirtual = true;
        }

        /// <summary>
        /// 場札をリセットする。
        /// Color = Fire（default）, Number = 0, IsVirtual = false に戻す。
        /// </summary>
        public void Clear()
        {
            _color = default(CardColor);
            _number = 0;
            _isVirtual = false;
        }
    }
}
