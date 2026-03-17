# CLAUDE.md — Magical Competition

## プロジェクト概要

| 項目 | 内容 |
|---|---|
| ゲーム名 | Magical Competition（原作: ソルカルコンペティション） |
| エンジン | Unity 2022.3 LTS |
| 言語 | C# |
| モード | 1人用（対戦AI） |
| アーキテクチャ | MVC + State Machine + ScriptableObject |

---

## ファイル配置ルール

**すべての制作物は以下のパス以下に置くこと。例外なし。**

```
Magical Competition/Assets/_Project/
├── Animations/
├── Audio/
│   ├── BGM/
│   └── SE/
├── Data/
│   ├── Cards/        ← CardData .asset ファイル
│   ├── Rules/        ← RuleConfig.asset
│   └── AI/           ← EasyAIConfig.asset 等
├── Materials/
├── Prefabs/
│   ├── Cards/
│   ├── UI/
│   └── Effects/
├── Scenes/
│   ├── TitleScene.unity
│   ├── GameScene.unity
│   └── ResultScene.unity
├── Scripts/
│   ├── Core/
│   │   ├── Model/    ← Unity非依存のデータクラス
│   │   ├── Systems/  ← ゲームロジック（JudgeSystem等）
│   │   └── AI/       ← AIController・各Strategyクラス
│   ├── Controllers/  ← MonoBehaviour系コントローラー
│   ├── Views/        ← 表示・アニメーション
│   ├── UI/           ← 画面遷移・ダイアログ
│   ├── Utils/        ← 共通ユーティリティ
│   └── Editor/       ← エディタ拡張（ビルド対象外）
├── Sprites/
│   ├── Cards/
│   │   ├── Detective/
│   │   ├── Asset/
│   │   └── Common/
│   ├── UI/
│   └── Effects/
└── Tests/
    ├── EditMode/
    └── PlayMode/
```

---

## 命名規則

| 対象 | 規則 | 例 |
|---|---|---|
| クラス名 | PascalCase | `GameController`, `CardView` |
| インターフェース | `I` + PascalCase | `IAIStrategy` |
| メソッド | PascalCase | `DecideAction()`, `Evaluate()` |
| privateフィールド | `_` + camelCase | `_currentPlayer`, `_deck` |
| ScriptableObject | 種別_連番 | `Detective_001.asset` |
| Prefab | PascalCase | `CardView.prefab` |
| Scene | PascalCase + Scene | `GameScene.unity` |
| Sprite | 種別_連番 | `Detective_001.png` |
| SE | `se_` + 動作名 | `se_card_play.ogg` |
| BGM | `bgm_` + 場面名 | `bgm_game.ogg` |

---

## アーキテクチャ・依存ルール

依存は **必ず一方向**。循環参照は禁止。

```
Controllers ──→ Core/Systems
Controllers ──→ Views
Views       ──→ Core/Model（IReadOnly経由）
Views       ──→ Controllers（Eventのみ）

❌ Core/Systems → Views   （禁止）
❌ Core/Model  → UnityEngine（CardData.cs 除く）
```

---

## ゲームのコアデータ

### CardData（ScriptableObject）
```csharp
public class CardData : ScriptableObject {
    public string   cardId;
    public CardType cardType;   // Detective / Asset
    public int      number;     // 1〜13
    public ColorType color;     // Red/Blue/Green/Yellow/Purple
    public bool     hasSameMark;
    public Sprite   cardSprite;
    public Sprite   backSprite;
    public string   description;
}
```

### GamePhase（enum）
```
Setup → Draw → Play → Judge → NextTurn → End
```

### AIStrategy（interface）
```csharp
public interface IAIStrategy {
    AIAction DecideAction(GameState state);
}
// 実装: RandomStrategy / GreedyStrategy / LookaheadStrategy
```

---

## ⚠️ 未確定ルール（実装前に確認すること）

以下はマニュアルから読み取れなかった箇所。**仮実装で進め、確認後に差し替える。**

| # | 項目 | 仮実装方針 |
|---|---|---|
| 1 | 勝利条件の数値 | 山札枯渇時に最高スコアで勝利 |
| 2 | sameマークの効果 | 揃ったとき特殊ボーナス（TBD） |
| 3 | カードのプレイ条件 | 手番中いつでも任意の1枚を出せる |
| 4 | 色の強度順序 | 赤 > 青 > 緑 > 黄 > 紫（仮） |
| 5 | 手札上限枚数 | 5枚（仮） |
| 6 | スコア計算式 | 資産カードのnumber合算値 |

---

## 作業ルール

- **1フェーズごとに動作確認を行ってから次に進む**
- フェーズ完了時は `✅ PhaseN 完了` とコメントに記録する
- Core/Systems/ のクラスは必ずEditModeテストを書く
- `TODO:` コメントで未確定箇所を明示する
- コミット単位はフェーズ単位を推奨
