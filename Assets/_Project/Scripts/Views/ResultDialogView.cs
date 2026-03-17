using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Utils;

namespace MagicalCompetition.Views
{
    /// <summary>
    /// ゲーム結果をモーダルダイアログで表示するコンポーネント。
    /// 背景暗転、ダイアログ登場アニメーション、勝利演出（紙吹雪）、
    /// スコアボード、タイトルに戻るボタンを含む。
    /// SerializeFieldが未設定の場合はランタイムで自動生成する。
    /// </summary>
    public class ResultDialogView : MonoBehaviour
    {
        [SerializeField] private Text _winnerText;
        [SerializeField] private Text _scoreText;
        [SerializeField] private Transform _playerResultContainer;
        [SerializeField] private Button _titleButton;
        [SerializeField] private GameObject _dialogPanel;

        private Image _overlayImage;
        private RectTransform _panelRT;
        private CanvasGroup _panelCanvasGroup;
        private readonly List<GameObject> _confettiPieces = new List<GameObject>();
        private readonly List<GameObject> _scoreRows = new List<GameObject>();
        private bool _isBuilt;
        private bool _isPlayerWin;

        public event Action OnTitleButtonClicked;

        private void Awake()
        {
            // シーン上で既存の DialogPanel がなければランタイムで構築
            if (_dialogPanel == null)
                BuildUI();
            else
                SetupExistingUI();

            // 初期非表示
            gameObject.SetActive(false);
        }

        /// <summary>結果ダイアログを表示する（演出付き）。</summary>
        public void Show(PlayResult result)
        {
            if (!_isBuilt) BuildUI();

            gameObject.SetActive(true);

            // 最前面に配置（PlayLogView等より前に表示）
            transform.SetAsLastSibling();

            // テキスト設定
            bool isPlayerWin = result.Winner.IsHuman;
            if (_winnerText != null)
                _winnerText.text = isPlayerWin ? "YOU WIN!" : $"AI{result.Winner.PlayerId} WIN";

            if (_scoreText != null)
                _scoreText.text = $"勝利点: {result.Score}点";

            // スコアボード生成
            BuildScoreboard(result);

            // 即座に表示状態にする（コルーチンに頼らない確実な表示）
            if (_overlayImage != null)
                _overlayImage.color = new Color(0f, 0f, 0f, 0.7f);
            if (_panelRT != null)
            {
                _panelRT.localScale = Vector3.one;
                if (_panelCanvasGroup != null)
                    _panelCanvasGroup.alpha = 1f;
            }

            // 追加演出（パルス・紙吹雪）をコルーチンで開始
            _isPlayerWin = isPlayerWin;
            Invoke(nameof(StartEffects), 0.1f);
        }

        /// <summary>結果ダイアログを非表示にする。</summary>
        public void Hide()
        {
            StopAllCoroutines();
            ClearConfetti();
            gameObject.SetActive(false);
        }

        private void HandleTitleButtonClick()
        {
            OnTitleButtonClicked?.Invoke();
        }

        private void OnDestroy()
        {
            if (_titleButton != null)
                _titleButton.onClick.RemoveListener(HandleTitleButtonClick);
        }

        // ─── 演出 ──────────────────────────────────────────────

        /// <summary>追加演出を開始する（Invoke経由で呼び出し）。</summary>
        private void StartEffects()
        {
            // 勝利テキストのパルス
            if (_winnerText != null)
                StartCoroutine(PulseText(_winnerText));

            // 紙吹雪（プレイヤー勝利時のみ）
            if (_isPlayerWin)
                StartCoroutine(SpawnConfetti());
        }

        private IEnumerator PulseText(Text text)
        {
            var rt = text.GetComponent<RectTransform>();
            if (rt == null) yield break;

            while (true)
            {
                float t = (Mathf.Sin(Time.time * 3f) + 1f) * 0.5f; // 0~1
                float scale = Mathf.Lerp(1.0f, 1.08f, t);
                rt.localScale = Vector3.one * scale;

                // 色の脈動（金→明るい金）
                text.color = Color.Lerp(UITheme.Gold, UITheme.GoldBright, t);
                yield return null;
            }
        }

        private IEnumerator SpawnConfetti()
        {
            var canvasRT = GetComponent<RectTransform>();
            if (canvasRT == null) yield break;

            float canvasW = canvasRT.rect.width;
            float canvasH = canvasRT.rect.height;

            Color[] colors = {
                new Color(1f, 0.3f, 0.3f),    // 赤
                new Color(0.3f, 0.5f, 1f),    // 青
                new Color(1f, 0.85f, 0.2f),   // 金
                new Color(0.3f, 0.8f, 0.4f),  // 緑
                new Color(0.8f, 0.4f, 0.9f),  // 紫
                new Color(1f, 0.6f, 0.2f),    // オレンジ
            };

            // バースト: 最初に一気に多数
            for (int i = 0; i < 40; i++)
            {
                SpawnOneConfetti(canvasRT, canvasW, canvasH, colors);
            }

            // 追加で少しずつ
            for (int i = 0; i < 30; i++)
            {
                yield return new WaitForSeconds(0.08f);
                SpawnOneConfetti(canvasRT, canvasW, canvasH, colors);
            }
        }

        private void SpawnOneConfetti(RectTransform parent, float canvasW, float canvasH, Color[] colors)
        {
            var go = new GameObject("Confetti", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            // ダイアログパネルの背面に配置（パネルの直前）
            if (_dialogPanel != null)
                go.transform.SetSiblingIndex(_dialogPanel.transform.GetSiblingIndex());

            var rt = go.GetComponent<RectTransform>();
            float startX = UnityEngine.Random.Range(-canvasW * 0.5f, canvasW * 0.5f);
            rt.anchoredPosition = new Vector2(startX, canvasH * 0.6f);
            float size = UnityEngine.Random.Range(4f, 10f);
            rt.sizeDelta = new Vector2(size, size * UnityEngine.Random.Range(1f, 2.5f));

            var img = go.AddComponent<Image>();
            img.color = colors[UnityEngine.Random.Range(0, colors.Length)];
            img.raycastTarget = false;

            _confettiPieces.Add(go);
            StartCoroutine(AnimateConfetti(go, rt, canvasH));
        }

        private IEnumerator AnimateConfetti(GameObject go, RectTransform rt, float canvasH)
        {
            float fallSpeed = UnityEngine.Random.Range(80f, 200f);
            float swaySpeed = UnityEngine.Random.Range(1.5f, 4f);
            float swayAmount = UnityEngine.Random.Range(20f, 60f);
            float rotSpeed = UnityEngine.Random.Range(90f, 360f);
            float startX = rt.anchoredPosition.x;
            float phase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float bottomY = -canvasH * 0.6f;

            while (rt != null && rt.anchoredPosition.y > bottomY)
            {
                var pos = rt.anchoredPosition;
                pos.y -= fallSpeed * Time.deltaTime;
                pos.x = startX + Mathf.Sin(Time.time * swaySpeed + phase) * swayAmount;
                rt.anchoredPosition = pos;
                rt.Rotate(0, 0, rotSpeed * Time.deltaTime);
                yield return null;
            }

            if (go != null)
            {
                _confettiPieces.Remove(go);
                Destroy(go);
            }
        }

        private void ClearConfetti()
        {
            foreach (var go in _confettiPieces)
            {
                if (go != null) Destroy(go);
            }
            _confettiPieces.Clear();
        }

        // ─── UI構築 ────────────────────────────────────────────

        private void SetupExistingUI()
        {
            _isBuilt = true;

            // オーバーレイ（背景暗転）を追加
            var overlayGo = CreateRect("Overlay", transform);
            Stretch(overlayGo);
            overlayGo.transform.SetAsFirstSibling();
            _overlayImage = overlayGo.AddComponent<Image>();
            _overlayImage.color = Color.clear;
            _overlayImage.raycastTarget = true;

            // 既存パネルにCanvasGroupを追加
            if (_dialogPanel != null)
            {
                _panelRT = _dialogPanel.GetComponent<RectTransform>();
                _panelCanvasGroup = _dialogPanel.GetComponent<CanvasGroup>();
                if (_panelCanvasGroup == null)
                    _panelCanvasGroup = _dialogPanel.AddComponent<CanvasGroup>();
            }

            if (_titleButton != null)
                _titleButton.onClick.AddListener(HandleTitleButtonClick);
        }

        private void BuildUI()
        {
            _isBuilt = true;
            var font = FontProvider.Regular;
            var boldFont = FontProvider.Bold;
            var rt = GetComponent<RectTransform>();

            // 全画面ストレッチ
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // === 背景オーバーレイ ===
            var overlayGo = CreateRect("Overlay", rt);
            Stretch(overlayGo);
            _overlayImage = overlayGo.AddComponent<Image>();
            _overlayImage.color = Color.clear;
            _overlayImage.raycastTarget = true;

            // === ダイアログパネル ===
            var panelGo = CreateRect("DialogPanel", rt);
            _dialogPanel = panelGo;
            _panelRT = panelGo.GetComponent<RectTransform>();
            _panelRT.anchorMin = new Vector2(0.15f, 0.1f);
            _panelRT.anchorMax = new Vector2(0.85f, 0.9f);
            _panelRT.offsetMin = Vector2.zero;
            _panelRT.offsetMax = Vector2.zero;

            _panelCanvasGroup = panelGo.AddComponent<CanvasGroup>();

            // パネル背景（角丸）
            var panelImg = panelGo.AddComponent<Image>();
            UITheme.ApplyRoundedRect(panelImg, new Color(0.10f, 0.06f, 0.18f, 0.95f));

            // パネル枠線
            var panelOutline = panelGo.AddComponent<Outline>();
            panelOutline.effectColor = UITheme.GoldDim;
            panelOutline.effectDistance = new Vector2(2, -2);

            // === 内部レイアウト ===
            var layout = panelGo.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 8;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlHeight = true;
            layout.padding = new RectOffset(20, 20, 20, 20);

            // === 上部装飾ライン ===
            var topLine = CreateLayoutRect("TopLine", panelGo.transform, -1, 2);
            var topLineImg = topLine.AddComponent<Image>();
            topLineImg.color = UITheme.GoldDim;
            topLineImg.raycastTarget = false;

            // === 勝者テキスト ===
            var winGo = CreateLayoutRect("WinnerText", panelGo.transform, -1, 50);
            _winnerText = winGo.AddComponent<Text>();
            _winnerText.font = boldFont ?? font;
            _winnerText.fontSize = 32;
            _winnerText.alignment = TextAnchor.MiddleCenter;
            _winnerText.color = UITheme.Gold;
            _winnerText.resizeTextForBestFit = true;
            _winnerText.resizeTextMinSize = 18;
            _winnerText.resizeTextMaxSize = 32;
            _winnerText.raycastTarget = false;

            // === スコアテキスト ===
            var scoreGo = CreateLayoutRect("ScoreText", panelGo.transform, -1, 36);
            _scoreText = scoreGo.AddComponent<Text>();
            _scoreText.font = boldFont ?? font;
            _scoreText.fontSize = 24;
            _scoreText.alignment = TextAnchor.MiddleCenter;
            _scoreText.color = UITheme.GoldBright;
            _scoreText.resizeTextForBestFit = true;
            _scoreText.resizeTextMinSize = 16;
            _scoreText.resizeTextMaxSize = 24;
            _scoreText.raycastTarget = false;

            // === 中央装飾ライン ===
            var midLine = CreateLayoutRect("MidLine", panelGo.transform, -1, 1);
            var midLineImg = midLine.AddComponent<Image>();
            midLineImg.color = new Color(UITheme.GoldDim.r, UITheme.GoldDim.g, UITheme.GoldDim.b, 0.4f);
            midLineImg.raycastTarget = false;

            // === スコアボードコンテナ ===
            var containerGo = CreateLayoutRect("ScoreboardContainer", panelGo.transform, -1, 120);
            _playerResultContainer = containerGo.transform;
            var containerLayout = containerGo.AddComponent<VerticalLayoutGroup>();
            containerLayout.childAlignment = TextAnchor.UpperCenter;
            containerLayout.spacing = 4;
            containerLayout.childForceExpandWidth = true;
            containerLayout.childForceExpandHeight = false;
            containerLayout.childControlHeight = true;
            containerLayout.padding = new RectOffset(10, 10, 5, 5);

            // === 下部装飾ライン ===
            var bottomLine = CreateLayoutRect("BottomLine", panelGo.transform, -1, 2);
            var bottomLineImg = bottomLine.AddComponent<Image>();
            bottomLineImg.color = UITheme.GoldDim;
            bottomLineImg.raycastTarget = false;

            // === タイトルに戻るボタン ===
            var btnGo = CreateLayoutRect("TitleButton", panelGo.transform, -1, 40);
            var btnImg = btnGo.AddComponent<Image>();
            btnImg.sprite = UITheme.RoundedRectSmall;
            btnImg.type = Image.Type.Sliced;
            btnImg.color = UITheme.BtnPurple;

            _titleButton = btnGo.AddComponent<Button>();
            _titleButton.targetGraphic = btnImg;
            var cb = ColorBlock.defaultColorBlock;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(
                UITheme.BtnPurpleHover.r / Mathf.Max(UITheme.BtnPurple.r, 0.01f),
                UITheme.BtnPurpleHover.g / Mathf.Max(UITheme.BtnPurple.g, 0.01f),
                UITheme.BtnPurpleHover.b / Mathf.Max(UITheme.BtnPurple.b, 0.01f), 1f);
            cb.pressedColor = new Color(
                UITheme.BtnPurplePress.r / Mathf.Max(UITheme.BtnPurple.r, 0.01f),
                UITheme.BtnPurplePress.g / Mathf.Max(UITheme.BtnPurple.g, 0.01f),
                UITheme.BtnPurplePress.b / Mathf.Max(UITheme.BtnPurple.b, 0.01f), 1f);
            cb.selectedColor = cb.highlightedColor;
            cb.fadeDuration = 0.12f;
            _titleButton.colors = cb;
            _titleButton.onClick.AddListener(HandleTitleButtonClick);

            var btnTextGo = CreateRect("Text", btnGo.transform);
            Stretch(btnTextGo);
            var btnText = btnTextGo.AddComponent<Text>();
            btnText.text = "タイトルに戻る";
            btnText.font = font;
            btnText.fontSize = 16;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = UITheme.TextWhite;
            btnText.raycastTarget = false;

            var btnOutline = btnGo.AddComponent<Outline>();
            btnOutline.effectColor = new Color(1f, 1f, 1f, 0.12f);
            btnOutline.effectDistance = new Vector2(1, -1);
        }

        private void BuildScoreboard(PlayResult result)
        {
            // 既存行クリア
            foreach (var row in _scoreRows)
            {
                if (row != null) Destroy(row);
            }
            _scoreRows.Clear();

            if (_playerResultContainer == null || result.AllPlayers == null) return;

            var font = FontProvider.Regular;

            // ヘッダー行
            var headerGo = CreateScoreRow(_playerResultContainer, "Header",
                "Player", "Hand", "Deck", "Total", font, true);
            _scoreRows.Add(headerGo);

            // ランキング順にソート（TotalCardCount昇順）
            var sorted = new List<PlayerState>(result.AllPlayers);
            sorted.Sort((a, b) => a.TotalCardCount.CompareTo(b.TotalCardCount));

            int rank = 0;
            foreach (var player in sorted)
            {
                rank++;
                string name = player.IsHuman ? "You" : $"AI{player.PlayerId}";
                bool isWinner = player == result.Winner;

                var rowGo = CreateScoreRow(_playerResultContainer, $"Row_{rank}",
                    $"{rank}. {name}",
                    $"{player.Hand.Count}",
                    $"{player.Deck.Count}",
                    $"{player.TotalCardCount}",
                    font, false, isWinner);
                _scoreRows.Add(rowGo);
            }
        }

        private GameObject CreateScoreRow(Transform parent, string name,
            string col1, string col2, string col3, string col4,
            Font font, bool isHeader, bool isHighlight = false)
        {
            var go = CreateLayoutRect(name, parent, -1, isHeader ? 22 : 20);

            // 行背景
            var bgImg = go.AddComponent<Image>();
            if (isHighlight)
                bgImg.color = new Color(UITheme.Gold.r, UITheme.Gold.g, UITheme.Gold.b, 0.15f);
            else if (isHeader)
                bgImg.color = new Color(1f, 1f, 1f, 0.05f);
            else
                bgImg.color = Color.clear;
            bgImg.raycastTarget = false;

            var hLayout = go.AddComponent<HorizontalLayoutGroup>();
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.spacing = 4;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = true;
            hLayout.padding = new RectOffset(8, 8, 0, 0);

            int fontSize = isHeader ? 11 : 13;
            Color textColor = isHeader ? UITheme.GoldDim
                : isHighlight ? UITheme.GoldBright : UITheme.TextCream;
            FontStyle style = isHeader ? FontStyle.Bold
                : isHighlight ? FontStyle.Bold : FontStyle.Normal;

            // 列 (名前は広め、数値は均等)
            AddColumnText(go.transform, col1, font, fontSize, textColor, style, 3f);
            AddColumnText(go.transform, col2, font, fontSize, textColor, style, 1f);
            AddColumnText(go.transform, col3, font, fontSize, textColor, style, 1f);
            AddColumnText(go.transform, col4, font, fontSize, textColor, style, 1f);

            return go;
        }

        private static void AddColumnText(Transform parent, string text,
            Font font, int fontSize, Color color, FontStyle style, float flex)
        {
            var go = CreateRect("Col", parent);
            var le = go.AddComponent<LayoutElement>();
            le.flexibleWidth = flex;

            var t = go.AddComponent<Text>();
            t.text = text;
            t.font = font;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
            t.raycastTarget = false;
        }

        // ─── ヘルパー ──────────────────────────────────────────

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        private static GameObject CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreateLayoutRect(string name, Transform parent,
            float flexW, float prefH)
        {
            var go = CreateRect(name, parent);
            var le = go.AddComponent<LayoutElement>();
            if (flexW >= 0) le.flexibleWidth = flexW;
            if (prefH >= 0) le.preferredHeight = prefH;
            return go;
        }

        private static void Stretch(GameObject go)
        {
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
        }
    }
}
