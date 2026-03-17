using UnityEngine;

namespace MagicalCompetition.Utils
{
    /// <summary>
    /// プロジェクト共通のUIテーマカラーとプロシージャルSprite生成ユーティリティ。
    /// </summary>
    public static class UITheme
    {
        // === 背景 ===
        public static readonly Color BgDeep       = new Color(0.08f, 0.04f, 0.16f);
        public static readonly Color BgMid        = new Color(0.14f, 0.08f, 0.26f);
        public static readonly Color BgLight      = new Color(0.18f, 0.12f, 0.32f);

        // === 金装飾 ===
        public static readonly Color Gold         = new Color(0.85f, 0.70f, 0.30f);
        public static readonly Color GoldBright   = new Color(1.0f, 0.88f, 0.45f);
        public static readonly Color GoldDim      = new Color(0.55f, 0.45f, 0.20f);

        // === ボタン ===
        public static readonly Color BtnPlay      = new Color(0.20f, 0.55f, 0.35f);
        public static readonly Color BtnPlayHover  = new Color(0.25f, 0.65f, 0.40f);
        public static readonly Color BtnPlayPress  = new Color(0.15f, 0.42f, 0.28f);

        public static readonly Color BtnPass      = new Color(0.55f, 0.25f, 0.25f);
        public static readonly Color BtnPassHover  = new Color(0.65f, 0.30f, 0.30f);
        public static readonly Color BtnPassPress  = new Color(0.42f, 0.18f, 0.18f);

        public static readonly Color BtnPurple    = new Color(0.30f, 0.18f, 0.45f);
        public static readonly Color BtnPurpleHover = new Color(0.40f, 0.25f, 0.55f);
        public static readonly Color BtnPurplePress = new Color(0.22f, 0.12f, 0.35f);

        public static readonly Color BtnDisabled  = new Color(0.30f, 0.30f, 0.35f, 0.6f);

        // === テキスト ===
        public static readonly Color TextWhite    = new Color(0.95f, 0.92f, 0.88f);
        public static readonly Color TextCream     = new Color(0.95f, 0.90f, 0.80f);

        // === パネル ===
        public static readonly Color PanelDark    = new Color(0.06f, 0.03f, 0.12f, 0.75f);
        public static readonly Color PanelMid     = new Color(0.12f, 0.08f, 0.22f, 0.85f);

        // === フィールド ===
        public static readonly Color FieldHighlight = new Color(0.85f, 0.70f, 0.30f, 0.3f);

        // === キャッシュ ===
        private static Sprite _roundedRect;
        private static Sprite _roundedRectSmall;
        private static Texture2D _gradientBgTexture;

        /// <summary>角丸矩形Spriteを生成する（radius=12, 64x64, Sliced用）。</summary>
        public static Sprite RoundedRect
        {
            get
            {
                if (_roundedRect == null)
                    _roundedRect = CreateRoundedRectSprite(64, 64, 12);
                return _roundedRect;
            }
        }

        /// <summary>小さめの角丸矩形Sprite（radius=8, 48x48）。</summary>
        public static Sprite RoundedRectSmall
        {
            get
            {
                if (_roundedRectSmall == null)
                    _roundedRectSmall = CreateRoundedRectSprite(48, 48, 8);
                return _roundedRectSmall;
            }
        }

        /// <summary>縦グラデーション背景テクスチャを返す。</summary>
        public static Texture2D GradientBgTexture
        {
            get
            {
                if (_gradientBgTexture == null)
                    _gradientBgTexture = CreateVerticalGradient(2, 256, BgDeep, BgMid, BgLight);
                return _gradientBgTexture;
            }
        }

        /// <summary>角丸矩形のSprite（Sliced対応border付き）を生成する。</summary>
        public static Sprite CreateRoundedRectSprite(int width, int height, int radius)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color32[width * height];
            var white = new Color32(255, 255, 255, 255);
            var clear = new Color32(0, 0, 0, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float alpha = GetRoundedRectAlpha(x, y, width, height, radius);
                    if (alpha >= 1f)
                        pixels[y * width + x] = white;
                    else if (alpha <= 0f)
                        pixels[y * width + x] = clear;
                    else
                        pixels[y * width + x] = new Color32(255, 255, 255, (byte)(alpha * 255));
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            int b = radius;
            var border = new Vector4(b, b, b, b);
            return Sprite.Create(tex, new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        /// <summary>縦3色グラデーションテクスチャを生成する（下→中→上）。</summary>
        public static Texture2D CreateVerticalGradient(int width, int height,
            Color bottom, Color mid, Color top)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float t = (float)y / (height - 1);
                Color c;
                if (t < 0.5f)
                    c = Color.Lerp(bottom, mid, t * 2f);
                else
                    c = Color.Lerp(mid, top, (t - 0.5f) * 2f);

                for (int x = 0; x < width; x++)
                    pixels[y * width + x] = c;
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        /// <summary>ButtonのColorBlockにテーマカラーを適用する。</summary>
        public static UnityEngine.UI.ColorBlock MakeButtonColors(
            Color normal, Color hover, Color pressed, Color disabled)
        {
            var cb = UnityEngine.UI.ColorBlock.defaultColorBlock;
            cb.normalColor = normal;
            cb.highlightedColor = hover;
            cb.pressedColor = pressed;
            cb.selectedColor = hover;
            cb.disabledColor = disabled;
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.12f;
            return cb;
        }

        /// <summary>ImageにSliced角丸Spriteをセットする。</summary>
        public static void ApplyRoundedRect(UnityEngine.UI.Image image, Color color,
            bool small = false)
        {
            image.sprite = small ? RoundedRectSmall : RoundedRect;
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.color = color;
        }

        private static float GetRoundedRectAlpha(int x, int y, int w, int h, int r)
        {
            // 角丸の中心座標を計算
            int cx, cy;

            if (x < r && y < r) { cx = r; cy = r; }                    // 左下
            else if (x >= w - r && y < r) { cx = w - r - 1; cy = r; }  // 右下
            else if (x < r && y >= h - r) { cx = r; cy = h - r - 1; }  // 左上
            else if (x >= w - r && y >= h - r) { cx = w - r - 1; cy = h - r - 1; } // 右上
            else return 1f; // 角丸外の矩形内

            float dx = x - cx;
            float dy = y - cy;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            if (dist <= r - 0.5f) return 1f;
            if (dist >= r + 0.5f) return 0f;
            return r + 0.5f - dist; // アンチエイリアス
        }
    }
}
