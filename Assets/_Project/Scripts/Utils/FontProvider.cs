using UnityEngine;

namespace MagicalCompetition.Utils
{
    /// <summary>
    /// プロジェクト共通フォントを提供するユーティリティ。
    /// Resources/Fonts/ から Noto Sans JP を読み込む。
    /// WebGL ビルドでも日本語が正しく表示される。
    /// </summary>
    public static class FontProvider
    {
        private static Font _regular;
        private static Font _bold;

        /// <summary>Noto Sans JP Regular を返す。</summary>
        public static Font Regular
        {
            get
            {
                if (_regular == null)
                    _regular = Resources.Load<Font>("Fonts/NotoSansJP-Regular");
                return _regular;
            }
        }

        /// <summary>Noto Sans JP Bold を返す。</summary>
        public static Font Bold
        {
            get
            {
                if (_bold == null)
                    _bold = Resources.Load<Font>("Fonts/NotoSansJP-Bold");
                return _bold;
            }
        }
    }
}
