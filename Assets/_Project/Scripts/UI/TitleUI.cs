using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MagicalCompetition.Controllers;
using MagicalCompetition.Utils;

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
            // === テーマカラー定義 ===
            var colorBgDeep     = new Color(0.12f, 0.06f, 0.20f);       // 深紫の背景
            var colorBgMid      = new Color(0.18f, 0.10f, 0.30f);       // パネル背景
            var colorGold       = new Color(0.85f, 0.70f, 0.30f);       // 金文字
            var colorGoldBright = new Color(1.0f, 0.88f, 0.45f);        // 明るい金
            var colorGoldDim    = new Color(0.55f, 0.45f, 0.20f);       // 暗い金（装飾線）
            var colorPurpleBtn  = new Color(0.30f, 0.18f, 0.45f);       // 紫ボタン
            var colorPurpleLight= new Color(0.40f, 0.25f, 0.55f);       // 紫ボタン(明)
            var colorCream      = new Color(0.95f, 0.90f, 0.80f);       // クリーム白

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
            var font = FontProvider.Regular;

            // === 背景（深紫グラデーション風） ===
            var bgGo = MakeRect("Background", rt);
            Stretch(bgGo);
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = colorBgDeep;
            bgImg.raycastTarget = false;

            // === 上部装飾ライン ===
            var topLineGo = MakeRect("TopLine", rt);
            var topLineRT = topLineGo.GetComponent<RectTransform>();
            topLineRT.anchorMin = new Vector2(0.15f, 0.88f);
            topLineRT.anchorMax = new Vector2(0.85f, 0.885f);
            topLineRT.offsetMin = Vector2.zero;
            topLineRT.offsetMax = Vector2.zero;
            var topLineImg = topLineGo.AddComponent<Image>();
            topLineImg.color = colorGoldDim;
            topLineImg.raycastTarget = false;

            // === 下部装飾ライン ===
            var bottomLineGo = MakeRect("BottomLine", rt);
            var bottomLineRT = bottomLineGo.GetComponent<RectTransform>();
            bottomLineRT.anchorMin = new Vector2(0.15f, 0.115f);
            bottomLineRT.anchorMax = new Vector2(0.85f, 0.12f);
            bottomLineRT.offsetMin = Vector2.zero;
            bottomLineRT.offsetMax = Vector2.zero;
            var bottomLineImg = bottomLineGo.AddComponent<Image>();
            bottomLineImg.color = colorGoldDim;
            bottomLineImg.raycastTarget = false;

            // === 装飾: 四隅のコーナー ===
            CreateCorner(rt, "CornerTL", 0.13f, 0.86f, colorGoldDim, false, false);
            CreateCorner(rt, "CornerTR", 0.85f, 0.86f, colorGoldDim, true, false);
            CreateCorner(rt, "CornerBL", 0.13f, 0.10f, colorGoldDim, false, true);
            CreateCorner(rt, "CornerBR", 0.85f, 0.10f, colorGoldDim, true, true);

            // === メインタイトル ===
            var titleGo = MakeRect("Title", rt);
            var titleRT = titleGo.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.1f, 0.72f);
            titleRT.anchorMax = new Vector2(0.9f, 0.86f);
            titleRT.offsetMin = Vector2.zero;
            titleRT.offsetMax = Vector2.zero;
            var titleText = titleGo.AddComponent<Text>();
            titleText.text = "Magical Competition";
            titleText.font = FontProvider.Bold;
            titleText.fontSize = 36;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = colorGold;
            titleText.resizeTextForBestFit = true;
            titleText.resizeTextMinSize = 20;
            titleText.resizeTextMaxSize = 36;
            titleText.raycastTarget = false;

            // === 日本語タイトル ===
            var jpTitleGo = MakeRect("JpTitle", rt);
            var jpTitleRT = jpTitleGo.GetComponent<RectTransform>();
            jpTitleRT.anchorMin = new Vector2(0.2f, 0.64f);
            jpTitleRT.anchorMax = new Vector2(0.8f, 0.72f);
            jpTitleRT.offsetMin = Vector2.zero;
            jpTitleRT.offsetMax = Vector2.zero;
            var jpTitleText = jpTitleGo.AddComponent<Text>();
            jpTitleText.text = "マジカルコンペ";
            jpTitleText.font = font;
            jpTitleText.fontSize = 18;
            jpTitleText.alignment = TextAnchor.MiddleCenter;
            jpTitleText.color = colorGoldBright;
            jpTitleText.raycastTarget = false;

            // === 中央パネル（操作エリア） ===
            var panelGo = MakeRect("CenterPanel", rt);
            var panelRT = panelGo.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.25f, 0.5f);
            panelRT.anchorMax = new Vector2(0.75f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.7f);
            panelRT.sizeDelta = new Vector2(0, 0);

            // パネル背景
            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(colorBgMid.r, colorBgMid.g, colorBgMid.b, 0.6f);
            panelBg.raycastTarget = false;

            var panelLayout = panelGo.AddComponent<VerticalLayoutGroup>();
            panelLayout.childAlignment = TextAnchor.MiddleCenter;
            panelLayout.spacing = 10;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;
            panelLayout.childControlHeight = true;
            panelLayout.padding = new RectOffset(30, 30, 16, 16);
            var fitter = panelGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // === AI人数ラベル ===
            var labelGo = MakeRect("Label", panelGo.transform);
            AddLayout(labelGo, -1, 24);
            var labelText = labelGo.AddComponent<Text>();
            labelText.text = "対戦AI人数";
            labelText.font = font;
            labelText.fontSize = 16;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = colorCream;
            labelText.raycastTarget = false;

            // === セレクター行  ◀  N体  ▶ ===
            var selectorGo = MakeRect("Selector", panelGo.transform);
            AddLayout(selectorGo, -1, 36);
            var hLayout = selectorGo.AddComponent<HorizontalLayoutGroup>();
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.spacing = 12;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = true;

            // ◀ ボタン
            _prevButton = MakeButton("PrevBtn", selectorGo.transform, "\u25C0", font, 16,
                colorPurpleBtn, 36);

            // 数字表示
            var countGo = MakeRect("CountText", selectorGo.transform);
            AddLayout(countGo, -1, -1, 70);
            var countBg = countGo.AddComponent<Image>();
            countBg.color = new Color(0.10f, 0.05f, 0.18f);
            countBg.raycastTarget = false;

            // 数字表示の枠線
            var countBorderGo = MakeRect("CountBorder", countGo.transform);
            Stretch(countBorderGo);
            var countBorder = countBorderGo.AddComponent<Outline>();
            var countBorderImg = countBorderGo.AddComponent<Image>();
            countBorderImg.color = Color.clear;
            countBorderImg.raycastTarget = false;
            countBorder.effectColor = colorGoldDim;
            countBorder.effectDistance = new Vector2(1, 1);

            _countText = MakeText("Text", countGo.transform, "1体", font, 18);
            Stretch(_countText.gameObject);
            _countText.alignment = TextAnchor.MiddleCenter;
            _countText.color = colorGoldBright;
            _countText.raycastTarget = false;

            // ▶ ボタン
            _nextButton = MakeButton("NextBtn", selectorGo.transform, "\u25B6", font, 16,
                colorPurpleBtn, 36);

            // スペーサー
            AddSpacer(panelGo.transform, 6);

            // === ゲーム開始ボタン ===
            _startButton = MakeButton("StartBtn", panelGo.transform, "ゲーム開始", font, 16,
                colorPurpleLight, -1, 38);

            // === フッター ===
            var footerGo = MakeRect("Footer", rt);
            var footerRT = footerGo.GetComponent<RectTransform>();
            footerRT.anchorMin = new Vector2(0.2f, 0.04f);
            footerRT.anchorMax = new Vector2(0.8f, 0.10f);
            footerRT.offsetMin = Vector2.zero;
            footerRT.offsetMax = Vector2.zero;
            var footerText = footerGo.AddComponent<Text>();
            footerText.text = "― Select opponents and begin ―";
            footerText.font = font;
            footerText.fontSize = 11;
            footerText.fontStyle = FontStyle.Italic;
            footerText.alignment = TextAnchor.MiddleCenter;
            footerText.color = new Color(colorGoldDim.r, colorGoldDim.g, colorGoldDim.b, 0.6f);
            footerText.raycastTarget = false;
        }

        /// <summary>コーナー装飾（L字型）を生成する。</summary>
        private static void CreateCorner(Transform parent, string name, float anchorX, float anchorY,
            Color color, bool flipH, bool flipV)
        {
            float size = 0.03f;
            float thickness = 0.005f;

            // 水平線
            var hGo = MakeRect(name + "_H", parent);
            var hRT = hGo.GetComponent<RectTransform>();
            float hMinX = flipH ? anchorX - size : anchorX;
            float hMaxX = flipH ? anchorX : anchorX + size;
            float hMinY = flipV ? anchorY : anchorY;
            float hMaxY = flipV ? anchorY + thickness : anchorY + thickness;
            hRT.anchorMin = new Vector2(hMinX, hMinY);
            hRT.anchorMax = new Vector2(hMaxX, hMaxY);
            hRT.offsetMin = Vector2.zero;
            hRT.offsetMax = Vector2.zero;
            var hImg = hGo.AddComponent<Image>();
            hImg.color = color;
            hImg.raycastTarget = false;

            // 垂直線
            var vGo = MakeRect(name + "_V", parent);
            var vRT = vGo.GetComponent<RectTransform>();
            float vMinX = flipH ? anchorX - thickness : anchorX;
            float vMaxX = flipH ? anchorX : anchorX + thickness;
            float vMinY = flipV ? anchorY : anchorY;
            float vMaxY = flipV ? anchorY + size : anchorY + size;
            vRT.anchorMin = new Vector2(vMinX, vMinY);
            vRT.anchorMax = new Vector2(vMaxX, vMaxY);
            vRT.offsetMin = Vector2.zero;
            vRT.offsetMax = Vector2.zero;
            var vImg = vGo.AddComponent<Image>();
            vImg.color = color;
            vImg.raycastTarget = false;
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
