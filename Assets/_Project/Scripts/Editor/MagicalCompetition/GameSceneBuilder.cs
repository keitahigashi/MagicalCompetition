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
        [MenuItem("MagicalCompetition/Build GameScene UI")]
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

            // === BG_Image（最背面） ===
            BuildBGImage(canvasTransform);

            // === AIArea_Root（上部 AI×4） ===
            BuildAIArea(canvasTransform);

            // === CenterCard_Root（中央カード＋ターン表示統合） ===
            BuildCenterCardArea(canvasTransform);

            // === PlayerHand_Root（下部手札） ===
            BuildPlayerHandArea(canvasTransform);

            // === LogPanel（左下ログ） ===
            BuildLogPanel(canvasTransform);

            // === ActionPanel（右下ボタン＋カウント） ===
            BuildActionPanel(canvasTransform);

            // === ResultDialog（オーバーレイ） ===
            BuildResultDialog(canvasTransform);

            // Camera
            var cam = Object.FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera", typeof(Camera), typeof(UnityEngine.Rendering.Universal.UniversalAdditionalCameraData));
                camGo.tag = "MainCamera";
                cam = camGo.GetComponent<Camera>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.102f, 0.039f, 0.227f, 1f); // #1A0A3A
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

        // ============================================================
        // BG_Image
        // ============================================================
        private static void BuildBGImage(Transform canvasTransform)
        {
            var bgGo = CreateUIObject("BG_Image", canvasTransform);
            SetAnchors(bgGo, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var bgImage = bgGo.AddComponent<Image>();
            bgImage.raycastTarget = false;

            // battle_arena.png をAssetDatabase経由でロード
            // TextureImporterの設定をSprite(2D)に変更してからロード
            var bgAssetPath = "Assets/_Project/Art/bg/battle_arena.png";
            var importer = AssetImporter.GetAtPath(bgAssetPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgAssetPath);
            if (sprite != null)
            {
                bgImage.sprite = sprite;
                bgImage.type = Image.Type.Simple;
                bgImage.preserveAspect = false;
                bgImage.color = Color.white;
            }
            else
            {
                Debug.LogWarning("battle_arena.png not found or not importable as Sprite. Using fallback color.");
                bgImage.color = new Color(0.102f, 0.039f, 0.227f); // #1A0A3A
            }

            bgGo.transform.SetAsFirstSibling();
        }

        // ============================================================
        // AIArea_Root（上部 AI×4）
        // ============================================================
        private static void BuildAIArea(Transform canvasTransform)
        {
            var aiArea = CreateUIObject("AIArea_Root", canvasTransform);
            SetAnchors(aiArea, new Vector2(0.02f, 0.78f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero);
            var aiLayoutGroup = aiArea.AddComponent<HorizontalLayoutGroup>();
            aiLayoutGroup.spacing = 40;
            aiLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            aiLayoutGroup.childForceExpandWidth = true;
            aiLayoutGroup.childForceExpandHeight = true;
            aiLayoutGroup.padding = new RectOffset(10, 10, 5, 5);

            // AI情報パネル×4
            for (int i = 1; i <= 4; i++)
            {
                var aiPanel = CreateAIInfoPanel($"AIInfo{i}", aiArea.transform, i);
                if (i > 1) aiPanel.SetActive(false);
            }
        }

        // ============================================================
        // CenterCard_Root（中央カード＋ターン表示統合）
        // ============================================================
        private static void BuildCenterCardArea(Transform canvasTransform)
        {
            var centerRoot = CreateUIObject("CenterCard_Root", canvasTransform);
            SetAnchors(centerRoot, new Vector2(0.3f, 0.32f), new Vector2(0.7f, 0.80f), Vector2.zero, Vector2.zero);

            // GlowEffect（パルスアニメーション用）
            var glowGo = CreateUIObject("GlowEffect", centerRoot.transform);
            SetAnchors(glowGo, new Vector2(-0.05f, -0.05f), new Vector2(1.05f, 1.05f), Vector2.zero, Vector2.zero);
            var glowImage = glowGo.AddComponent<Image>();
            glowImage.color = new Color(0.831f, 0.686f, 0.216f, 0.4f); // #D4AF37 Gold glow
            glowImage.raycastTarget = false;
            var glowCanvasGroup = glowGo.AddComponent<CanvasGroup>();
            glowCanvasGroup.alpha = 0.4f;

            // CardFrame（場札表示エリア）
            var cardFrame = CreateUIObject("FieldCardBg", centerRoot.transform);
            SetAnchors(cardFrame, new Vector2(0.2f, 0.15f), new Vector2(0.8f, 0.85f), Vector2.zero, Vector2.zero);
            var fieldImage = cardFrame.AddComponent<Image>();
            fieldImage.color = new Color(0.176f, 0.106f, 0.369f, 0.85f); // #2D1B5E

            // CostBadge（左上コスト数値）
            var costBadge = CreateTextObject("CostBadge", cardFrame.transform, "", 18);
            SetAnchors(costBadge.gameObject, new Vector2(0f, 0.85f), new Vector2(0.25f, 1f), Vector2.zero, Vector2.zero);
            costBadge.alignment = TextAnchor.MiddleCenter;
            costBadge.color = new Color(0.831f, 0.686f, 0.216f); // #D4AF37 Gold
            costBadge.fontStyle = FontStyle.Bold;
            costBadge.raycastTarget = false;

            // FieldCardView（カード画像）
            var fieldCardView = CreateUIObject("FieldCardView", cardFrame.transform);
            SetAnchors(fieldCardView, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            fieldCardView.AddComponent<Image>();
            var fieldCV = fieldCardView.AddComponent<CardView>();

            // CardName（下部中央カード名）
            var cardName = CreateTextObject("CardName", cardFrame.transform, "", 14);
            SetAnchors(cardName.gameObject, new Vector2(0f, 0f), new Vector2(1f, 0.15f), Vector2.zero, Vector2.zero);
            cardName.alignment = TextAnchor.MiddleCenter;
            cardName.color = Color.white;
            cardName.raycastTarget = false;

            // FieldView コンポーネント
            var fieldView = centerRoot.AddComponent<FieldView>();
            SetPrivateField(fieldView, "_fieldCardView", fieldCV);
            SetPrivateField(fieldView, "_glowEffect", glowGo);
            SetPrivateField(fieldView, "_costBadgeText", costBadge);
            SetPrivateField(fieldView, "_cardNameText", cardName);
            SetPrivateField(fieldView, "_glowCanvasGroup", glowCanvasGroup);

            // TurnLabel（ターン表示）
            var turnLabel = CreateUIObject("TurnLabel", centerRoot.transform);
            SetAnchors(turnLabel, new Vector2(0.1f, 0f), new Vector2(0.9f, 0.12f), Vector2.zero, Vector2.zero);

            var turnText = CreateTextObject("TurnText", turnLabel.transform, "あなたのターン", 28);
            SetAnchors(turnText.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            turnText.alignment = TextAnchor.MiddleCenter;
            turnText.color = Color.white;

            var turnBg = turnLabel.AddComponent<Image>();
            turnBg.color = new Color(0.176f, 0.106f, 0.369f, 0.85f); // #2D1B5E

            var turnView = turnLabel.AddComponent<TurnIndicatorView>();
            SetPrivateField(turnView, "_turnText", turnText);
        }

        // ============================================================
        // PlayerHand_Root（下部手札）
        // ============================================================
        private static void BuildPlayerHandArea(Transform canvasTransform)
        {
            var handRoot = CreateUIObject("PlayerHand_Root", canvasTransform);
            SetAnchors(handRoot, new Vector2(0.15f, 0.01f), new Vector2(0.72f, 0.30f), Vector2.zero, Vector2.zero);

            // 手札コンテナ
            var handContainer = CreateUIObject("HandContainer", handRoot.transform);
            SetAnchors(handContainer, new Vector2(0.02f, 0.05f), new Vector2(0.98f, 0.95f), Vector2.zero, Vector2.zero);
            var handLayout = handContainer.AddComponent<HorizontalLayoutGroup>();
            handLayout.spacing = 12;
            handLayout.childAlignment = TextAnchor.MiddleCenter;
            handLayout.childForceExpandWidth = false;
            handLayout.childForceExpandHeight = true;

            // CardViewテンプレート（非表示）— SelectHighlight子オブジェクト付き
            var cardTemplate = CreateUIObject("CardViewTemplate", handContainer.transform);
            cardTemplate.AddComponent<Image>();
            var templateCV = cardTemplate.AddComponent<CardView>();
            var templateLayout = cardTemplate.AddComponent<LayoutElement>();
            templateLayout.preferredWidth = 60;
            templateLayout.preferredHeight = 100;

            // SelectHighlight（黄色ボーダー、デフォルト非表示）
            var selectHighlight = CreateUIObject("SelectHighlight", cardTemplate.transform);
            SetAnchors(selectHighlight, new Vector2(-0.05f, -0.03f), new Vector2(1.05f, 1.03f), Vector2.zero, Vector2.zero);
            var highlightImage = selectHighlight.AddComponent<Image>();
            highlightImage.color = new Color(0.831f, 0.686f, 0.216f, 0.6f); // #D4AF37 Gold
            highlightImage.raycastTarget = false;
            // ゴールドボーダーで枠感（TitleScene統一）
            var highlightOutline = selectHighlight.AddComponent<Outline>();
            highlightOutline.effectColor = new Color(0.831f, 0.686f, 0.216f, 1f); // #D4AF37
            highlightOutline.effectDistance = new Vector2(2, -2);
            selectHighlight.SetActive(false);

            SetPrivateField(templateCV, "_selectHighlight", selectHighlight);
            cardTemplate.SetActive(false);

            var handView = handRoot.AddComponent<HandView>();
            SetPrivateField(handView, "_cardViewPrefab", cardTemplate);
            SetPrivateField(handView, "_cardContainer", handContainer.transform);
        }

        // ============================================================
        // LogPanel（左下ログ）
        // ============================================================
        private static void BuildLogPanel(Transform canvasTransform)
        {
            var logPanel = CreateUIObject("LogPanel", canvasTransform);
            // 左下: 手札エリアと同じ高さまで、幅は左側14%
            SetAnchors(logPanel, new Vector2(0f, 0.01f), new Vector2(0.14f, 0.30f), Vector2.zero, Vector2.zero);

            // 背景 #2D1B5E alpha 0.80 — TitleScene統一パネル色
            var bgImage = logPanel.AddComponent<Image>();
            bgImage.color = new Color(0.176f, 0.106f, 0.369f, 0.80f);
            bgImage.raycastTarget = false;

            // ScrollRect
            var scrollRect = logPanel.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 10f;

            // Viewport
            var viewportGo = CreateUIObject("Viewport", logPanel.transform);
            SetAnchors(viewportGo, Vector2.zero, Vector2.one, new Vector2(4, 4), new Vector2(-4, -4));
            viewportGo.AddComponent<RectMask2D>();

            // Content（VerticalLayoutGroup付きエントリコンテナ）
            var contentGo = CreateUIObject("Content", viewportGo.transform);
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

            // ScrollRectに接続
            scrollRect.viewport = viewportGo.GetComponent<RectTransform>();
            scrollRect.content = contentRT;

            // PlayLogView コンポーネント
            var playLogView = logPanel.AddComponent<PlayLogView>();
            SetPrivateField(playLogView, "_entryContainer", contentGo.transform);
        }

        // ============================================================
        // ActionPanel（右下ボタン＋カウント）
        // ============================================================
        private static void BuildActionPanel(Transform canvasTransform)
        {
            var actionPanel = CreateUIObject("ActionPanel", canvasTransform);
            SetAnchors(actionPanel, new Vector2(0.74f, 0.01f), new Vector2(0.99f, 0.30f), Vector2.zero, Vector2.zero);

            // DeckInfoRow（上部に山札・手札表示）
            var deckArea = CreateUIObject("DeckInfoRow", actionPanel.transform);
            SetAnchors(deckArea, new Vector2(0.05f, 0.70f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);

            var deckCountText = CreateTextObject("DeckCountText", deckArea.transform, "山札: 0枚", 14);
            SetAnchors(deckCountText.gameObject, new Vector2(0f, 0.5f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            deckCountText.alignment = TextAnchor.MiddleCenter;
            deckCountText.color = Color.white;

            var handCountText = CreateTextObject("HandCountText", deckArea.transform, "手札: 0枚", 14);
            SetAnchors(handCountText.gameObject, new Vector2(0f, 0f), new Vector2(1f, 0.5f), Vector2.zero, Vector2.zero);
            handCountText.alignment = TextAnchor.MiddleCenter;
            handCountText.color = new Color(0.8f, 0.8f, 0.8f);

            var reachIndicator = CreateUIObject("ReachIndicator", deckArea.transform);
            SetAnchors(reachIndicator, new Vector2(0.7f, 0.5f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            var reachText = CreateTextObject("ReachText", reachIndicator.transform, "REACH!", 12);
            SetAnchors(reachText.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            reachText.color = Color.red;
            reachText.alignment = TextAnchor.MiddleCenter;
            reachIndicator.SetActive(false);

            var deckView = deckArea.AddComponent<DeckView>();
            SetPrivateField(deckView, "_countText", deckCountText);
            SetPrivateField(deckView, "_handCountText", handCountText);
            SetPrivateField(deckView, "_reachIndicator", reachIndicator);

            // PlayButton #2D8A5E
            var playBtn = CreateButton("PlayButton", actionPanel.transform, "出す");
            SetAnchors(playBtn, new Vector2(0.05f, 0.38f), new Vector2(0.95f, 0.65f), Vector2.zero, Vector2.zero);
            var playBtnImg = playBtn.GetComponent<Image>();
            if (playBtnImg != null)
                playBtnImg.color = new Color(0.416f, 0.239f, 0.722f); // #6A3DB8 — TitleScene統一

            // PassButton — 暗めパープル #3D2878
            var passBtn = CreateButton("PassButton", actionPanel.transform, "パス");
            SetAnchors(passBtn, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.32f), Vector2.zero, Vector2.zero);
            var passBtnImg = passBtn.GetComponent<Image>();
            if (passBtnImg != null)
                passBtnImg.color = new Color(0.239f, 0.157f, 0.471f); // #3D2878 — TitleScene統一

            // 確定ボタン（非表示）
            var confirmBtn = CreateButton("ConfirmButton", actionPanel.transform, "確定");
            SetAnchors(confirmBtn, new Vector2(0.2f, 0.33f), new Vector2(0.8f, 0.42f), Vector2.zero, Vector2.zero);
            confirmBtn.SetActive(false);

            // カード戻しパネル（非表示）
            var returnPanel = CreateUIObject("ReturnCardPanel", actionPanel.transform);
            SetAnchors(returnPanel, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);
            returnPanel.AddComponent<Image>().color = new Color(0.5f, 0.5f, 0.2f, 0.3f);
            returnPanel.SetActive(false);

            // GameUI コンポーネント
            var gameUI = actionPanel.AddComponent<GameUI>();
            SetPrivateField(gameUI, "_playButton", playBtn.GetComponent<Button>());
            SetPrivateField(gameUI, "_passButton", passBtn.GetComponent<Button>());
            SetPrivateField(gameUI, "_confirmButton", confirmBtn.GetComponent<Button>());
            SetPrivateField(gameUI, "_returnCardPanel", returnPanel);
        }

        // ============================================================
        // ResultDialog（オーバーレイ）
        // ============================================================
        private static void BuildResultDialog(Transform canvasTransform)
        {
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
            SetAnchors(titleBtn, new Vector2(0.3f, 0.05f), new Vector2(0.7f, 0.25f), Vector2.zero, Vector2.zero);

            var resultView = resultOverlay.AddComponent<ResultDialogView>();
            SetPrivateField(resultView, "_winnerText", winnerText);
            SetPrivateField(resultView, "_scoreText", scoreText);
            SetPrivateField(resultView, "_dialogPanel", resultOverlay);
            SetPrivateField(resultView, "_titleButton", titleBtn.GetComponent<Button>());

            resultOverlay.SetActive(false);
        }

        // ============================================================
        // AIInfoPanel
        // ============================================================
        private static GameObject CreateAIInfoPanel(string name, Transform parent, int aiIndex)
        {
            var panel = CreateUIObject(name, parent);
            var layoutElem = panel.AddComponent<LayoutElement>();
            layoutElem.flexibleWidth = 1;
            layoutElem.flexibleHeight = 1;

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.176f, 0.106f, 0.369f, 0.80f); // #2D1B5E CC — TitleScene統一

            // 上段 80%~100%: AI名 + 山札枚数 + アイコン + トータル
            var aiNameText = CreateTextObject("AINameText", panel.transform, $"AI{aiIndex}", 11);
            SetAnchors(aiNameText.gameObject, new Vector2(0.02f, 0.8f), new Vector2(0.22f, 1f), Vector2.zero, Vector2.zero);
            aiNameText.color = Color.white;
            aiNameText.alignment = TextAnchor.MiddleCenter;
            aiNameText.raycastTarget = false;

            // アイコンフレーム（circle mask プレイスホルダー）
            var iconFrame = CreateUIObject("IconFrame", panel.transform);
            SetAnchors(iconFrame, new Vector2(0.22f, 0.80f), new Vector2(0.32f, 1f), Vector2.zero, Vector2.zero);
            var iconImage = iconFrame.AddComponent<Image>();
            iconImage.color = new Color(0.239f, 0.149f, 0.471f, 0.8f); // #3D2878
            iconImage.raycastTarget = false;
            var mask = iconFrame.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            var deckCountText = CreateTextObject("DeckCountText", panel.transform, "山札:6", 9);
            SetAnchors(deckCountText.gameObject, new Vector2(0.33f, 0.8f), new Vector2(0.55f, 1f), Vector2.zero, Vector2.zero);
            deckCountText.color = new Color(0.7f, 0.7f, 0.7f);
            deckCountText.alignment = TextAnchor.MiddleLeft;
            deckCountText.raycastTarget = false;

            // TotalCardCount
            var totalCountText = CreateTextObject("TotalCountText", panel.transform, "計:9枚", 9);
            SetAnchors(totalCountText.gameObject, new Vector2(0.55f, 0.8f), new Vector2(0.72f, 1f), Vector2.zero, Vector2.zero);
            totalCountText.color = new Color(0.7f, 0.7f, 0.7f);
            totalCountText.alignment = TextAnchor.MiddleLeft;
            totalCountText.raycastTarget = false;

            var reachIcon = CreateUIObject("ReachIcon", panel.transform);
            SetAnchors(reachIcon, new Vector2(0.72f, 0.8f), new Vector2(0.84f, 1f), Vector2.zero, Vector2.zero);
            var reachText = CreateTextObject("ReachText", reachIcon.transform, "\u26A1", 11);
            SetAnchors(reachText.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            reachText.alignment = TextAnchor.MiddleCenter;
            reachText.color = Color.yellow;
            reachText.raycastTarget = false;
            reachIcon.SetActive(false);

            var thinkingIcon = CreateUIObject("ThinkingIcon", panel.transform);
            SetAnchors(thinkingIcon, new Vector2(0.84f, 0.8f), new Vector2(0.98f, 1f), Vector2.zero, Vector2.zero);
            var thinkText = CreateTextObject("ThinkingText", thinkingIcon.transform, "思考中...", 8);
            SetAnchors(thinkText.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            thinkText.alignment = TextAnchor.MiddleCenter;
            thinkText.color = Color.cyan;
            thinkText.raycastTarget = false;
            thinkingIcon.SetActive(false);

            // 中段: 手札カード裏面
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

            // 下段: 手札枚数
            var handCountText = CreateTextObject("HandCountText", panel.transform, "3枚", 9);
            SetAnchors(handCountText.gameObject, new Vector2(0.02f, 0f), new Vector2(0.98f, 0.15f), Vector2.zero, Vector2.zero);
            handCountText.color = new Color(0.8f, 0.8f, 0.8f);
            handCountText.alignment = TextAnchor.MiddleCenter;
            handCountText.raycastTarget = false;

            var aiInfoView = panel.AddComponent<AIInfoView>();
            SetPrivateField(aiInfoView, "_aiNameText", aiNameText);
            SetPrivateField(aiInfoView, "_handCountText", handCountText);
            SetPrivateField(aiInfoView, "_deckCountText", deckCountText);
            SetPrivateField(aiInfoView, "_totalCountText", totalCountText);
            SetPrivateField(aiInfoView, "_reachIcon", reachIcon);
            SetPrivateField(aiInfoView, "_thinkingIcon", thinkingIcon);
            SetPrivateField(aiInfoView, "_handContainer", handContainer.transform);

            return panel;
        }

        // ============================================================
        // Helpers
        // ============================================================
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
            image.color = new Color(0.416f, 0.239f, 0.722f, 1f); // #6A3DB8 TitleScene統一
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
