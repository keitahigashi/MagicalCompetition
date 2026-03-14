using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MagicalCompetition.Controllers;

namespace MagicalCompetition.UI
{
    /// <summary>
    /// タイトル画面のUIコンポーネント。
    /// AI人数選択（◀ N体 ▶）とゲーム開始ボタンを提供する。
    /// SerializeFieldが未設定の場合はランタイムで自動生成する。
    /// </summary>
    public class TitleUI : MonoBehaviour
    {
        private const int MinAICount = 1;
        private const int MaxAICount = 4;

        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Text _countText;
        [SerializeField] private Button _startButton;

        private int _selectedAICount = 1;

        public event Action<int> OnGameStart;

        public int SelectedAICount => _selectedAICount;

        private void Awake()
        {
            if (_startButton == null)
                BuildUI();

            if (_prevButton != null) _prevButton.onClick.AddListener(OnPrev);
            if (_nextButton != null) _nextButton.onClick.AddListener(OnNext);
            if (_startButton != null) _startButton.onClick.AddListener(HandleStartButton);

            UpdateCountText();
        }

        /// <summary>AI人数を選択する（1~4の範囲）。</summary>
        public void SelectAICount(int count)
        {
            _selectedAICount = Mathf.Clamp(count, MinAICount, MaxAICount);
            UpdateCountText();
        }

        private void OnPrev()
        {
            if (_selectedAICount > MinAICount)
            {
                _selectedAICount--;
                UpdateCountText();
            }
        }

        private void OnNext()
        {
            if (_selectedAICount < MaxAICount)
            {
                _selectedAICount++;
                UpdateCountText();
            }
        }

        private void UpdateCountText()
        {
            if (_countText != null)
                _countText.text = $"{_selectedAICount}体";
        }

        private void HandleStartButton()
        {
            SceneController.AICount = _selectedAICount;
            OnGameStart?.Invoke(_selectedAICount);
            SceneManager.LoadScene("GameScene");
        }

        private void OnDestroy()
        {
            if (_prevButton != null) _prevButton.onClick.RemoveListener(OnPrev);
            if (_nextButton != null) _nextButton.onClick.RemoveListener(OnNext);
            if (_startButton != null) _startButton.onClick.RemoveListener(HandleStartButton);
        }

        private void BuildUI()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            if (GetComponent<CanvasScaler>() == null)
            {
                var scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(960, 540);
                scaler.matchWidthOrHeight = 0.5f;
            }
            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            var rt = GetComponent<RectTransform>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // === 背景（raycast無効） ===
            var bgGo = MakeRect("Background", rt);
            Stretch(bgGo);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.08f, 0.25f);
            bgImg.raycastTarget = false;

            // === 中央パネル（VerticalLayoutGroup + ContentSizeFitter） ===
            var panelGo = MakeRect("CenterPanel", rt);
            var panelRT = panelGo.GetComponent<RectTransform>();
            // 横: 中央60%  縦: 中央寄せ（ContentSizeFitterで高さ自動）
            panelRT.anchorMin = new Vector2(0.2f, 0.5f);
            panelRT.anchorMax = new Vector2(0.8f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(0, 0); // ContentSizeFitterが上書き
            var panelLayout = panelGo.AddComponent<VerticalLayoutGroup>();
            panelLayout.childAlignment = TextAnchor.MiddleCenter;
            panelLayout.spacing = 8;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;
            panelLayout.childControlHeight = true;
            panelLayout.padding = new RectOffset(20, 20, 10, 10);
            var fitter = panelGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // === タイトル ===
            var titleGo = MakeRect("Title", panelGo.transform);
            AddLayout(titleGo, -1, 40);
            var titleText = titleGo.AddComponent<Text>();
            titleText.text = "Magical Competition";
            titleText.font = font;
            titleText.fontSize = 30;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.9f, 0.3f);
            titleText.resizeTextForBestFit = true;
            titleText.resizeTextMinSize = 14;
            titleText.resizeTextMaxSize = 30;
            titleText.raycastTarget = false;

            // スペーサー
            AddSpacer(panelGo.transform, 8);

            // === AI人数ラベル ===
            var labelGo = MakeRect("Label", panelGo.transform);
            AddLayout(labelGo, -1, 22);
            var labelText = labelGo.AddComponent<Text>();
            labelText.text = "対戦AI人数";
            labelText.font = font;
            labelText.fontSize = 18;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.raycastTarget = false;

            // === セレクター行  ◀  N体  ▶ ===
            var selectorGo = MakeRect("Selector", panelGo.transform);
            AddLayout(selectorGo, -1, 32);
            var hLayout = selectorGo.AddComponent<HorizontalLayoutGroup>();
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.spacing = 10;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = true;

            // ◀ ボタン
            _prevButton = MakeButton("PrevBtn", selectorGo.transform, "\u25C0", font, 16,
                new Color(0.3f, 0.3f, 0.6f), 34);

            // 数字表示
            var countGo = MakeRect("CountText", selectorGo.transform);
            AddLayout(countGo, -1, -1, 60);
            var countBg = countGo.AddComponent<Image>();
            countBg.color = new Color(0.2f, 0.2f, 0.5f);
            countBg.raycastTarget = false;
            _countText = MakeText("Text", countGo.transform, "1体", font, 16);
            Stretch(_countText.gameObject);
            _countText.alignment = TextAnchor.MiddleCenter;
            _countText.color = Color.white;
            _countText.raycastTarget = false;

            // ▶ ボタン
            _nextButton = MakeButton("NextBtn", selectorGo.transform, "\u25B6", font, 16,
                new Color(0.3f, 0.3f, 0.6f), 34);

            // スペーサー
            AddSpacer(panelGo.transform, 8);

            // === ゲーム開始ボタン ===
            _startButton = MakeButton("StartBtn", panelGo.transform, "ゲーム開始", font, 16,
                new Color(0.15f, 0.65f, 0.3f), -1, 34);
        }

        // --- ヘルパー ---

        private static GameObject MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
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

        private static void AddLayout(GameObject go, float flexW, float prefH, float prefW = -1)
        {
            var le = go.AddComponent<LayoutElement>();
            if (flexW >= 0) le.flexibleWidth = flexW;
            if (prefH >= 0) le.preferredHeight = prefH;
            if (prefW >= 0) le.preferredWidth = prefW;
        }

        private static void AddSpacer(Transform parent, float height)
        {
            var go = MakeRect("Spacer", parent);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.flexibleWidth = 1;
        }

        private static Text MakeText(string name, Transform parent, string content, Font font, int size)
        {
            var go = MakeRect(name, parent);
            var t = go.AddComponent<Text>();
            t.text = content;
            t.font = font;
            t.fontSize = size;
            return t;
        }

        private static Button MakeButton(string name, Transform parent, string label, Font font,
            int fontSize, Color bgColor, float prefW = -1, float prefH = -1)
        {
            var go = MakeRect(name, parent);
            var le = go.AddComponent<LayoutElement>();
            if (prefW >= 0) le.preferredWidth = prefW;
            if (prefH >= 0) le.preferredHeight = prefH;
            le.flexibleWidth = prefW < 0 ? 1 : 0;

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            // ホバー/押下時のカラー
            var colors = btn.colors;
            colors.highlightedColor = bgColor * 1.2f;
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;

            var txt = MakeText("Text", go.transform, label, font, fontSize);
            Stretch(txt.gameObject);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.raycastTarget = false;

            return btn;
        }
    }
}
