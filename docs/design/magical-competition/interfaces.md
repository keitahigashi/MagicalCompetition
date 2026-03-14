# Magical Competition C# 型定義

**作成日**: 2026-03-13
**関連設計**: [architecture.md](architecture.md)

**【信頼性レベル凡例】**:
- 🔵 **青信号**: 要件定義書・マニュアル・ユーザヒアリングを参考にした確実な定義
- 🟡 **黄信号**: 要件定義書・マニュアル・ユーザヒアリングから妥当な推測による定義
- 🔴 **赤信号**: 要件定義書・マニュアル・ユーザヒアリングにない推測による定義

---

## 列挙型

### CardColor 🔵

*要件定義書・マニュアルのカード構成より*

```csharp
// カードの色（元素）
public enum CardColor
{
    Fire,   // 火（赤）  ファイルプレフィックス: 030-038
    Water,  // 水（青）  ファイルプレフィックス: 010-018
    Light,  // 光（黄）  ファイルプレフィックス: 020-028
    Earth,  // 土（緑）  ファイルプレフィックス: 040-048
    Wind,   // 風（紫）  ファイルプレフィックス: 050-058
    Any     // 任意（「好きな色の5」疑似場札用）
}
```

### GamePhase 🔵

*要件定義書・ユーザヒアリング（フェーズ設計承認）より*

```csharp
// ゲームフェーズ
public enum GamePhase
{
    Setup,            // カード配布・初期手札ドロー
    SelectFieldCard,  // 初期場札/リセット後の「好きな色の5」設定
    Play,             // カードを出す or パス
    Draw,             // 手札補充
    CheckWin,         // 勝利判定
    NextTurn,         // 次プレイヤーへ
    AllPassReset,     // 全員パス時の場札リセット
    End               // ゲーム終了・結果表示
}
```

### PlayType 🔵

*要件定義書 REQ-101〜103 より*

```csharp
// カードプレイの種類
public enum PlayType
{
    SameNumber,    // 同じ数字（REQ-101）
    SameColor,     // 同じ色（REQ-102）
    Arithmetic,    // 計算出し（REQ-103）
    Pass           // パス（REQ-106）
}
```

---

## Core/Model クラス

### Card 🔵

*要件定義書 REQ-001・マニュアルより*

```csharp
// カードのランタイム表現（Unity非依存）
public class Card
{
    public CardColor Color { get; }  // カードの色
    public int Number { get; }       // カードの番号（1〜9）

    public Card(CardColor color, int number) { ... }

    public override bool Equals(object obj) { ... }
    public override int GetHashCode() { ... }
    public override string ToString() => $"{Color}:{Number}";
}
```

### CardData (ScriptableObject) 🔵

*CLAUDE.md・tech-stack.md より*

```csharp
// カードの静的データ（ScriptableObject）
[CreateAssetMenu(fileName = "CardData", menuName = "MagicalCompetition/CardData")]
public class CardData : ScriptableObject
{
    public CardColor color;
    public int number;          // 1〜9
    public Sprite cardSprite;   // カード表面スプライト
    public Sprite backSprite;   // カード裏面スプライト（共通）
}
```

### PlayerState 🔵

*要件定義書 REQ-003,004,106,109 より*

```csharp
// プレイヤーの状態
public class PlayerState
{
    public int PlayerId { get; }
    public bool IsHuman { get; }
    public List<Card> Hand { get; }          // 手札（最大3枚、リーチ時は3未満）
    public List<Card> Deck { get; }          // 山札
    public bool IsReach { get; set; }        // リーチ状態（山札が空）

    public int TotalCardCount => Hand.Count + Deck.Count;
    public bool HasWon => TotalCardCount == 0;

    public void AddToHand(Card card) { ... }
    public void RemoveFromHand(Card card) { ... }
    public void RemoveFromHand(List<Card> cards) { ... }
    public Card DrawFromDeck() { ... }
    public void ReturnToDeckBottom(List<Card> cards) { ... }
}
```

### FieldState 🔵

*要件定義書 REQ-005,105,107 より*

```csharp
// 場札の状態
public class FieldState
{
    public CardColor Color { get; private set; }  // 現在の場札の色
    public int Number { get; private set; }       // 現在の場札の番号
    public bool IsVirtual { get; private set; }   // 疑似場札か（好きな色の5）

    // 場札更新（最後に出したカードで更新）
    public void Update(Card lastPlayedCard) { ... }

    // 疑似場札設定（好きな色の5）
    public void SetVirtual(CardColor color) { ... }

    // 場札リセット
    public void Clear() { ... }
}
```

### GameState 🔵

*要件定義書全体より*

```csharp
// ゲーム全体の状態
public class GameState
{
    public List<PlayerState> Players { get; }
    public GamePhase CurrentPhase { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public FieldState Field { get; }
    public int ConsecutivePassCount { get; set; }  // 連続パス回数
    public List<Card> DiscardPile { get; }         // 場に出されたカード
    public List<Card> ExcludedCards { get; }        // 配布余りの除外カード

    public PlayerState CurrentPlayer => Players[CurrentPlayerIndex];
    public int PlayerCount => Players.Count;
    public bool AllPlayersPassed => ConsecutivePassCount >= PlayerCount;

    public void AdvanceTurn() { ... }  // 次プレイヤーへ
    public void ResetPassCount() { ... }
}
```

### GameConfig 🔵

*要件定義書 REQ-002 より*

```csharp
// ゲーム設定
public class GameConfig
{
    public int AICount { get; }  // AI対戦相手の人数（1〜4）

    public int TotalPlayerCount => AICount + 1;  // プレイヤー + AI
    public const int TotalCards = 45;
    public const int HandSize = 3;
    public const int InitialFieldNumber = 5;
}
```

### PlayAction 🔵

*要件定義書 REQ-101〜106 より*

```csharp
// プレイヤーのアクション
public class PlayAction
{
    public PlayType Type { get; }
    public List<Card> Cards { get; }             // 出すカード（パス時は山札に戻すカード）
    public Card LastCard => Cards.LastOrDefault(); // 場札更新用の最後のカード

    public static PlayAction CreatePlay(PlayType type, List<Card> cards) { ... }
    public static PlayAction CreatePass(List<Card> cardsToReturn) { ... }
}
```

### PlayResult 🔵

*要件定義書 REQ-202 より*

```csharp
// ゲーム結果
public class PlayResult
{
    public PlayerState Winner { get; }
    public int Score { get; }  // 他プレイヤーの残カード合計枚数
    public List<PlayerState> AllPlayers { get; }
}
```

---

## Core/Systems インターフェース

### PlayValidator 🔵

*要件定義書 REQ-101〜104 より*

```csharp
// カードプレイの妥当性判定
public class PlayValidator
{
    // 選択カードが合法なプレイかを判定
    public bool Validate(List<Card> selectedCards, FieldState field);

    // 出せる全ての有効な組み合わせを取得
    public List<PlayAction> GetAllValidPlays(List<Card> hand, FieldState field);

    // 同数字プレイの判定
    public bool IsSameNumberPlay(List<Card> cards, FieldState field);

    // 同色プレイの判定
    public bool IsSameColorPlay(List<Card> cards, FieldState field);

    // 計算出しの判定
    public bool IsArithmeticPlay(List<Card> cards, FieldState field);
}
```

### ArithmeticValidator 🔵

*要件定義書 REQ-103,104・EDGE-201〜204 より*

```csharp
// 計算出しの妥当性判定
public class ArithmeticValidator
{
    // 2〜3枚のカードの組み合わせで場札の数字（一の位）と一致するか判定
    // 計算過程でマイナス禁止、0は許容
    public bool Validate(List<Card> cards, int targetNumber);

    // 全ての有効な演算子の組み合わせを探索
    // 例: [a,b] → a+b, a-b, b-a
    // 例: [a,b,c] → 全順列 × 全演算子パターン
    public List<ArithmeticExpression> FindValidExpressions(List<Card> cards, int targetNumber);
}

// 計算式の表現
public class ArithmeticExpression
{
    public List<Card> Cards { get; }        // 使用するカード（順序付き）
    public List<char> Operators { get; }    // 演算子（'+' or '-'）
    public int Result { get; }              // 計算結果
    public string Expression { get; }       // 表示用文字列（例: "9 - 2 = 7"）
}
```

---

## Core/AI インターフェース

### IAIStrategy 🔵

*CLAUDE.md・要件定義書 REQ-301 より*

```csharp
// AI戦略インターフェース
public interface IAIStrategy
{
    PlayAction DecideAction(GameState state, List<PlayAction> validPlays);
    CardColor SelectFieldColor(GameState state);  // 場札リセット時の色選択
}
```

### NormalStrategy 🔵

*要件定義書 REQ-301 より*

```csharp
// Normal難易度AI
public class NormalStrategy : IAIStrategy
{
    // 戦略:
    // 1. 出せるカードがあれば、最も多く出せる手を優先
    // 2. 同数字 > 同色 > 計算出し の優先順位
    // 3. パス時は次のターンで使いにくいカードを山札に戻す
    public PlayAction DecideAction(GameState state, List<PlayAction> validPlays) { ... }

    // 手札に最も多い色を選択
    public CardColor SelectFieldColor(GameState state) { ... }
}
```

---

## Views インターフェース概要

### CardView 🔵

*要件定義書 REQ-601・tech-stack.md より*

```csharp
// カードの表示（UI Image ベース）
public class CardView : MonoBehaviour
{
    public Image cardImage;           // カード画像
    public Card CardData { get; }     // 対応するカードデータ

    public void SetCard(Card card, Sprite sprite);
    public void SetSelected(bool selected);       // 選択状態（少し上に浮く）
    public void SetInteractable(bool enabled);    // 操作可能/グレーアウト
    public void PlayMoveAnimation(Vector3 target, float duration);  // 移動アニメ
    public void PlayFadeAnimation(float targetAlpha, float duration);
}
```

---

## 画面レイアウト定義 🔵

*ユーザヒアリングより*

```
┌───────────────────────────────────────────┐
│          AI情報エリア（上部）               │
│  [AI1: 手札3 山札8] [AI2: 手札2 山札5 ⚡]  │
│  ※ ⚡ = リーチ                            │
├───────────────────────────────────────────┤
│                                           │
│          場札エリア（中央）                 │
│        ┌─────────┐                        │
│        │  火 ・ 5 │  ← 現在の場札          │
│        └─────────┘                        │
│     ターン表示: 「あなたのターン」           │
│                                           │
├───────────────────────────────────────────┤
│  プレイヤーエリア（下部）                   │
│                                           │
│  ┌───┐ ┌───┐ ┌───┐    ┌────┐             │
│  │ C1│ │ C2│ │ C3│    │山札│             │
│  └───┘ └───┘ └───┘    │ 12 │             │
│   手札（横並び）        └────┘             │
│                                           │
│      [出す]  [パス]                        │
└───────────────────────────────────────────┘
```

---

## 信頼性レベルサマリー

- 🔵 青信号: 18件 (100%)
- 🟡 黄信号: 0件 (0%)
- 🔴 赤信号: 0件 (0%)

**品質評価**: ✅ 高品質
