# Unity Battle Arena UI — 実装プロンプト

## 目標
添付スクリーンショット（Magical Competition Battle Arena）のデザインに合わせて、既存のUnity uGUI実装を修正・更新してください。

## 基本仕様
- 解像度: Canvas Reference Resolution 1920×1080, Screen Match Mode: Match Width or Height (0.5)
- 背景アセット: Assets\_Project\Art\bg\battle_arena.png → BattleScene の Canvas 最背面に配置（Stretch fill）
- UIフレームワーク: uGUI（Canvas/Image/TextMeshPro）
- キャラクターアイコン: プレイスホルダースプライトで可
- カードスプライト: 既存Spriteアセットを動的に割り当て

---

## 01. 全体レイアウト・Canvas 構成

Canvas Hierarchy:
Canvas
  ├── BG_Image（battle_arena.png, Stretch all）
  ├── AIArea_Root（上部 AI×4）
  ├── CenterCard_Root（中央カード＋魔法陣）
  ├── PlayerHand_Root（下部手札）
  ├── LogPanel（左下ログ）
  └── ActionPanel（右下ボタン）

---

## 02. AI キャラクターエリア（上部×4）

AIArea_Root:
- Horizontal Layout Group, spacing=40, padding top=30

AISlot Prefab（×4）:
- IconFrame: Circle Mask + Image 90×90px（プレイスホルダー）
- NameLabel: TMP "AI1"〜"AI4", 16px Bold
- HPLabel: TMP "HP: XX", カラー=#FF6B6B, 14px
- DeckCount: 手札枚数バッジ（"3枚"〜"4枚"）

public void SetHP(int current) => hpLabel.text = $"HP: {current}";

---

## 03. 中央カード表示エリア

CenterCard_Root（anchor center, pos=(0,30), 200×280px）:
- GlowEffect: Image + Animator（pulse alpha 0.4→1.0）
- CardFrame: Image
  - CostBadge: TMP コスト数値, anchor=top-left, カラー=#FFD700
  - CardArt: Image（動的スプライトセット）
  - CardName: TMP カード名, anchor=bottom-center
- TurnLabel: TMP "あなたのターン", pos=(0,-160), 22px

public void ShowCard(CardData data) {
    cardArt.sprite = data.sprite;
    cardName.text  = data.displayName;
    costBadge.text = data.cost.ToString();
}

---

## 04. プレイヤー手札エリア（下部）

PlayerHand_Root（anchor bottom-center, pos=(0,20)）:
- Horizontal Layout Group, spacing=12

HandCard Prefab:
- CardFrame > ElementIcon / CardArt / CardName（12px）
- SelectHighlight: Image 黄色枠, デフォルト非表示

public void OnCardClick(HandCard card) {
    selectedCard = card;
    card.selectHighlight.SetActive(true);
    centerCardView.ShowCard(card.data);
}

Sprite割り当て: Resources.Load<Sprite>($"Cards/{elementType}/{cardName}")

---

## 05. ログパネル（左下）

LogPanel（anchor bottom-left, pos=(20,20), 280×220px）:
- Background: カラー=#1A1040, alpha=0.75
- ScrollRect（verticalScrollbar非表示）> Content（Vertical Layout Group）> LogEntry Prefab
- ログ追加時に最大20件を超えたら古いものを削除し、末尾へ自動スクロール

public void AddLog(string text) {
    var entry = Instantiate(logEntryPrefab, content);
    entry.GetComponent<TMP_Text>().text = text;
    if (content.childCount > 20)
        Destroy(content.GetChild(0).gameObject);
    Canvas.ForceUpdateCanvases();
    scrollRect.verticalNormalizedPosition = 0f;
}

---

## 06. アクションパネル（右下）

ActionPanel（anchor bottom-right, pos=(-20,20)）:
- DeckInfoRow: "山札: X枚" / "手札: X枚", 14px
- PlayButton:  200×60, TMP "出す" 18px, 色: Normal=#2D8A5E / Highlighted=#38B077 / Pressed=#1F6344
- PassButton:  200×60, TMP "パス" 18px, 色: Normal=#8A2D2D / Highlighted=#B03838 / Pressed=#631F1F

public void UpdateCounts(int deck, int hand) {
    deckLabel.text = $"山札: {deck}枚";
    handLabel.text = $"手札: {hand}枚";
}

---

## 07. 既存コードへの修正・新規作成指示

修正対象:
- BattleSceneManager.cs: UI参照フィールドを再アサイン、TurnStart()でcenterCardViewとturnLabelを更新
- CardView.cs: Sprite動的ロード処理と選択ハイライトON/OFFメソッドを追加
- LogManager.cs: AddLog()の修正とScrollRect自動スクロール処理を追加

新規作成スクリプト:
- AISlotView.cs: HP表示・デッキ枚数表示の管理
- HandCardView.cs: 手札カード1枚の表示・選択状態管理
- ActionPanelView.cs: ボタンイベント・カウント更新

Inspectorアサインチェックリスト:
□ BG_Image に battle_arena sprite をセット
□ 各 AISlot に対応する AISlotView をアサイン
□ PlayButton/PassButton の onClick に BattleSceneManager メソッドを登録
□ LogPanel の logEntryPrefab をアサイン