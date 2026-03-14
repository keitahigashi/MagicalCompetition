using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using MagicalCompetition.UI;
namespace MagicalCompetition.Editor
{
    public static class TitleSceneBuilder
    {
        [MenuItem("Tools/MagicalCompetition/Build TitleScene UI")]
        public static void BuildTitleSceneUI()
        {
            // TitleSceneを開く
            var scenePath = "Assets/_Project/Scenes/TitleScene.unity";
            var scene = EditorSceneManager.OpenScene(scenePath);

            // Canvas取得 or 作成
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var canvasTransform = canvas.transform;

            // 既存の子を削除
            for (int i = canvasTransform.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(canvasTransform.GetChild(i).gameObject);

            // === 背景 ===
            var bg = CreateUIObject("Background", canvasTransform);
            SetAnchors(bg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.3f, 1f);

            // === タイトルテキスト ===
            var titleText = CreateTextObject("TitleText", canvasTransform, "Magical Competition", 64);
            SetAnchors(titleText.gameObject, new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.9f), Vector2.zero, Vector2.zero);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = new Color(1f, 0.9f, 0.3f, 1f);

            // === AI人数選択エリア ===
            var selectArea = CreateUIObject("AISelectArea", canvasTransform);
            SetAnchors(selectArea, new Vector2(0.2f, 0.35f), new Vector2(0.8f, 0.55f), Vector2.zero, Vector2.zero);

            var selectLabel = CreateTextObject("SelectLabel", selectArea.transform, "対戦AI人数を選択", 28);
            SetAnchors(selectLabel.gameObject, new Vector2(0f, 0.5f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            selectLabel.alignment = TextAnchor.MiddleCenter;
            selectLabel.color = Color.white;

            var btnContainer = CreateUIObject("ButtonContainer", selectArea.transform);
            SetAnchors(btnContainer, new Vector2(0.1f, 0f), new Vector2(0.9f, 0.5f), Vector2.zero, Vector2.zero);
            var layout = btnContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            var aiCountButtons = new Button[4];
            for (int i = 0; i < 4; i++)
            {
                var btn = CreateButton($"AICount{i + 1}Btn", btnContainer.transform, $"{i + 1}体");
                aiCountButtons[i] = btn.GetComponent<Button>();
            }

            // === 選択表示テキスト ===
            var selectedText = CreateTextObject("SelectedCountText", canvasTransform, "AI 1体", 24);
            SetAnchors(selectedText.gameObject, new Vector2(0.3f, 0.25f), new Vector2(0.7f, 0.35f), Vector2.zero, Vector2.zero);
            selectedText.alignment = TextAnchor.MiddleCenter;
            selectedText.color = Color.white;

            // === 開始ボタン ===
            var startBtn = CreateButton("StartButton", canvasTransform, "ゲーム開始");
            SetAnchors(startBtn, new Vector2(0.35f, 0.1f), new Vector2(0.65f, 0.22f), Vector2.zero, Vector2.zero);
            var startImage = startBtn.GetComponent<Image>();
            startImage.color = new Color(0.2f, 0.7f, 0.3f, 1f);

            // === TitleUI コンポーネント ===
            var titleUI = canvasTransform.gameObject.AddComponent<TitleUI>();
            SetPrivateField(titleUI, "_aiCountButtons", aiCountButtons);
            SetPrivateField(titleUI, "_startButton", startBtn.GetComponent<Button>());
            SetPrivateField(titleUI, "_selectedCountText", selectedText);

            // EventSystem
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem));
            }

            EditorUtility.SetDirty(canvas.gameObject);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("TitleScene UI built successfully!");
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Text CreateTextObject(string name, Transform parent, string text, int fontSize)
        {
            var go = CreateUIObject(name, parent);
            var textComp = go.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return textComp;
        }

        private static GameObject CreateButton(string name, Transform parent, string label)
        {
            var go = CreateUIObject(name, parent);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.3f, 0.5f, 0.8f, 1f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            var textObj = CreateTextObject("Text", go.transform, label, 22);
            SetAnchors(textObj.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            textObj.alignment = TextAnchor.MiddleCenter;
            textObj.color = Color.white;

            return go;
        }

        private static void SetAnchors(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(target, value);
        }
    }
}
