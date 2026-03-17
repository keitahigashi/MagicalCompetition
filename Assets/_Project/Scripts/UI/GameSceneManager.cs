using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MagicalCompetition.Controllers;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.Systems;
using MagicalCompetition.Core.AI;
using MagicalCompetition.Utils;
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
        private PlayLogView _playLogView;

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
            // 既存のResultDialogを破棄して新規生成（デザイン改修版）
            var existingResult = FindAnyObjectByType<ResultDialogView>(FindObjectsInactive.Include);
            if (existingResult != null)
                Destroy(existingResult.gameObject);
            _resultDialog = CreateResultDialog();
            _gameUI = FindAnyObjectByType<GameUI>();
            _aiInfoViews = FindObjectsByType<AIInfoView>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            // プレイログ生成（Canvas左側に配置）
            _playLogView = CreatePlayLogView();

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
                    {
                        _inputController.SelectCard(cv.CardData);
                        SoundManager.Instance.PlayCardSelect();
                    }
                    else
                    {
                        _inputController.DeselectCard(cv.CardData);
                        SoundManager.Instance.PlayCardDeselect();
                    }

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

            // UI見栄え適用
            ApplyVisualTheme();

            // ゲーム開始
            _gameController.ExecuteSetup(_config);
            _gameController.ExecuteSelectFieldCard(CardColor.Any);

            UpdateAllViews();
            StartPlayerTurn();

        }

        private ResultDialogView CreateResultDialog()
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return null;

            var go = new GameObject("ResultDialog", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);

            // 全画面ストレッチ
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // 最前面に配置
            go.transform.SetAsLastSibling();

            return go.AddComponent<ResultDialogView>();
        }

        private PlayLogView CreatePlayLogView()
        {
            // Canvasを検索
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return null;

            var go = new GameObject("PlayLogView", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.3f);
            rt.anchorMax = new Vector2(0.22f, 0.78f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            return go.AddComponent<PlayLogView>();
        }

        private void AddPlayLog(string message)
        {
            Debug.Log(message);
            if (_playLogView != null)
                _playLogView.AddLog(message);
        }

        /// <summary>GameScene全体の見栄えを整える（背景・ボタン・パネル）。</summary>
        private void ApplyVisualTheme()
        {
            // --- カメラ背景色 ---
            var cam = Camera.main ?? FindAnyObjectByType<Camera>();
            if (cam != null)
                cam.backgroundColor = UITheme.BgDeep;

            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            var canvasRT = canvas.GetComponent<RectTransform>();

            // --- 全画面グラデーション背景 ---
            CreateGradientBackground(canvasRT);

            // --- PlayerArea 背景パネル ---
            var playerAreaRT = _handView != null
                ? _handView.GetComponent<RectTransform>()?.parent?.GetComponent<RectTransform>()
                : null;
            if (playerAreaRT != null)
            {
                var panelImg = playerAreaRT.GetComponent<Image>();
                if (panelImg == null)
                    panelImg = playerAreaRT.gameObject.AddComponent<Image>();
                UITheme.ApplyRoundedRect(panelImg, UITheme.PanelDark);
            }

            // --- ボタンスタイル（名前で検索） ---
            var allButtons = canvasRT.GetComponentsInChildren<Button>(true);
            foreach (var btn in allButtons)
            {
                if (btn.name == "PlayButton")
                    StyleButton(btn, UITheme.BtnPlay, UITheme.BtnPlayHover, UITheme.BtnPlayPress);
                else if (btn.name == "PassButton")
                    StyleButton(btn, UITheme.BtnPass, UITheme.BtnPassHover, UITheme.BtnPassPress);
                else if (btn.name == "ConfirmButton")
                    StyleButton(btn, UITheme.BtnPurple, UITheme.BtnPurpleHover, UITheme.BtnPurplePress);
            }

            // --- TurnIndicator パネル背景 ---
            if (_turnIndicator != null)
            {
                var tiImg = _turnIndicator.GetComponent<Image>();
                if (tiImg != null)
                    UITheme.ApplyRoundedRect(tiImg, UITheme.PanelMid);
            }

            // --- FieldArea パネル背景 ---
            if (_fieldView != null)
            {
                // FieldView自身にImageがなければ追加
                var fvImg = _fieldView.GetComponent<Image>();
                if (fvImg == null)
                    fvImg = _fieldView.gameObject.AddComponent<Image>();
                UITheme.ApplyRoundedRect(fvImg, new Color(0.10f, 0.06f, 0.20f, 0.7f));

                // 子のImageをすべてテーマ色に（CardView関連は除外）
                foreach (Transform child in _fieldView.transform)
                {
                    var childImg = child.GetComponent<Image>();
                    if (childImg != null && child.GetComponent<CardView>() == null)
                        childImg.color = new Color(0.15f, 0.10f, 0.28f, 0.9f);
                }
            }

            // --- AIInfoArea 各パネル背景 ---
            if (_aiInfoViews != null)
            {
                foreach (var aiView in _aiInfoViews)
                {
                    var aiImg = aiView.GetComponent<Image>();
                    if (aiImg == null)
                        aiImg = aiView.gameObject.AddComponent<Image>();
                    UITheme.ApplyRoundedRect(aiImg, UITheme.PanelDark);
                }
            }
        }

        private void CreateGradientBackground(RectTransform canvasRT)
        {
            var bgGo = new GameObject("GradientBackground", typeof(RectTransform));
            bgGo.transform.SetParent(canvasRT, false);
            bgGo.transform.SetAsFirstSibling();

            var bgRT = bgGo.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;

            var bgImg = bgGo.AddComponent<RawImage>();
            bgImg.texture = UITheme.GradientBgTexture;
            bgImg.color = Color.white;
            bgImg.raycastTarget = false;
        }

        private static void StyleButton(Button button, Color normal, Color hover, Color pressed)
        {
            if (button == null) return;

            var img = button.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = UITheme.RoundedRectSmall;
                img.type = Image.Type.Sliced;
                img.color = normal;
            }

            // ColorBlockは相対的な明暗で制御
            var cb = ColorBlock.defaultColorBlock;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(hover.r / Mathf.Max(normal.r, 0.01f),
                hover.g / Mathf.Max(normal.g, 0.01f),
                hover.b / Mathf.Max(normal.b, 0.01f), 1f);
            cb.pressedColor = new Color(pressed.r / Mathf.Max(normal.r, 0.01f),
                pressed.g / Mathf.Max(normal.g, 0.01f),
                pressed.b / Mathf.Max(normal.b, 0.01f), 1f);
            cb.selectedColor = cb.highlightedColor;
            cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            cb.fadeDuration = 0.12f;
            button.colors = cb;

            // ボタン内テキストの色を白に統一
            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.color = UITheme.TextWhite;
                text.fontStyle = FontStyle.Bold;
            }

            // Outline追加
            var outline = button.GetComponent<Outline>();
            if (outline == null)
                outline = button.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.15f);
            outline.effectDistance = new Vector2(1, -1);
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
            SoundManager.Instance.PlayTurnStart();

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
            var aiView = (aiIndex >= 0 && aiIndex < _aiInfoViews.Length)
                ? _aiInfoViews[aiIndex] : null;
            var action = _aiController.ExecuteTurn(state, _aiStrategy);

            if (aiView != null) aiView.HideThinking();

            var playerName = $"AI{state.CurrentPlayerIndex}";
            string aiLog;
            if (action.Type == PlayType.Pass)
            {
                // パスアニメーション（裏面のまま山札へ → 補充）
                int handBefore = state.CurrentPlayer.Hand.Count;
                yield return StartCoroutine(AnimateAIPass(aiView, action));

                _gameController.ExecutePass(action);

                // 補充アニメーション
                int drawn = state.CurrentPlayer.Hand.Count - (handBefore - action.Cards.Count);
                yield return StartCoroutine(AnimateAIDraw(aiView, drawn));

                aiLog = action.Cards.Count > 0
                    ? $"{playerName}:pass(return:{string.Join(",", action.Cards.Select(c => FormatCard(c)))})"
                    : $"{playerName}:pass";
            }
            else
            {
                var fieldNumberBefore = state.Field.Number;
                int handBefore = state.CurrentPlayer.Hand.Count;

                // カード出しアニメーション（フリップ → 場札へ移動）
                yield return StartCoroutine(AnimateAIPlay(aiView, action));

                _gameController.ExecutePlayCards(action);
                _gameController.ExecuteDraw();
                _gameController.ExecuteCheckWin();

                // 補充アニメーション
                int drawn = state.CurrentPlayer.Hand.Count - (handBefore - action.Cards.Count);
                yield return StartCoroutine(AnimateAIDraw(aiView, drawn));

                aiLog = FormatPlayLog(playerName, action, fieldNumberBefore);
            }
            AddPlayLog(aiLog);

            UpdateAllViews();
            HandlePostAction();
        }

        /// <summary>AIカード出しアニメーション: 手札の裏面カード→フリップ→場札へ移動。</summary>
        private IEnumerator AnimateAIPlay(AIInfoView aiView, PlayAction action)
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null || aiView == null) yield break;

            Vector3 fieldPos = _fieldView != null ? _fieldView.FieldWorldPosition : transform.position;

            foreach (var card in action.Cards)
            {
                // AI手札の最後のカード裏面の位置を取得してから減らす
                Vector3 startPos = aiView.LastCardBackWorldPosition;
                aiView.RemoveCardBacks(1);

                // 一時的なCardViewを生成（裏面表示）
                var tempGo = CreateTempCard(canvas.transform, startPos);
                var cardView = tempGo.GetComponent<CardView>();

                var backSprite = CardSpriteLoader.GetBackSprite();
                cardView.SetBackSprite(backSprite);

                // フリップアニメーション（裏→表）
                var faceSprite = CardSpriteLoader.GetSprite(card);
                cardView.PlayFlipAnimation(card, faceSprite, 0.4f);
                SoundManager.Instance.PlayCardFlip();
                yield return new WaitForSeconds(0.45f);

                // 場札へ移動
                cardView.PlayMoveAnimation(fieldPos, 0.3f);
                yield return new WaitForSeconds(0.35f);

                Destroy(tempGo);
            }
        }

        /// <summary>AIパスアニメーション: 手札の裏面カードが裏のまま山札方向へ移動。</summary>
        private IEnumerator AnimateAIPass(AIInfoView aiView, PlayAction action)
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null || aiView == null) yield break;
            if (action.Cards.Count == 0) yield break;

            Vector3 deckPos = aiView.DeckWorldPosition;

            foreach (var card in action.Cards)
            {
                Vector3 startPos = aiView.LastCardBackWorldPosition;
                aiView.RemoveCardBacks(1);

                // 一時的なCardView（裏面のまま）
                var tempGo = CreateTempCard(canvas.transform, startPos);
                var cardView = tempGo.GetComponent<CardView>();

                var backSprite = CardSpriteLoader.GetBackSprite();
                cardView.SetBackSprite(backSprite);

                // 山札方向へ移動
                cardView.PlayMoveAnimation(deckPos, 0.3f);
                cardView.PlayFadeAnimation(0f, 0.3f);
                yield return new WaitForSeconds(0.35f);

                Destroy(tempGo);
            }
        }

        /// <summary>AI手札補充アニメーション: 山札からカード裏面が手札に追加される。</summary>
        private IEnumerator AnimateAIDraw(AIInfoView aiView, int drawCount)
        {
            if (aiView == null || drawCount <= 0) yield break;

            for (int i = 0; i < drawCount; i++)
            {
                aiView.AddCardBacks(1);
                SoundManager.Instance.PlayDraw();
                yield return new WaitForSeconds(0.15f);
            }
        }

        /// <summary>一時的なカードUIオブジェクトを生成する。</summary>
        private GameObject CreateTempCard(Transform parent, Vector3 worldPos)
        {
            var go = new GameObject("TempCard", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(60, 100);

            go.AddComponent<CardView>();
            return go;
        }

        private void OnPlayButton()
        {
            if (!_inputController.CanConfirmPlay()) return;

            // 操作を無効化してアニメーション中の二重操作を防ぐ
            if (_gameUI != null)
            {
                _gameUI.SetPlayButtonEnabled(false);
                _gameUI.SetPassButtonEnabled(false);
            }

            var action = _inputController.ConfirmPlay();
            SoundManager.Instance.PlayCardPlay();
            StartCoroutine(ExecutePlayerPlay(action));
        }

        private IEnumerator ExecutePlayerPlay(PlayAction action)
        {
            // 選択カードのアニメーション（手札 → 場札移動）
            yield return StartCoroutine(AnimatePlayerPlay(action));

            var fieldNumberBefore = _gameController.State.Field.Number;
            _gameController.ExecutePlayCards(action);
            _gameController.ExecuteDraw();
            _gameController.ExecuteCheckWin();

            AddPlayLog(FormatPlayLog("Player", action, fieldNumberBefore));

            UpdateAllViews();

            if (_gameController.State.CurrentPhase == GamePhase.End)
            {
                ShowResult();
                yield break;
            }

            AdvanceToNextPlayer();
        }

        /// <summary>プレイヤーカードプレイのアニメーション（手札→場札移動）。</summary>
        private IEnumerator AnimatePlayerPlay(PlayAction action)
        {
            if (_handView == null || _fieldView == null) yield break;

            Vector3 fieldPos = _fieldView.FieldWorldPosition;

            // 選択中のカードViewを見つける
            var selectedViews = new List<CardView>();
            var actionCards = new HashSet<Card>(action.Cards);
            foreach (var cv in _handView.CardViews)
            {
                if (actionCards.Contains(cv.CardData))
                    selectedViews.Add(cv);
            }

            // カードを1枚ずつ順番に場札へ移動
            foreach (var cv in selectedViews)
            {
                // LayoutGroupから外してワールド座標で自由に動かす
                var rt = cv.GetComponent<RectTransform>();
                Vector3 worldPos = rt.position;
                cv.transform.SetParent(cv.transform.root, true);
                rt.position = worldPos;

                cv.PlayMoveAnimation(fieldPos, 0.3f);
                yield return new WaitForSeconds(0.35f);
            }
        }

        private void OnPassButton()
        {
            // 選択中のカードがあれば山札に戻すカードとして設定
            foreach (var card in _inputController.SelectedCards)
                _inputController.SelectCardToReturn(card);

            var action = _inputController.ConfirmPass();
            SoundManager.Instance.PlayPass();
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
            AddPlayLog(passLog);

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
            if (result != null)
                ShowResult(result);
        }

        private void ShowResult(PlayResult result)
        {
            Debug.Log($"[Game] END! Winner={result.Winner?.PlayerId} Score={result.Score} ResultDialog={_resultDialog != null}");

            if (result.Winner != null && result.Winner.IsHuman)
                SoundManager.Instance.PlayWin();
            else
                SoundManager.Instance.PlayLose();

            if (_resultDialog != null)
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
