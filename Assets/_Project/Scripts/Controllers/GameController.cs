using System.Collections.Generic;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.Systems;
using MagicalCompetition.Core.AI;

namespace MagicalCompetition.Controllers
{
    /// <summary>
    /// ゲーム全体のフロー制御を担う。
    /// 各フェーズの処理をシステムクラスに委譲し、StateMachineでフェーズ遷移を管理する。
    /// </summary>
    public class GameController
    {
        // ─── 依存システム ────────────────────────────────────────────────────
        private readonly CardDistributor _cardDistributor = new CardDistributor();
        private readonly DrawSystem _drawSystem = new DrawSystem();
        private readonly FieldUpdater _fieldUpdater = new FieldUpdater();
        private readonly TurnManager _turnManager = new TurnManager();
        private readonly WinChecker _winChecker = new WinChecker();
        private readonly ScoreCalculator _scoreCalculator = new ScoreCalculator();

        // ─── 状態 ────────────────────────────────────────────────────────────
        public GameStateMachine StateMachine { get; }
        public GameState State { get; private set; }
        public PlayResult Result { get; private set; }

        public GameController()
        {
            StateMachine = new GameStateMachine();
        }

        // ─── Setup フェーズ ──────────────────────────────────────────────────

        /// <summary>
        /// ゲームのセットアップを実行する。
        /// デッキ生成・シャッフル・配布・初期ドロー・疑似場札設定を行い、
        /// SelectFieldCardフェーズへ遷移する。
        /// </summary>
        public void ExecuteSetup(GameConfig config)
        {
            // プレイヤー生成
            var players = new List<PlayerState>();
            for (int i = 0; i < config.TotalPlayerCount; i++)
                players.Add(new PlayerState(i, i == 0));

            State = new GameState(players);

            // デッキ生成・シャッフル・配布
            var deck = _cardDistributor.CreateDeck();
            _cardDistributor.Shuffle(deck);
            var distributeResult = _cardDistributor.Distribute(deck, config.TotalPlayerCount);

            // 各プレイヤーに山札をセット
            for (int i = 0; i < config.TotalPlayerCount; i++)
            {
                foreach (var card in distributeResult.PlayerDecks[i])
                    players[i].Deck.Add(card);
            }

            // 余りカードを保存
            foreach (var card in distributeResult.ExcludedCards)
                State.ExcludedCards.Add(card);

            // 初期ドロー（各プレイヤー3枚）
            _drawSystem.InitialDraw(players);

            // 疑似場札設定
            _fieldUpdater.SetVirtualField(State.Field, CardColor.Any);

            // SelectFieldCardフェーズへ遷移
            StateMachine.TransitionTo(GamePhase.SelectFieldCard);
            State.CurrentPhase = GamePhase.SelectFieldCard;
        }

        // ─── SelectFieldCard フェーズ ────────────────────────────────────────

        /// <summary>
        /// 場札の色を選択し、Playフェーズへ遷移する。
        /// </summary>
        public void ExecuteSelectFieldCard(CardColor selectedColor)
        {
            _fieldUpdater.SetVirtualField(State.Field, selectedColor);
            StateMachine.TransitionTo(GamePhase.Play);
            State.CurrentPhase = GamePhase.Play;
        }

        // ─── Play フェーズ ───────────────────────────────────────────────────

        /// <summary>
        /// カード出しアクションを処理する。
        /// 手札からカードを削除し、場札を更新し、Drawフェーズへ遷移する。
        /// </summary>
        public void ExecutePlayCards(PlayAction action)
        {
            var player = State.CurrentPlayer;

            // 手札からカードを削除
            player.RemoveFromHand(action.Cards);

            // 捨て札に追加
            foreach (var card in action.Cards)
                State.DiscardPile.Add(card);

            // 場札を更新（最後のカードで）
            _fieldUpdater.Update(State.Field, action.LastCard);

            // パスカウントリセット
            _turnManager.RecordPlay(State);

            // Drawフェーズへ遷移
            StateMachine.TransitionTo(GamePhase.Draw);
            State.CurrentPhase = GamePhase.Draw;
        }

        /// <summary>
        /// パスアクションを処理する。
        /// パスカウントを増加させ、全員パスならAllPassReset、そうでなければNextTurnへ遷移する。
        /// </summary>
        public void ExecutePass(PlayAction action)
        {
            var player = State.CurrentPlayer;

            // パス時に手札を山札に戻す
            if (action.Cards.Count > 0)
            {
                player.RemoveFromHand(action.Cards);
                player.ReturnToDeckBottom(action.Cards);
            }

            _turnManager.RecordPass(State);

            if (_turnManager.IsAllPlayersPassed(State))
            {
                StateMachine.TransitionTo(GamePhase.AllPassReset);
                State.CurrentPhase = GamePhase.AllPassReset;
            }
            else
            {
                StateMachine.TransitionTo(GamePhase.NextTurn);
                State.CurrentPhase = GamePhase.NextTurn;
            }
        }

        // ─── Draw フェーズ ───────────────────────────────────────────────────

        /// <summary>
        /// 手札を3枚まで補充し、CheckWinフェーズへ遷移する。
        /// </summary>
        public void ExecuteDraw()
        {
            _drawSystem.Refill(State.CurrentPlayer);

            StateMachine.TransitionTo(GamePhase.CheckWin);
            State.CurrentPhase = GamePhase.CheckWin;
        }

        // ─── CheckWin フェーズ ───────────────────────────────────────────────

        /// <summary>
        /// 勝利判定を行い、勝利ならEndへ、継続ならNextTurnへ遷移する。
        /// </summary>
        public void ExecuteCheckWin()
        {
            if (_winChecker.Check(State.CurrentPlayer))
            {
                int score = _scoreCalculator.Calculate(State.CurrentPlayer, State.Players);
                Result = new PlayResult(State.CurrentPlayer, score, State.Players);

                StateMachine.TransitionTo(GamePhase.End);
                State.CurrentPhase = GamePhase.End;
            }
            else
            {
                StateMachine.TransitionTo(GamePhase.NextTurn);
                State.CurrentPhase = GamePhase.NextTurn;
            }
        }

        // ─── NextTurn フェーズ ───────────────────────────────────────────────

        /// <summary>
        /// 次プレイヤーに切り替え、Playフェーズへ遷移する。
        /// </summary>
        public void ExecuteNextTurn()
        {
            _turnManager.AdvanceTurn(State);

            StateMachine.TransitionTo(GamePhase.Play);
            State.CurrentPhase = GamePhase.Play;
        }

        // ─── AllPassReset フェーズ ───────────────────────────────────────────

        /// <summary>
        /// パスカウントリセット・場札クリアを行い、SelectFieldCardフェーズへ遷移する。
        /// </summary>
        public void ExecuteAllPassReset()
        {
            _turnManager.ResetForNewRound(State);
            _fieldUpdater.ClearField(State.Field);

            StateMachine.TransitionTo(GamePhase.SelectFieldCard);
            State.CurrentPhase = GamePhase.SelectFieldCard;
        }
    }
}
