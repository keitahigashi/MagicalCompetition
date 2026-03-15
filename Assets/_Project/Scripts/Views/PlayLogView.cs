using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MagicalCompetition.Views
{
    /// <summary>
    /// プレイログを画面左側に表示するコンポーネント。
    /// ランタイムでUIを自動生成し、ScrollViewで過去のログをスクロール可能にする。
    /// </summary>
    public class PlayLogView : MonoBehaviour
    {
        private const int MaxLogLines = 50;
        private const int FontSize = 12;

        private Text _logText;
        private ScrollRect _scrollRect;
        private readonly List<string> _logLines = new List<string>();

        private void Awake()
        {
            BuildUI();
        }

        /// <summary>ログ行を追加し、最下部にスクロールする。</summary>
        public void AddLog(string message)
        {
            _logLines.Add(message);

            // 上限超過時は古い行を削除
            while (_logLines.Count > MaxLogLines)
                _logLines.RemoveAt(0);

            if (_logText != null)
            {
                _logText.text = string.Join("\n", _logLines);

                // 次フレームで最下部にスクロール
                if (_scrollRect != null)
                    Canvas.ForceUpdateCanvases();
                if (_scrollRect != null)
                    _scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        /// <summary>ログをクリアする。</summary>
        public void Clear()
        {
            _logLines.Clear();
            if (_logText != null)
                _logText.text = "";
        }

        private void BuildUI()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 自身の RectTransform 設定（左側パネル）
            var root = GetComponent<RectTransform>();
            if (root == null)
                root = gameObject.AddComponent<RectTransform>();

            // 半透明背景
            var bgImage = gameObject.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.4f);
            bgImage.raycastTarget = false;

            // ScrollRect
            _scrollRect = gameObject.AddComponent<ScrollRect>();
            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _scrollRect.scrollSensitivity = 10f;

            // Viewport（RectMask2D でクリッピング）
            var viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(transform, false);
            var viewportRT = viewportGo.GetComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = new Vector2(4f, 4f);
            viewportRT.offsetMax = new Vector2(-4f, -4f);
            viewportGo.AddComponent<RectMask2D>();

            // Content（テキストを載せるコンテナ）
            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRT = contentGo.GetComponent<RectTransform>();
            // 上端に吸着、幅は Viewport に合わせる
            contentRT.anchorMin = new Vector2(0f, 1f);
            contentRT.anchorMax = new Vector2(1f, 1f);
            contentRT.pivot = new Vector2(0f, 1f);
            // sizeDelta.x = 0（アンカーで幅確定）、sizeDelta.y は ContentSizeFitter が制御
            contentRT.sizeDelta = new Vector2(0f, 0f);

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // テキスト
            _logText = contentGo.AddComponent<Text>();
            _logText.font = font;
            _logText.fontSize = FontSize;
            _logText.color = Color.white;
            _logText.alignment = TextAnchor.UpperLeft;
            _logText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _logText.verticalOverflow = VerticalWrapMode.Overflow;
            _logText.raycastTarget = false;

            // ScrollRect に接続
            _scrollRect.viewport = viewportRT;
            _scrollRect.content = contentRT;
        }
    }
}
