using UnityEngine;
using UnityEngine.UI;

namespace MagicalCompetition.Utils
{
    /// <summary>
    /// プロジェクト共通のUIテーマカラーとプロシージャルSprite生成ユーティリティ。
    /// TitleScene と統一されたダークパープル＋ゴールド装飾パレット。
    /// </summary>
    public static class UITheme
    {
        // === 背景（TitleScene: #1A0A3A） ===
        public static readonly Color BgDeep       = new Color(0.102f, 0.039f, 0.227f); // #1A0A3A
        public static readonly Color BgMid        = new Color(0.176f, 0.106f, 0.369f); // #2D1B5E
        public static readonly Color BgLight      = new Color(0.239f, 0.149f, 0.471f); // #3D2878

        // === 金装飾（TitleScene: #D4AF37） ===
        public static readonly Color Gold         = new Color(0.831f, 0.686f, 0.216f); // #D4AF37
        public static readonly Color GoldBright   = new Color(0.910f, 0.851f, 0.690f); // #E8D9B0
        public static readonly Color GoldDim      = new Color(0.55f, 0.45f, 0.20f);

        // === ボタン — TitleScene統一パープル系 ===
        // 出す: TitleScene のゲーム開始ボタン系 (#6A3DB8)
        public static readonly Color BtnPlay      = new Color(0.416f, 0.239f, 0.722f); // #6A3DB8
        public static readonly Color BtnPlayHover  = new Color(0.502f, 0.318f, 0.800f); // #8051CC
        public static readonly Color BtnPlayPress  = new Color(0.318f, 0.176f, 0.561f); // #512D8F

        // パス: ダークレッド (#8A2D2D)
        public static readonly Color BtnPass      = new Color(0.541f, 0.176f, 0.176f); // #8A2D2D
        public static readonly Color BtnPassHover  = new Color(0.620f, 0.220f, 0.220f);
        public static readonly Color BtnPassPress  = new Color(0.440f, 0.140f, 0.140f);

        // プレイ: ダークグリーン (#2D8A5E)
        public static readonly Color BtnGreen     = new Color(0.176f, 0.541f, 0.369f); // #2D8A5E
        public static readonly Color BtnGreenHover = new Color(0.220f, 0.620f, 0.420f);
        public static readonly Color BtnGreenPress = new Color(0.140f, 0.440f, 0.290f);

        // 確定: ゴールド系
        public static readonly Color BtnPurple    = new Color(0.416f, 0.239f, 0.722f); // #6A3DB8
        public static readonly Color BtnPurpleHover = new Color(0.502f, 0.318f, 0.800f);
        public static readonly Color BtnPurplePress = new Color(0.318f, 0.176f, 0.561f);

        public static readonly Color BtnDisabled  = new Color(0.30f, 0.30f, 0.35f, 0.6f);

        // === テキスト ===
        public static readonly Color TextWhite    = new Color(0.95f, 0.92f, 0.88f);
        public static readonly Color TextCream     = new Color(0.910f, 0.851f, 0.690f); // #E8D9B0
        public static readonly Color TextGold     = new Color(0.831f, 0.686f, 0.216f); // #D4AF37

        // === パネル（TitleScene: #2D1B5E CC） ===
        public static readonly Color PanelDark    = new Color(0.176f, 0.106f, 0.369f, 0.80f); // #2D1B5E CC
        public static readonly Color PanelMid     = new Color(0.239f, 0.149f, 0.471f, 0.85f); // #3D2878 D9

        // === ゴールドボーダー ===
        public static readonly Color GoldBorder   = new Color(0.831f, 0.686f, 0.216f, 0.9f); // #D4AF37

        // === フィールド ===
        public static readonly Color FieldHighlight = new Color(0.831f, 0.686f, 0.216f, 0.3f); // Gold glow

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

        /// <summary>ゴールドボーダーのOutlineを追加する（TitleScene統一デザイン）。</summary>
        public static Outline AddGoldBorder(GameObject go, float distance = 2f)
        {
            var outline = go.GetComponent<Outline>();
            if (outline == null)
                outline = go.AddComponent<Outline>();
            outline.effectColor = GoldBorder;
            outline.effectDistance = new Vector2(distance, -distance);
            return outline;
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
