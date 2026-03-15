using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using MagicalCompetition.Controllers;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.Systems;
using MagicalCompetition.Core.AI;
using MagicalCompetition.Views;

namespace MagicalCompetition.UI
{
    /// <summary>
    /// GameSceneのエントリーポイント。
    /// GameController / AIController / View を接続しゲームフローを駆動する。
    /// </summary>
    public class GameSceneManager : MonoBehaviour
    {
        private GameController _gameController;
        private AIController _aiController;
        private PlayerInputController _inputController;
        private IAIStrategy _aiStrategy;
        private GameConfig _config;

        // View参照（シーン上のオブジェクトから検索）
        private HandView _handView;
        private FieldView _fieldView;
        private DeckView _deckView;
        private TurnIndicatorView _turnIndicator;
        private ResultDialogView _resultDialog;
        private GameUI _gameUI;
        private AIInfoView[] _aiInfoViews;

        private void Start()
        {
            // AI人数取得
            int aiCount = Mathf.Clamp(SceneController.AICount, 1, 4);
            _config = new GameConfig(aiCount);

            // コントローラー初期化
            _gameController = new GameController();
            _aiController = new AIController();
            _inputController = new PlayerInputController();
            _aiStrategy = new NormalStrategy();

            // View取得
            _handView = FindAnyObjectByType<HandView>();
            _fieldView = FindAnyObjectByType<FieldView>();
            _deckView = FindAnyObjectByType<DeckView>();
            _turnIndicator = FindAnyObjectByType<TurnIndicatorView>();
            _resultDialog = FindAnyObjectByType<ResultDialogView>(FindObjectsInactive.Include);
            _gameUI = FindAnyObjectByType<GameUI>();
            _aiInfoViews = FindObjectsByType<AIInfoView>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            // AI情報パネルを人数分だけ表示
            ActivateAIPanels(aiCount);

            // UI イベント接続
            if (_gameUI != null)
            {
                _gameUI.OnPlayButtonClicked += OnPlayButton;
                _gameUI.OnPassButtonClicked += OnPassButton;
            }

            // リザルト → タイトルへ戻る
            if (_resultDialog != null)
                _resultDialog.OnTitleButtonClicked += () => SceneManager.LoadScene("TitleScene");

            // 手札カード選択 → InputController 接続
            if (_handView != null)
            {
                _handView.OnCardClicked += cv =>
                {
                    if (cv.IsSelected)
                        _inputController.SelectCard(cv.CardData);
                    else
                        _inputController.DeselectCard(cv.CardData);

                    // 出すボタンの有効/無効を更新
                    bool canPlay = _inputController.CanConfirmPlay();
                    if (_gameUI != null)
                        _gameUI.SetPlayButtonEnabled(canPlay);

                    // 出せる場合はプレビューをTurnIndicatorに表示
                    if (_turnIndicator != null)
                    {
                        if (canPlay)
                        {
                            var preview = GetPlayPreview();
                            _turnIndicator.ShowMessage(preview);
                        }
                        else
                        {
                            _turnIndicator.ShowMessage("あなたのターン");
                        }
                    }
                };
            }

            // ゲーム開始
            _gameController.ExecuteSetup(_config);
            _gameController.ExecuteSelectFieldCard(CardColor.Any);

            UpdateAllViews();
            StartPlayerTurn();
        }

        private void ActivateAIPanels(int aiCount)
        {
            // AIInfoViewをソート（名前順）
            if (_aiInfoViews == null) return;
            System.Array.Sort(_aiInfoViews, (a, b) => string.Compare(a.name, b.name));

            for (int i = 0; i < _aiInfoViews.Length; i++)
                _aiInfoViews[i].gameObject.SetActive(i < aiCount);
        }

        private void StartPlayerTurn()
        {
            var state = _gameController.State;
            if (state.CurrentPhase == GamePhase.End) return;

            if (state.CurrentPlayer.IsHuman)
            {
                // プレイヤーターン
                _inputController.WaitForInput(state.Field);
                if (_turnIndicator != null)
                    _turnIndicator.UpdateTurn(state.CurrentPlayer);
                if (_gameUI != null)
                {
                    _gameUI.SetPlayButtonEnabled(false);
                    _gameUI.SetPassButtonEnabled(true);
                }
            }
            else
            {
                // AIターン
                int aiIndex = state.CurrentPlayerIndex - 1;
                if (aiIndex >= 0 && aiIndex < _aiInfoViews.Length)
                    _aiInfoViews[aiIndex].ShowThinking();

                if (_turnIndicator != null)
                    _turnIndicator.UpdateTurn(state.CurrentPlayer);

                StartCoroutine(ExecuteAITurn());
            }
        }

        private IEnumerator ExecuteAITurn()
        {
            yield return new WaitForSeconds(1.0f);

            var state = _gameController.State;
            int aiIndex = state.CurrentPlayerIndex - 1;
            var action = _aiController.ExecuteTurn(state, _aiStrategy);

            if (aiIndex >= 0 && aiIndex < _aiInfoViews.Length)
                _aiInfoViews[aiIndex].HideThinking();

            var playerName = $"AI{state.CurrentPlayerIndex}";
            string aiLog;
            if (action.Type == PlayType.Pass)
            {
                _gameController.ExecutePass(action);
                aiLog = $"{playerName}:pass";
            }
            else
            {
                var fieldNumberBefore = state.Field.Number;
                _gameController.ExecutePlayCards(action);
                _gameController.ExecuteDraw();
                _gameController.ExecuteCheckWin();
                aiLog = FormatPlayLog(playerName, action, fieldNumberBefore);
            }
            Debug.Log(aiLog);

            UpdateAllViews();
            HandlePostAction();
        }

        private void OnPlayButton()
        {
            if (!_inputController.CanConfirmPlay()) return;

            var action = _inputController.ConfirmPlay();
            var fieldNumberBefore = _gameController.State.Field.Number;
            _gameController.ExecutePlayCards(action);
            _gameController.ExecuteDraw();
            _gameController.ExecuteCheckWin();

            Debug.Log(FormatPlayLog("Player", action, fieldNumberBefore));

            UpdateAllViews();

            if (_gameController.State.CurrentPhase == GamePhase.End)
            {
                ShowResult();
                return;
            }

            AdvanceToNextPlayer();
        }

        private void OnPassButton()
        {
            // 選択中のカードがあれば山札に戻すカードとして設定
            foreach (var card in _inputController.SelectedCards)
                _inputController.SelectCardToReturn(card);

            var action = _inputController.ConfirmPass();
            _gameController.ExecutePass(action);

            var returnedCards = action.Cards;
            string passLog;
            if (returnedCards.Count > 0)
            {
                var cardList = string.Join(",", returnedCards.Select(c => FormatCard(c)));
                passLog = $"Player:pass(return:{cardList})";
            }
            else
            {
                passLog = "Player:pass";
            }
            Debug.Log(passLog);

            UpdateAllViews();
            HandlePostAction();
        }

        private void AdvanceToNextPlayer()
        {
            _gameController.ExecuteNextTurn();
            UpdateAllViews();
            StartPlayerTurn();
        }

        private void HandlePostAction()
        {
            var state = _gameController.State;

            if (state.CurrentPhase == GamePhase.End)
            {
                ShowResult();
                return;
            }

            if (state.CurrentPhase == GamePhase.AllPassReset)
            {
                _gameController.ExecuteAllPassReset();
                _gameController.ExecuteSelectFieldCard(CardColor.Any);
                UpdateAllViews();
                // AllPassReset 後は現在のプレイヤーが続行
                StartPlayerTurn();
                return;
            }

            if (state.CurrentPhase == GamePhase.NextTurn)
            {
                AdvanceToNextPlayer();
            }
        }

        private void UpdateAllViews()
        {
            var state = _gameController.State;
            if (state == null) return;

            // プレイヤー手札（スプライト付き）
            var humanPlayer = state.Players[0];
            if (_handView != null)
                _handView.UpdateHand(humanPlayer.Hand, CardSpriteLoader.GetSprite);

            // 場札（スプライト付き）
            if (_fieldView != null)
            {
                var fieldCard = new Core.Model.Card(state.Field.Color, state.Field.Number);
                _fieldView.UpdateField(state.Field, CardSpriteLoader.GetSprite(fieldCard));
            }

            // デッキ・手札枚数
            if (_deckView != null)
            {
                _deckView.UpdateCount(humanPlayer.Deck.Count);
                _deckView.UpdateHandCount(humanPlayer.Hand.Count);
            }

            // AI情報
            for (int i = 0; i < _aiInfoViews.Length; i++)
            {
                int playerIndex = i + 1;
                if (playerIndex < state.Players.Count && _aiInfoViews[i].gameObject.activeSelf)
                    _aiInfoViews[i].UpdateInfo(state.Players[playerIndex]);
            }
        }

        private void ShowResult()
        {
            var result = _gameController.Result;
            Debug.Log($"[Game] END! Winner={result?.Winner?.PlayerId} Score={result?.Score} ResultDialog={_resultDialog != null}");

            if (_resultDialog != null && result != null)
            {
                _resultDialog.gameObject.SetActive(true);
                _resultDialog.Show(result);
            }

            // UI操作を無効化
            if (_gameUI != null)
            {
                _gameUI.SetPlayButtonEnabled(false);
                _gameUI.SetPassButtonEnabled(false);
            }
        }

        /// <summary>プレイアクションのログ文字列を生成する。</summary>
        private string FormatPlayLog(string playerName, PlayAction action, int fieldNumber)
        {
            var cards = action.Cards;
            var lastCard = action.LastCard;
            var cardList = string.Join(",", cards.Select(c => FormatCard(c)));

            string typeLabel;
            string detail;

            switch (action.Type)
            {
                case PlayType.SameNumber:
                    typeLabel = "number";
                    detail = $"[{cardList}]";
                    break;

                case PlayType.Arithmetic:
                    typeLabel = "same";
                    var validator = new ArithmeticValidator();
                    var expressions = validator.FindValidExpressions(cards, fieldNumber);
                    if (expressions.Count > 0)
                    {
                        var expr = expressions[0];
                        var sb = new StringBuilder();
                        sb.Append(FormatCard(expr.Cards[0]));
                        for (int i = 0; i < expr.Operators.Count; i++)
                        {
                            sb.Append(expr.Operators[i] == '+' ? "+" : "-");
                            sb.Append(FormatCard(expr.Cards[i + 1]));
                        }
                        sb.Append($" = {expr.Result}");
                        detail = $"[{sb}]";
                    }
                    else
                    {
                        detail = $"[{cardList} = {fieldNumber}]";
                    }
                    break;

                case PlayType.SameColor:
                    typeLabel = "color";
                    detail = $"[{cardList}]";
                    break;

                default:
                    typeLabel = action.Type.ToString().ToLower();
                    detail = $"[{cardList}]";
                    break;
            }

            var lastCardStr = lastCard != null ? $" {FormatCard(lastCard)}" : "";
            return $"{playerName}:{typeLabel}{detail}{lastCardStr}";
        }

        /// <summary>選択中カードのプレイプレビュー文字列を生成する（TurnIndicator用短縮形式）。</summary>
        private string GetPlayPreview()
        {
            var selected = new List<Card>(_inputController.SelectedCards);
            var field = _gameController.State.Field;
            var validator = new PlayValidator();
            var result = validator.Validate(selected, field);

            var cards = result.OrderedCards != null
                ? new List<Card>(result.OrderedCards)
                : selected;

            switch (result.Type)
            {
                case PlayType.SameNumber:
                    return $"同数字: {string.Join(", ", cards.Select(c => c.Number.ToString()))}";

                case PlayType.SameColor:
                    return $"同色: {string.Join(", ", cards.Select(c => c.Number.ToString()))}";

                case PlayType.Arithmetic:
                    var arithmeticValidator = new ArithmeticValidator();
                    var expressions = arithmeticValidator.FindValidExpressions(cards, field.Number);
                    if (expressions.Count > 0)
                    {
                        var expr = expressions[0];
                        var sb = new StringBuilder();
                        sb.Append($"計算: {expr.Cards[0].Number}");
                        for (int i = 0; i < expr.Operators.Count; i++)
                        {
                            sb.Append(expr.Operators[i] == '+' ? " + " : " - ");
                            sb.Append(expr.Cards[i + 1].Number);
                        }
                        sb.Append($" = {expr.Result}");
                        return sb.ToString();
                    }
                    return $"計算: {string.Join(", ", cards.Select(c => c.Number.ToString()))}";

                default:
                    return "あなたのターン";
            }
        }

        private static string FormatCard(Card card)
        {
            return $"{card.Color.ToString().ToLower()}{card.Number}";
        }

        private void OnDestroy()
        {
            if (_gameUI != null)
            {
                _gameUI.OnPlayButtonClicked -= OnPlayButton;
                _gameUI.OnPassButtonClicked -= OnPassButton;
            }
        }
    }
}
