using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Core.Systems
{
    /// <summary>
    /// 場札の更新処理を行う。
    /// カードプレイ後の更新、疑似場札設定、クリアを担当する。
    /// </summary>
    public class FieldUpdater
    {
        /// <summary>
        /// 場札を最後に出したカードの色・番号で更新する。
        /// </summary>
        public void Update(FieldState field, Card lastPlayedCard)
        {
            field.Update(lastPlayedCard);
        }

        /// <summary>
        /// 疑似場札（好きな色の5）を設定する。
        /// 全員パス時に使用される。
        /// </summary>
        public void SetVirtualField(FieldState field, CardColor color)
        {
            field.SetVirtual(color);
        }

        /// <summary>
        /// 場札をクリアして初期状態に戻す。
        /// </summary>
        public void ClearField(FieldState field)
        {
            field.Clear();
        }
    }
}
