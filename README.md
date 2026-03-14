# Magical Competition

**原作: ソルカルコンペティション** — 魔法石カードを使った計算バトルゲーム

| 項目 | 内容 |
|---|---|
| エンジン | Unity 2022.3 LTS |
| レンダリング | URP (2D) |
| 言語 | C# |
| モード | 1人プレイ（AI 1〜4体と対戦） |
| アーキテクチャ | MVC + State Machine + ScriptableObject |

---

## ゲームルール概要

- **デッキ構成**: 5色（火・水・光・土・風）× 数字 1〜9 = 計45枚
- **手札**: 常に3枚まで補充（デッキ切れ後はリーチ状態）
- **場の初期値**: 任意色の5（バーチャルフィールド）
- **カードの出し方**:
  - **同数プレイ** — 場と同じ数字のカードを出す
  - **同色プレイ** — 場と同じ色のカードを出す
  - **算術プレイ** — 2〜3枚の四則演算で場の数字の一の位と一致させる（左→右評価、途中マイナス禁止）
- **勝利条件**: 手札 + デッキ = 0枚で上がり
- **スコア**: 対戦相手全員の残りカード枚数の合計
- **全員パス時**: 場をリセットして続行

---

## 実装状況

### Core Model（7クラス） — 完了

純粋C#のデータ層。Unity非依存。

| クラス | 概要 |
|---|---|
| `Card` | 不変カードクラス（Color, Number, Equals/GetHashCode） |
| `CardColor` | 列挙型: Fire, Water, Light, Earth, Wind, Any |
| `CardData` | ScriptableObject（スプライト参照付き） |
| `GameState` | Players, CurrentPhase, FieldState, PassCount 等を保持 |
| `PlayerState` | Hand, Deck, IsReach, IsHuman |
| `PlayAction` | 不変。ファクトリメソッド `CreatePlay()` / `CreatePass()` |
| `FieldState` | 場の Color, Number, IsVirtual |
| `GameConfig` | AI人数などの設定 |
| `GamePhase` | 8フェーズ: Setup → SelectFieldCard → Play → Draw → CheckWin → NextTurn → AllPassReset → End |
| `PlayType` | SameNumber / SameColor / Arithmetic / Pass |
| `PlayResult` | Winner, Score, AllPlayers |

### Core Systems（7クラス） — 完了

ゲームロジック層。すべてEditModeテスト済み。

| クラス | 概要 |
|---|---|
| `PlayValidator` | カードプレイの妥当性検証（同数/同色/算術） |
| `ArithmeticValidator` | 2〜3枚の四則演算を左→右で評価 |
| `CardDistributor` | 45枚デッキ生成 + Fisher-Yatesシャッフル + 配布 |
| `DrawSystem` | 初期ドロー（3枚）、補充、リーチ判定 |
| `FieldUpdater` | プレイ後の場の状態更新 |
| `TurnManager` | ターン進行、パス/プレイ記録、全員パス判定 |
| `WinChecker` | 勝利判定（手札+デッキ=0） |
| `ScoreCalculator` | 勝者スコア = 相手全員の残りカード合計 |

### Core AI（2クラス + インターフェース） — 一部完了

| クラス | 概要 | 状態 |
|---|---|---|
| `IAIStrategy` | 戦略インターフェース（`DecideAction`, `SelectFieldColor`） | 完了 |
| `NormalStrategy` | 貪欲法（カード枚数優先 → 同数 > 同色 > 算術） | 完了 |
| `AIActionEvaluator` | PlayValidatorラッパー（全有効手の列挙） | 完了 |
| Easy / Hard Strategy | 難易度別戦略 | **未実装** |

### Controllers（5クラス） — 完了

| クラス | 概要 |
|---|---|
| `GameController` | ゲーム全体の進行制御（8フェーズの実行メソッド） |
| `GameStateMachine` | フェーズ遷移バリデーション + OnPhaseEnter/Exit イベント |
| `AIController` | AI思考実行（有効手探索 → 戦略選択） |
| `PlayerInputController` | プレイヤー入力管理（カード選択、パスモード、確認） |
| `SceneController` | シーン切り替え + AI人数の受け渡し |

### Views（7クラス） — 完了

| クラス | 概要 |
|---|---|
| `CardView` | カードUI（選択、グレーアウト、移動/フェードアニメーション） |
| `HandView` | 手札表示（HorizontalLayout、カード生成、選択追跡） |
| `FieldView` | 場カード表示（色/数字テキスト、バーチャル場の特殊表示） |
| `DeckView` | デッキ残数 + リーチ表示 |
| `AIInfoView` | AI情報（名前、手札数、デッキ数、リーチ、思考中アイコン） |
| `TurnIndicatorView` | 現在ターンのプレイヤー表示 |
| `ResultDialogView` | リザルトモーダル（勝者、スコア、タイトルへ戻る） |

### UI（2クラス） — 完了

| クラス | 概要 |
|---|---|
| `GameUI` | ゲーム中の操作ボタン（プレイ、パス、確認） |
| `TitleUI` | タイトル画面（AI人数選択 1〜4、スタートボタン） |

### Editor拡張（6クラス） — 完了

CardDataGenerator, CardAtlasBuilder, CardSpriteImporter, GameSceneBuilder, TitleSceneBuilder, WebGLBuildConfig

---

## テスト

| 種別 | ファイル数 | 対象 |
|---|---|---|
| EditMode | 24 | Model全クラス、Systems全クラス、Controllers、AI |
| PlayMode | 10 | Views全クラス、UI、ゲームフロー統合テスト |

---

## シーン構成

| シーン | 状態 | 概要 |
|---|---|---|
| `TitleScene` | 構築済み | AI人数選択 → ゲーム開始 |
| `GameScene` | 構築済み | メインゲーム（全View/Controller配置済み） |
| `ResultScene` | 未作成 | GameScene内のResultDialogViewで代替中 |

---

## アセット

| 種別 | 状態 | 詳細 |
|---|---|---|
| カード画像 | 57枚収録済み | `Assets/_Project/Art/card/` に高解像度JPG |
| CardData SO | **未生成** | `Data/Cards/` は空。Editor拡張で一括生成可能 |
| Prefab | **未作成** | `Prefabs/Cards/`, `Prefabs/UI/` は空 |
| SpriteAtlas | 作成済み | `Sprites/Cards/CardAtlas.spriteatlas` |
| BGM / SE | **未作成** | `Audio/` ディレクトリのみ |

---

## アセンブリ構成

```
MagicalCompetition.Core          （依存なし）
  ├── MagicalCompetition.Controllers  → Core
  ├── MagicalCompetition.Views        → Core
  └── MagicalCompetition.UI           → Core + Controllers + Views
MagicalCompetition.Editor            → 全アセンブリ（Editor専用）
MagicalCompetition.Tests.EditMode    → Core + Controllers（Editor専用）
MagicalCompetition.Tests.PlayMode    → Core + Controllers + Views + UI
```

---

## 未実装・TODO

| 項目 | 詳細 |
|---|---|
| Easy / Hard AI | NormalStrategyのみ実装済み。難易度別戦略は未着手 |
| CardData アセット | Editor拡張（CardDataGenerator）で生成可能だが未実行 |
| Prefab | CardView等のPrefab化が未完了 |
| BGM / SE | オーディオアセットなし |
| `FieldState.cs:8` | `TODO: Switch to GameConfig.InitialFieldNumber when confirmed` |
| `PlayResult.cs:8` | `TODO: Change AllPlayers to IReadOnlyList on refactor` |

---

## セットアップ

1. Unity Hub で Unity **2022.3 LTS** をインストール
2. このリポジトリをクローン
3. Unity Hub → **Open** → プロジェクトフォルダを選択
4. URP および関連パッケージは `Packages/manifest.json` で自動解決される

---

## ライセンス

Private — All rights reserved.
