using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MagicalCompetition.Controllers;
using MagicalCompetition.Utils;
using static MagicalCompetition.Utils.UITheme;

namespace MagicalCompetition.UI
{
    /// <summary>
    /// タイトル画面のUIコンポーネント。
    /// AI人数選択（◀ N体 ▶）とゲーム開始ボタンを提供する。
    /// TitleScene.md / TitleScene.png 準拠のダークパープル＋ゴールド装飾デザイン。
    /// </summary>
    public class TitleUI : MonoBehaviour
    {
        private const int MinAICount = 1;
        private const int MaxAICount = 4;

        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Text _countText;
        [SerializeField] private Button _startButton;
        [SerializeField] private Image _fadePanel;
        [SerializeField] private Sprite _bgSprite;

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
                SoundManager.Instance.PlayButtonClick();
            }
        }

        private void OnNext()
        {
            if (_selectedAICount < MaxAICount)
            {
                _selectedAICount++;
                UpdateCountText();
                SoundManager.Instance.PlayButtonClick();
            }
        }

        private void UpdateCountText()
        {
            if (_countText != null)
                _countText.text = $"{_selectedAICount}体";

            // 最小/最大時にボタンのアルファを変更
            if (_prevButton != null)
            {
                var cg = _prevButton.GetComponent<CanvasGroup>();
                if (cg == null) cg = _prevButton.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = _selectedAICount <= MinAICount ? 0.4f : 1f;
                _prevButton.interactable = _selectedAICount > MinAICount;
            }
            if (_nextButton != null)
            {
                var cg = _nextButton.GetComponent<CanvasGroup>();
                if (cg == null) cg = _nextButton.gameObject.AddComponent<CanvasGroup>();
                cg.alpha = _selectedAICount >= MaxAICount ? 0.4f : 1f;
                _nextButton.interactable = _selectedAICount < MaxAICount;
            }
        }

        private void HandleStartButton()
        {
            SoundManager.Instance.PlayButtonClick();
            SceneController.AICount = _selectedAICount;
            OnGameStart?.Invoke(_selectedAICount);

            // フェードアウト → シーン遷移
            if (_fadePanel != null)
                StartCoroutine(FadeAndLoad("GameScene"));
            else
                SceneManager.LoadScene("GameScene");
        }

        private IEnumerator FadeAndLoad(string sceneName)
        {
            _fadePanel.gameObject.SetActive(true);
            float elapsed = 0f;
            const float duration = 1f;
            var color = _fadePanel.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Clamp01(elapsed / duration);
                _fadePanel.color = color;
                yield return null;
            }

            SceneManager.LoadScene(sceneName);
        }

        private void OnDestroy()
        {
            if (_prevButton != null) _prevButton.onClick.RemoveListener(OnPrev);
            if (_nextButton != null) _nextButton.onClick.RemoveListener(OnNext);
            if (_startButton != null) _startButton.onClick.RemoveListener(HandleStartButton);
        }

        private void BuildUI()
        {
            // === TitleScene.md 準拠カラー ===
            var colorGold       = Gold;         // #D4AF37
            var colorGoldBright = GoldBright;   // #E8D9B0
            var colorGoldDim    = GoldDim;
            var colorPanel      = PanelDark;    // #2D1B5E CC
            var colorBtnArrow   = BgLight;      // #3D2878
            var colorBtnStart   = BtnPlay;      // #6A3DB8
            var colorFooter     = new Color(0.722f, 0.627f, 0.565f, 0.667f); // #B8A090 AA

            // === Canvas 設定 ===
            var canvas = GetComponent<Canvas>();
            if (canvas == null)
                canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main ?? FindAnyObjectByType<Camera>();
            if (GetComponent<CanvasScaler>() == null)
            {
                var scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(960, 540);
                scaler.matchWidthOrHeight = 0.5f;
            }
            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            // カメラ背景色を #1A0A3A に
            var cam = canvas.worldCamera;
            if (cam != null)
                cam.backgroundColor = BgDeep;

            var rt = GetComponent<RectTransform>();
            var font = FontProvider.Regular;
            var fontBold = FontProvider.Bold;

            // === 背景: start_screen.png（_bgSpriteはEditorでTitleSceneBuilderがセット） ===
            var bgGo = MakeRect("Background", rt);
            Stretch(bgGo);
            if (_bgSprite != null)
            {
                var bgImg = bgGo.AddComponent<Image>();
                bgImg.sprite = _bgSprite;
                bgImg.type = Image.Type.Simple;
                bgImg.preserveAspect = false;
                bgImg.color = Color.white;
                bgImg.raycastTarget = false;
            }
            else
            {
                // フォールバック: プロシージャルグラデーション
                var bgRaw = bgGo.AddComponent<RawImage>();
                bgRaw.texture = GradientBgTexture;
                bgRaw.color = Color.white;
                bgRaw.raycastTarget = false;
            }

            // === 上部装飾ライン（ゴールド） ===
            var topLineGo = MakeRect("TopLine", rt);
            SetAnchors(topLineGo, new Vector2(0.15f, 0.88f), new Vector2(0.85f, 0.885f));
            var topLineImg = topLineGo.AddComponent<Image>();
            topLineImg.color = colorGold;
            topLineImg.raycastTarget = false;

            // === 下部装飾ライン（ゴールド） ===
            var bottomLineGo = MakeRect("BottomLine", rt);
            SetAnchors(bottomLineGo, new Vector2(0.15f, 0.115f), new Vector2(0.85f, 0.12f));
            var bottomLineImg = bottomLineGo.AddComponent<Image>();
            bottomLineImg.color = colorGold;
            bottomLineImg.raycastTarget = false;

            // === 装飾: 四隅のコーナー（ゴールド） ===
            CreateCorner(rt, "CornerTL", 0.13f, 0.86f, colorGold, false, false);
            CreateCorner(rt, "CornerTR", 0.85f, 0.86f, colorGold, true, false);
            CreateCorner(rt, "CornerBL", 0.13f, 0.10f, colorGold, false, true);
            CreateCorner(rt, "CornerBR", 0.85f, 0.10f, colorGold, true, true);

            // === メインタイトル "Magical Competition"（ゴールド・大きめ） ===
            var titleGo = MakeRect("TitleText", rt);
            SetAnchors(titleGo, new Vector2(0.1f, 0.70f), new Vector2(0.9f, 0.86f));
            var titleText = titleGo.AddComponent<Text>();
            titleText.text = "Magical Competition";
            titleText.font = fontBold;
            titleText.fontSize = 40;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.95f, 0.75f); // 明るいクリームゴールド
            titleText.resizeTextForBestFit = true;
            titleText.resizeTextMinSize = 24;
            titleText.resizeTextMaxSize = 40;
            titleText.raycastTarget = false;

            // 黒アウトラインで背景から浮かせる
            var titleOutline = titleGo.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0.05f, 0.02f, 0.12f, 0.9f);
            titleOutline.effectDistance = new Vector2(2, -2);

            // ドロップシャドウでさらに奥行き
            var titleShadow = titleGo.AddComponent<Shadow>();
            titleShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
            titleShadow.effectDistance = new Vector2(3, -3);

            // === サブタイトル "マジカルコンペ"（薄ゴールドホワイト #E8D9B0） ===
            var subTitleGo = MakeRect("SubTitleText", rt);
            SetAnchors(subTitleGo, new Vector2(0.2f, 0.63f), new Vector2(0.8f, 0.71f));
            var subTitleText = subTitleGo.AddComponent<Text>();
            subTitleText.text = "マジカルコンペ";
            subTitleText.font = font;
            subTitleText.fontSize = 14;
            subTitleText.alignment = TextAnchor.MiddleCenter;
            subTitleText.color = new Color(0.95f, 0.92f, 0.82f); // 明るいクリーム白
            subTitleText.raycastTarget = false;

            var subTitleOutline = subTitleGo.AddComponent<Outline>();
            subTitleOutline.effectColor = new Color(0.05f, 0.02f, 0.12f, 0.7f);
            subTitleOutline.effectDistance = new Vector2(1, -1);

            // === 中央パネル（半透明パープル #2D1B5E CC ＋ゴールドボーダー） ===
            var panelGo = MakeRect("Panel_Settings", rt);
            var panelRT = panelGo.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.25f, 0.5f);
            panelRT.anchorMax = new Vector2(0.75f, 0.5f);
            panelRT.pivot = new Vector2(0.5f, 0.7f);
            panelRT.sizeDelta = new Vector2(0, 0);

            var panelBg = panelGo.AddComponent<Image>();
            ApplyRoundedRect(panelBg, colorPanel);
            panelBg.raycastTarget = false;

            // ゴールドボーダー（α=50/255）
            var panelOutline = panelGo.AddComponent<Outline>();
            panelOutline.effectColor = new Color(GoldBorder.r, GoldBorder.g, GoldBorder.b, 50f / 255f);
            panelOutline.effectDistance = new Vector2(2, -2);

            var panelLayout = panelGo.AddComponent<VerticalLayoutGroup>();
            panelLayout.childAlignment = TextAnchor.MiddleCenter;
            panelLayout.spacing = 10;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;
            panelLayout.childControlHeight = true;
            panelLayout.padding = new RectOffset(30, 30, 16, 16);
            var fitter = panelGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // === AI人数ラベル "対戦AI人数" ===
            var labelGo = MakeRect("Label_AICount", panelGo.transform);
            AddLayout(labelGo, -1, 24);
            var labelText = labelGo.AddComponent<Text>();
            labelText.text = "対戦AI人数";
            labelText.font = font;
            labelText.fontSize = 16;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = TextCream;
            labelText.raycastTarget = false;

            // === セレクター行  ◀  N体  ▶ ===
            var selectorGo = MakeRect("Selector", panelGo.transform);
            AddLayout(selectorGo, -1, 40);
            var hLayout = selectorGo.AddComponent<HorizontalLayoutGroup>();
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.spacing = 12;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = true;

            // ◀ ボタン（#3D2878 ＋ゴールドボーダー）
            _prevButton = MakeGoldButton("Btn_Decrease", selectorGo.transform, "\u25C0", font, 16,
                colorBtnArrow, 40);

            // 数字表示（ゴールドボーダー枠）
            var countGo = MakeRect("Text_AICount_Bg", selectorGo.transform);
            AddLayout(countGo, -1, -1, 80);
            var countBg = countGo.AddComponent<Image>();
            countBg.color = new Color(BgDeep.r, BgDeep.g, BgDeep.b, 0.8f);
            countBg.raycastTarget = false;
            AddGoldBorder(countGo, 1.5f);

            _countText = MakeText("Text_AICount", countGo.transform, "1体", font, 18);
            Stretch(_countText.gameObject);
            _countText.alignment = TextAnchor.MiddleCenter;
            _countText.color = colorGoldBright; // #E8D9B0
            _countText.raycastTarget = false;

            // ▶ ボタン（#3D2878 ＋ゴールドボーダー）
            _nextButton = MakeGoldButton("Btn_Increase", selectorGo.transform, "\u25B6", font, 16,
                colorBtnArrow, 40);

            // スペーサー
            AddSpacer(panelGo.transform, 6);

            // === ゲーム開始ボタン（#6A3DB8 ＋ゴールドボーダー） ===
            _startButton = MakeGoldButton("Btn_StartGame", panelGo.transform, "ゲーム開始", font, 18,
                colorBtnStart, -1, 42);

            // === フッター "— Select opponents and begin —" ===
            var footerGo = MakeRect("FooterText", rt);
            SetAnchors(footerGo, new Vector2(0.2f, 0.04f), new Vector2(0.8f, 0.10f));
            var footerText = footerGo.AddComponent<Text>();
            footerText.text = "― Select opponents and begin ―";
            footerText.font = font;
            footerText.fontSize = 11;
            footerText.fontStyle = FontStyle.Italic;
            footerText.alignment = TextAnchor.MiddleCenter;
            footerText.color = colorFooter; // #B8A090 AA
            footerText.raycastTarget = false;

            // === FadePanel（黒・最前面・初期非表示） ===
            var fadePanelGo = MakeRect("FadePanel", rt);
            Stretch(fadePanelGo);
            _fadePanel = fadePanelGo.AddComponent<Image>();
            _fadePanel.color = new Color(0f, 0f, 0f, 0f);
            _fadePanel.raycastTarget = false;
            fadePanelGo.transform.SetAsLastSibling();
            fadePanelGo.SetActive(false);
        }

        // === ヘルパー ===

        /// <summary>コーナー装飾（L字型）を生成する。</summary>
        private static void CreateCorner(Transform parent, string name, float anchorX, float anchorY,
            Color color, bool flipH, bool flipV)
        {
            float size = 0.03f;
            float thickness = 0.005f;

            var hGo = MakeRect(name + "_H", parent);
            float hMinX = flipH ? anchorX - size : anchorX;
            float hMaxX = flipH ? anchorX : anchorX + size;
            float hMinY = flipV ? anchorY : anchorY;
            float hMaxY = flipV ? anchorY + thickness : anchorY + thickness;
            SetAnchors(hGo, new Vector2(hMinX, hMinY), new Vector2(hMaxX, hMaxY));
            var hImg = hGo.AddComponent<Image>();
            hImg.color = color;
            hImg.raycastTarget = false;

            var vGo = MakeRect(name + "_V", parent);
            float vMinX = flipH ? anchorX - thickness : anchorX;
            float vMaxX = flipH ? anchorX : anchorX + thickness;
            float vMinY = flipV ? anchorY : anchorY;
            float vMaxY = flipV ? anchorY + size : anchorY + size;
            SetAnchors(vGo, new Vector2(vMinX, vMinY), new Vector2(vMaxX, vMaxY));
            var vImg = vGo.AddComponent<Image>();
            vImg.color = color;
            vImg.raycastTarget = false;
        }

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

        private static void SetAnchors(GameObject go, Vector2 anchorMin, Vector2 anchorMax)
        {
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
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

        /// <summary>ゴールドボーダー付きボタンを生成する（TitleScene統一デザイン）。</summary>
        private static Button MakeGoldButton(string name, Transform parent, string label, Font font,
            int fontSize, Color bgColor, float prefW = -1, float prefH = -1)
        {
            var go = MakeRect(name, parent);
            var le = go.AddComponent<LayoutElement>();
            if (prefW >= 0) le.preferredWidth = prefW;
            if (prefH >= 0) le.preferredHeight = prefH;
            le.flexibleWidth = prefW < 0 ? 1 : 0;

            var img = go.AddComponent<Image>();
            ApplyRoundedRect(img, Color.white, true);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.colors = MakeButtonColors(bgColor, bgColor * 1.2f, bgColor * 0.8f, BtnDisabled);

            // ゴールドボーダー（TitleScene.md 仕様: #D4AF37）
            AddGoldBorder(go, 1.5f);

            var txt = MakeText("Text", go.transform, label, font, fontSize);
            Stretch(txt.gameObject);
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = TextCream; // #E8D9B0
            txt.fontStyle = FontStyle.Bold;
            txt.raycastTarget = false;

            return btn;
        }
    }
}
