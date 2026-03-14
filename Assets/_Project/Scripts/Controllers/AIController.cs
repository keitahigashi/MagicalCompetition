using System.Collections.Generic;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.AI;

namespace MagicalCompetition.Controllers
{
    /// <summary>
    /// AIターンの実行制御を担う。
    /// 有効手探索、戦略による最適手選択、アクション実行を行う。
    /// </summary>
    public class AIController
    {
        private readonly AIActionEvaluator _evaluator = new AIActionEvaluator();

        /// <summary>思考中かどうか。</summary>
        public bool IsThinking { get; private set; }

        /// <summary>
        /// AIのターンを実行し、選択したPlayActionを返す。
        /// EditModeテスト向けの同期版。
        /// </summary>
        public PlayAction ExecuteTurn(GameState state, IAIStrategy strategy)
        {
            IsThinking = true;

            var hand = state.CurrentPlayer.Hand;
            var field = state.Field;

            // 全有効手を探索
            var validPlays = _evaluator.FindAllValidPlays(hand, field);

            // 戦略で最適手を選択
            var action = strategy.DecideAction(state, validPlays);

            IsThinking = false;
            return action;
        }
    }
}
