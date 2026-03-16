using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using MagicalCompetition.Views;
using MagicalCompetition.UI;
using MagicalCompetition.Controllers;
using MagicalCompetition.Utils;

namespace MagicalCompetition.Editor
{
    public static class GameSceneBuilder
    {
        [MenuItem("Tools/MagicalCompetition/Build GameScene UI")]
        public static void BuildGameSceneUI()
        {
            // GameSceneを自動ロード
            var scenePath = "Assets/_Project/Scenes/GameScene.unity";
            EditorSceneManager.OpenScene(scenePath);

            // 既存のCanvas取得
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas not found in scene.");
                return;
            }

            var canvasTransform = canvas.transform;

            // CanvasScaler設定
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(960, 540);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            // 既存の子オブジェクトを削除
            for (int i = canvasTransform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(canvasTransform.GetChild(i).gameObject);
            }

            // 既存の非Canvas UIオブジェクトを削除
            DeleteIfExists("Field");
            DeleteIfExists("PlayerHand");
            DeleteIfExists("AIHand");
            DeleteIfExists("DeckArea");

            // === AI情報エリア（上部） ===
            var aiInfoArea = CreateUIObject("AIInfoArea", canvasTransform);
            SetAnchors(aiInfoArea, new Vector2(0, 0.78f), new Vector2(0.96f, 0.98f), Vector2.zero, Vector2.zero);
            var aiLayoutGroup = aiInfoArea.AddComponent<HorizontalLayoutGroup>();
            aiLayoutGroup.spacing = 8;
            aiLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            aiLayoutGroup.childForceExpandWidth = true;
            aiLayoutGroup.childForceExpandHeight = true;
            aiLayoutGroup.padding = new RectOffset(2, 2, 2, 2);

            // AI情報パネル×4
            for (int i = 1; i <= 4; i++)
            {
                var aiPanel = CreateAIInfoPanel($"AIInfo{i}", aiInfoArea.transform, i);
                if (i > 1) aiPanel.SetActive(false); // デフォルト: AI1のみ表示
            }

            // === 場札エリア（中央） ===
            var fieldArea = CreateUIObject("FieldArea", canvasTransform);
            SetAnchors(fieldArea, new Vector2(0.3f, 0.4f), new Vector2(0.7f, 0.8f), Vector2.zero, Vector2.zero);

            var fieldCardBg = CreateUIObject("FieldCardBg", fieldArea.transform);
            SetAnchors(fieldCardBg, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);
            var fieldImage = fieldCardBg.AddComponent<Image>();
            fieldImage.color = new Color(0.9f, 0.9f, 0.8f, 1f);

            // FieldView用のCardView子オブジェクト
            var fieldCardView = CreateUIObject("FieldCardView", fieldCardBg.transform);
            SetAnchors(fieldCardView, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            fieldCardView.AddComponent<Image>();
            var fieldCV = fieldCardView.AddComponent<CardView>();

            var fieldView = fieldArea.AddComponent<FieldView>();
            SetPrivateField(fieldView, "_fieldCardView", fieldCV);

            // === ターン表示（中央下） ===
            var turnArea = CreateUIObject("TurnIndicator", canvasTransform);
            SetAnchors(turnArea, new Vector2(0.3f, 0.32f), new Vector2(0.7f, 0.4f), Vector2.zero, Vector2.zero);

            var turnText = CreateTextObject("TurnText", turnArea.transform, "あなたのターン", 28);
            SetAnchors(turnText.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            turnText.alignment = TextAnchor.MiddleCenter;
            turnText.color = Color.white;

            var turnBg = turnArea.AddComponent<Image>();
            turnBg.color = new Color(0.2f, 0.2f, 0.6f, 0.8f);

            var turnView = turnArea.AddComponent<TurnIndicatorView>();
            SetPrivateField(turnView, "_turnText", turnText);

            // === プレイヤーエリア（下部） ===
            // レイアウト: [出す] [手札...] [山札] [パス]
            var playerArea = CreateUIObject("PlayerArea", canvasTransform);
            SetAnchors(playerArea, new Vector2(0, 0), new Vector2(1, 0.3f), Vector2.zero, Vector2.zero);

            // 「出す」ボタン（左端）
            var playBtn = CreateButton("PlayButton", playerArea.transform, "出す");
            SetAnchors(playBtn, new Vector2(0.01f, 0.15f), new Vector2(0.1f, 0.85f), Vector2.zero, Vector2.zero);

            // 手札コンテナ（中央左）
            var handContainer = CreateUIObject("HandContainer", playerArea.transform);
            SetAnchors(handContainer, new Vector2(0.11f, 0.05f), new Vector2(0.72f, 0.95f), Vector2.zero, Vector2.zero);
            var handLayout = handContainer.AddComponent<HorizontalLayoutGroup>();
            handLayout.spacing = 5;
            handLayout.childAlignment = TextAnchor.MiddleCenter;
            handLayout.childForceExpandWidth = false;
            handLayout.childForceExpandHeight = true;

            // CardViewプレハブ用テンプレート（非表示）
            var cardTemplate = CreateUIObject("CardViewTemplate", handContainer.transform);
            cardTemplate.AddComponent<Image>();
            cardTemplate.AddComponent<CardView>();
            var templateLayout = cardTemplate.AddComponent<LayoutElement>();
            templateLayout.preferredWidth = 60;
            templateLayout.preferredHeight = 100;
            cardTemplate.SetActive(false);

            var handView = playerArea.AddComponent<HandView>();
            SetPrivateField(handView, "_cardViewPrefab", cardTemplate);
            SetPrivateField(handView, "_cardContainer", handContainer.transform);

            // 山札表示（中央右）
            var deckArea = CreateUIObject("DeckArea", playerArea.transform);
            SetAnchors(deckArea, new Vector2(0.73f, 0.15f), new Vector2(0.88f, 0.85f), Vector2.zero, Vector2.zero);

            var deckCountText = CreateTextObject("DeckCountText", deckArea.transform, "山札:0", 16);
            SetAnchors(deckCountText.gameObject, new Vector2(0f, 0.5f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            deckCountText.alignment = TextAnchor.MiddleCenter;
            deckCountText.color = Color.white;

            var handCountText = CreateTextObject("HandCountText", deckArea.transform, "手札:0", 14);
            SetAnchors(handCountText.gameObject, new Vector2(0f, 0f), new Vector2(1f, 0.5f), Vector2.zero, Vector2.zero);
            handCountText.alignment = TextAnchor.MiddleCenter;
            handCountText.color = new Color(0.8f, 0.8f, 0.8f);

            var reachIndicator = CreateUIObject("ReachIndicator", deckArea.transform);
            SetAnchors(reachIndicator, new Vector2(0.5f, 0.8f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            var reachText = CreateTextObject("ReachText", reachIndicator.transform, "REACH!", 12);
            SetAnchors(reachText.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            reachText.color = Color.red;
            reachText.alignment = TextAnchor.MiddleCenter;
            reachIndicator.SetActive(false);

            var deckView = deckArea.AddComponent<DeckView>();
            SetPrivateField(deckView, "_countText", deckCountText);
            SetPrivateField(deckView, "_handCountText", handCountText);
            SetPrivateField(deckView, "_reachIndicator", reachIndicator);

            // 「パス」ボタン（右端）
            var passBtn = CreateButton("PassButton", playerArea.transform, "パス");
            SetAnchors(passBtn, new Vector2(0.89f, 0.15f), new Vector2(0.99f, 0.85f), Vector2.zero, Vector2.zero);

            // 確定ボタン（非表示）
            var confirmBtn = CreateButton("ConfirmButton", playerArea.transform, "確定");
            SetAnchors(confirmBtn, new Vector2(0.4f, 0.01f), new Vector2(0.6f, 0.14f), Vector2.zero, Vector2.zero);
            confirmBtn.SetActive(false);

            // カード戻しパネル（非表示）
            var returnPanel = CreateUIObject("ReturnCardPanel", playerArea.transform);
            SetAnchors(returnPanel, new Vector2(0.11f, 0.05f), new Vector2(0.72f, 0.95f), Vector2.zero, Vector2.zero);
            returnPanel.AddComponent<Image>().color = new Color(0.5f, 0.5f, 0.2f, 0.3f);
            returnPanel.SetActive(false);

            // GameUI（ActionPanelは不要になったのでplayerAreaに直接付与）
            var gameUI = playerArea.AddComponent<GameUI>();
            SetPrivateField(gameUI, "_playButton", playBtn.GetComponent<Button>());
            SetPrivateField(gameUI, "_passButton", passBtn.GetComponent<Button>());
            SetPrivateField(gameUI, "_confirmButton", confirmBtn.GetComponent<Button>());
            SetPrivateField(gameUI, "_returnCardPanel", returnPanel);

            // === 結果ダイアログ（オーバーレイ） ===
            var resultOverlay = CreateUIObject("ResultDialog", canvasTransform);
            SetAnchors(resultOverlay, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var resultBg = resultOverlay.AddComponent<Image>();
            resultBg.color = new Color(0, 0, 0, 0.6f);

            var dialogPanel = CreateUIObject("DialogPanel", resultOverlay.transform);
            SetAnchors(dialogPanel, new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.8f), Vector2.zero, Vector2.zero);
            dialogPanel.AddComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f, 1f);

            var winnerText = CreateTextObject("WinnerText", dialogPanel.transform, "勝者", 36);
            SetAnchors(winnerText.gameObject, new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.9f), Vector2.zero, Vector2.zero);
            winnerText.alignment = TextAnchor.MiddleCenter;
            winnerText.color = Color.black;

            var scoreText = CreateTextObject("ScoreText", dialogPanel.transform, "スコア: 0点", 28);
            SetAnchors(scoreText.gameObject, new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.6f), Vector2.zero, Vector2.zero);
            scoreText.alignment = TextAnchor.MiddleCenter;
            scoreText.color = Color.black;

            var titleBtn = CreateButton("TitleButton", dialogPanel.transform, "タイトルに戻る");
            var titleBtnRT = titleBtn.GetComponent<RectTransform>();
            SetAnchors(titleBtn, new Vector2(0.3f, 0.05f), new Vector2(0.7f, 0.25f), Vector2.zero, Vector2.zero);

            var resultView = resultOverlay.AddComponent<ResultDialogView>();
            SetPrivateField(resultView, "_winnerText", winnerText);
            SetPrivateField(resultView, "_scoreText", scoreText);
            SetPrivateField(resultView, "_dialogPanel", resultOverlay);
            SetPrivateField(resultView, "_titleButton", titleBtn.GetComponent<Button>());

            resultOverlay.SetActive(false);

            // Camera
            var cam = Object.FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera", typeof(Camera), typeof(UnityEngine.Rendering.Universal.UniversalAdditionalCameraData));
                camGo.tag = "MainCamera";
                cam = camGo.GetComponent<Camera>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.1f, 0.2f, 1f);
            cam.orthographic = true;

            // EventSystem
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            }

            // GameSceneManager（ゲームフロー制御）
            if (Object.FindFirstObjectByType<GameSceneManager>() == null)
            {
                var managerGo = new GameObject("GameSceneManager");
                managerGo.AddComponent<GameSceneManager>();
            }

            // シーンを保存
            EditorUtility.SetDirty(canvas.gameObject);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("GameScene UI built successfully!");
        }

        private static GameObject CreateAIInfoPanel(string name, Transform parent, int aiIndex)
        {
            var panel = CreateUIObject(name, parent);
            var layoutElem = panel.AddComponent<LayoutElement>();
            layoutElem.flexibleWidth = 1;
            layoutElem.flexibleHeight = 1;

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);

            // LayoutGroup は使わず、アンカー比率で3行に分割
            // 上段 80%~100%: AI名 + 山札枚数 + アイコン
            // 中段 15%~80%:  手札カード裏面
            // 下段  0%~15%:  手札枚数

            // === 上段: AI名 + 山札 + リーチ + 思考中 ===
            var aiNameText = CreateTextObject("AINameText", panel.transform, $"AI{aiIndex}", 11);
            SetAnchors(aiNameText.gameObject, new Vector2(0.02f, 0.8f), new Vector2(0.28f, 1f), Vector2.zero, Vector2.zero);
            aiNameText.color = Color.white;
            aiNameText.alignment = TextAnchor.MiddleCenter;
            aiNameText.raycastTarget = false;

            var deckCountText = CreateTextObject("DeckCountText", panel.transform, "山札:6", 9);
            SetAnchors(deckCountText.gameObject, new Vector2(0.28f, 0.8f), new Vector2(0.55f, 1f), Vector2.zero, Vector2.zero);
            deckCountText.color = new Color(0.7f, 0.7f, 0.7f);
            deckCountText.alignment = TextAnchor.MiddleLeft;
            deckCountText.raycastTarget = false;

            var reachIcon = CreateUIObject("ReachIcon", panel.transform);
            SetAnchors(reachIcon, new Vector2(0.55f, 0.8f), new Vector2(0.7f, 1f), Vector2.zero, Vector2.zero);
            var reachText = CreateTextObject("ReachText", reachIcon.transform, "\u26A1", 11);
            SetAnchors(reachText.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            reachText.alignment = TextAnchor.MiddleCenter;
            reachText.color = Color.yellow;
            reachText.raycastTarget = false;
            reachIcon.SetActive(false);

            var thinkingIcon = CreateUIObject("ThinkingIcon", panel.transform);
            SetAnchors(thinkingIcon, new Vector2(0.7f, 0.8f), new Vector2(0.98f, 1f), Vector2.zero, Vector2.zero);
            var thinkText = CreateTextObject("ThinkingText", thinkingIcon.transform, "思考中...", 8);
            SetAnchors(thinkText.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            thinkText.alignment = TextAnchor.MiddleCenter;
            thinkText.color = Color.cyan;
            thinkText.raycastTarget = false;
            thinkingIcon.SetActive(false);

            // === 中段: 手札カード裏面 ===
            var handContainer = CreateUIObject("HandContainer", panel.transform);
            SetAnchors(handContainer, new Vector2(0.05f, 0.2f), new Vector2(0.95f, 0.78f), Vector2.zero, Vector2.zero);
            var handLayout = handContainer.AddComponent<HorizontalLayoutGroup>();
            handLayout.spacing = -2;
            handLayout.childAlignment = TextAnchor.MiddleCenter;
            handLayout.childForceExpandWidth = false;
            handLayout.childForceExpandHeight = true;
            handLayout.childControlHeight = true;
            handLayout.childControlWidth = false;
            handLayout.padding = new RectOffset(2, 2, 0, 0);

            // === 下段: 手札枚数（中央） ===
            var handCountText = CreateTextObject("HandCountText", panel.transform, "3枚", 9);
            SetAnchors(handCountText.gameObject, new Vector2(0.02f, 0f), new Vector2(0.98f, 0.15f), Vector2.zero, Vector2.zero);
            handCountText.color = new Color(0.8f, 0.8f, 0.8f);
            handCountText.alignment = TextAnchor.MiddleCenter;
            handCountText.raycastTarget = false;

            var aiInfoView = panel.AddComponent<AIInfoView>();
            SetPrivateField(aiInfoView, "_aiNameText", aiNameText);
            SetPrivateField(aiInfoView, "_handCountText", handCountText);
            SetPrivateField(aiInfoView, "_deckCountText", deckCountText);
            SetPrivateField(aiInfoView, "_reachIcon", reachIcon);
            SetPrivateField(aiInfoView, "_thinkingIcon", thinkingIcon);
            SetPrivateField(aiInfoView, "_handContainer", handContainer.transform);

            return panel;
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
            textComp.font = FontProvider.Regular;
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
            else
                Debug.LogWarning($"Field '{fieldName}' not found on {target.GetType().Name}");
        }

        private static void DeleteIfExists(string name)
        {
            var go = GameObject.Find(name);
            if (go != null)
                Object.DestroyImmediate(go);
        }
    }
}
