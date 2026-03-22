using UnityEngine;
using UnityEngine.UI;
using MagicalCompetition.Utils;

namespace MagicalCompetition.Views
{
    /// <summary>
    /// プレイログを画面左下に表示するコンポーネント。
    /// 個別GameObjectエントリ方式でログを管理し、ScrollViewで自動スクロールする。
    /// GameSceneBuilderでエディタ構築済みの場合は_entryContainerを使用し、
    /// 未設定の場合はランタイムでUIを自動生成する。
    /// </summary>
    public class PlayLogView : MonoBehaviour
    {
        private const int MaxLogLines = 20;
        private const int FontSize = 12;

        [SerializeField] private Transform _entryContainer;

        private ScrollRect _scrollRect;
        private int _logCount;

        private void Awake()
        {
            // _entryContainerが未設定の場合はランタイムでUI構築
            if (_entryContainer == null)
                BuildUI();
            else
                _scrollRect = GetComponentInChildren<ScrollRect>();
        }

        /// <summary>ログ行を追加し、最下部にスクロールする。</summary>
        public void AddLog(string message)
        {
            if (_entryContainer == null) return;

            // 新規エントリ生成
            var entryGo = new GameObject($"LogEntry_{_logCount++}", typeof(RectTransform));
            entryGo.transform.SetParent(_entryContainer, false);

            var text = entryGo.AddComponent<Text>();
            text.font = FontProvider.Regular;
            text.fontSize = FontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = message;

            // ContentSizeFitterで高さを自動計算
            var fitter = entryGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 幅をコンテナに合わせる
            var rt = entryGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0f, 1f);

            // 上限超過時は古いエントリを削除（DestroyImmediateで即座に反映）
            while (_entryContainer.childCount > MaxLogLines)
                DestroyImmediate(_entryContainer.GetChild(0).gameObject);

            // 最下部にスクロール
            if (_scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                _scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        /// <summary>ログをクリアする。</summary>
        public void Clear()
        {
            if (_entryContainer == null) return;

            for (int i = _entryContainer.childCount - 1; i >= 0; i--)
                Destroy(_entryContainer.GetChild(i).gameObject);

            _logCount = 0;
        }

        private void BuildUI()
        {
            var font = FontProvider.Regular;

            var root = GetComponent<RectTransform>();
            if (root == null)
                root = gameObject.AddComponent<RectTransform>();

            // 半透明背景 #1A1040 alpha 0.75
            var bgImage = GetComponent<Image>();
            if (bgImage == null)
                bgImage = gameObject.AddComponent<Image>();
            bgImage.color = new Color(0.176f, 0.106f, 0.369f, 0.80f); // #2D1B5E — TitleScene統一
            bgImage.raycastTarget = false;

            // ScrollRect
            _scrollRect = GetComponent<ScrollRect>();
            if (_scrollRect == null)
                _scrollRect = gameObject.AddComponent<ScrollRect>();
            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _scrollRect.scrollSensitivity = 10f;

            // Viewport
            var viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(transform, false);
            var viewportRT = viewportGo.GetComponent<RectTransform>();
            viewportRT.anchorMin = Vector2.zero;
            viewportRT.anchorMax = Vector2.one;
            viewportRT.offsetMin = new Vector2(4f, 4f);
            viewportRT.offsetMax = new Vector2(-4f, -4f);
            viewportGo.AddComponent<RectMask2D>();

            // Content（エントリコンテナ）
            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRT = contentGo.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0f, 1f);
            contentRT.anchorMax = new Vector2(1f, 1f);
            contentRT.pivot = new Vector2(0f, 1f);
            contentRT.sizeDelta = new Vector2(0f, 0f);

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var vlg = contentGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2;
            vlg.childAlignment = TextAnchor.UpperLeft;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            _entryContainer = contentRT;

            // ScrollRectに接続
            _scrollRect.viewport = viewportRT;
            _scrollRect.content = contentRT;
        }
    }
}
