using System;
using System.Collections.Generic;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Controllers
{
    /// <summary>
    /// 8フェーズのゲーム状態遷移を管理する。
    /// 不正な遷移を防止し、フェーズ開始・終了時のイベントを提供する。
    /// </summary>
    public class GameStateMachine
    {
        private static readonly Dictionary<GamePhase, HashSet<GamePhase>> _transitions =
            new Dictionary<GamePhase, HashSet<GamePhase>>
            {
                { GamePhase.Setup, new HashSet<GamePhase> { GamePhase.SelectFieldCard } },
                { GamePhase.SelectFieldCard, new HashSet<GamePhase> { GamePhase.Play } },
                { GamePhase.Play, new HashSet<GamePhase> { GamePhase.Draw, GamePhase.NextTurn, GamePhase.AllPassReset } },
                { GamePhase.Draw, new HashSet<GamePhase> { GamePhase.CheckWin } },
                { GamePhase.CheckWin, new HashSet<GamePhase> { GamePhase.NextTurn, GamePhase.End } },
                { GamePhase.NextTurn, new HashSet<GamePhase> { GamePhase.Play } },
                { GamePhase.AllPassReset, new HashSet<GamePhase> { GamePhase.SelectFieldCard } },
            };

        public GamePhase CurrentPhase { get; private set; }

        public event Action<GamePhase> OnPhaseEnter;
        public event Action<GamePhase> OnPhaseExit;

        public GameStateMachine()
        {
            CurrentPhase = GamePhase.Setup;
        }

        /// <summary>
        /// 次のフェーズへ遷移する。
        /// 不正な遷移の場合は InvalidOperationException をスローする。
        /// </summary>
        public void TransitionTo(GamePhase next)
        {
            if (!_transitions.ContainsKey(CurrentPhase) || !_transitions[CurrentPhase].Contains(next))
            {
                throw new InvalidOperationException(
                    $"Invalid transition: {CurrentPhase} → {next}");
            }

            var previous = CurrentPhase;
            OnPhaseExit?.Invoke(previous);
            CurrentPhase = next;
            OnPhaseEnter?.Invoke(next);
        }
    }
}
