namespace MagicalCompetition.Core.Model
{
    /// <summary>ゲームの進行フェーズを表す列挙型。</summary>
    public enum GamePhase
    {
        Setup,            // カード配布・初期手札ドロー
        SelectFieldCard,  // 初期場札/リセット後の「好きな色の5」設定
        Play,             // カードを出す or パス
        Draw,             // 手札補充
        CheckWin,         // 勝利判定
        NextTurn,         // 次プレイヤーへ
        AllPassReset,     // 全員パス時の場札リセット
        End               // ゲーム終了・結果表示
    }
}
