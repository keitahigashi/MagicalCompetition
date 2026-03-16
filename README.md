# Magical Competition

**原作: マジカル・コンペ** — 魔法石カードを使った計算バトルゲーム

| 項目 | 内容 |
|---|---|
| エンジン | Unity 6 (6000.3.7f1) |
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
  - **同数プレイ (number)** — 場と同じ数字のカードを出す
  - **同色プレイ (color)** — 場と同じ色のカードを出す
  - **算術プレイ (same)** — 2〜3枚の加減算で場の数字と一致させる（左→右評価、途中マイナス禁止）
- **パス**: 選択中のカードがあれば山札の底に戻し、新たなカードを補充
- **勝利条件**: 手札 + デッキ = 0枚で上がり
- **スコア**: 対戦相手全員の残りカード枚数の合計
- **全員パス時**: 場をリセットして続行

---

## 画面構成

### タイトル画面 (TitleScene)
- AI人数選択（◀ 1〜4体 ▶）
- ゲーム開始ボタン

### ゲーム画面 (GameScene)
```
[AI1 山札:N] [AI2 山札:N] [AI3 山札:N] [AI4 山札:N]  ← 上部: AI情報パネル
 [裏][裏][裏]  [裏][裏][裏]  [裏][裏][裏]  [裏][裏][裏]
    3枚           3枚           3枚           3枚

              ┌─────────────┐
              │   場 札     │                              ← 中央: 場札エリア
              └─────────────┘
              あなたのターン                                ← ターン表示

[出す] [手札1][手札2][手札3] [山札:N / 手札:N] [パス]     ← 下部: プレイヤーエリア
```

- リザルトはゲーム画面内のモーダルダイアログで表示

---

## 実装状況

### Core Model（11クラス） — 完了

純粋C#のデータ層。Unity非依存。

| クラス | 概要 |
|---|---|
| `Card` | 不変カードクラス（Color, Number, Equals/GetHashCode） |
| `CardColor` | 列挙型: Fire, Water, Light, Earth, Wind, Any |
| `CardData` | ScriptableObject（スプライト参照付き） |
| `GameState` | Players, CurrentPhase, FieldState, PassCount 等を保持 |
| `PlayerState` | Hand, Deck, IsReach, IsHuman |
| `PlayAction` | 不変。ファクトリメソッド `CreatePlay()` / `CreatePass()` |
| `PlayResult` | Winner, Score, AllPlayers |
| `FieldState` | 場の Color, Number, IsVirtual |
| `GameConfig` | AI人数などの設定 |
| `GamePhase` | 8フェーズ: Setup → SelectFieldCard → Play → Draw → CheckWin → NextTurn → AllPassReset → End |
| `PlayType` | SameNumber / SameColor / Arithmetic / Pass |

### Core Systems（8クラス） — 完了

ゲームロジック層。すべてEditModeテスト済み。

| クラス | 概要 |
|---|---|
| `PlayValidator` | カードプレイの妥当性検証（同数/同色/算術）+ 全有効手列挙 |
| `ArithmeticValidator` | 2〜3枚の加減算を左→右で評価（途中マイナス禁止） |
| `CardDistributor` | 45枚デッキ生成 + Fisher-Yatesシャッフル + 配布 |
| `DrawSystem` | 初期ドロー（3枚）、補充、リーチ判定 |
| `FieldUpdater` | プレイ後の場の状態更新、バーチャルフィールド設定 |
| `TurnManager` | ターン進行、パス/プレイ記録、全員パス判定 |
| `WinChecker` | 勝利判定（手札+デッキ=0） |
| `ScoreCalculator` | 勝者スコア = 相手全員の残りカード合計 |

### Core AI（2クラス + インターフェース） — 一部完了

| クラス | 概要 | 状態 |
|---|---|---|
| `IAIStrategy` | 戦略インターフェース（`DecideAction`, `SelectFieldColor`） | 完了 |
| `NormalStrategy` | 貪欲法（カード枚数優先 → 同数 > 同色 > 算術）。パス時は使いにくいカードを山札に戻す | 完了 |
| `AIActionEvaluator` | PlayValidatorラッパー（全有効手の列挙） | 完了 |
| Easy / Hard Strategy | 難易度別戦略 | **未実装** |

### Controllers（5クラス） — 完了

| クラス | 概要 |
|---|---|
| `GameController` | ゲーム全体の進行制御（8フェーズの実行メソッド）。パス時も手札補充を実行 |
| `GameStateMachine` | フェーズ遷移バリデーション + OnPhaseEnter/Exit イベント |
| `AIController` | AI思考実行（有効手探索 → 戦略選択） |
| `PlayerInputController` | プレイヤー入力管理（カード選択、パスモード、確認） |
| `SceneController` | シーン切り替え + AI人数の受け渡し |

### Views（7クラス） — 完了

| クラス | 概要 |
|---|---|
| `CardView` | カードUI（画像+テキストラベル、選択時Y浮上、グレーアウト、移動/フェードアニメーション） |
| `HandView` | 手札表示（HorizontalLayout、カード生成、選択追跡、プレイ可否表示） |
| `FieldView` | 場カード表示（バーチャル場の特殊表示対応） |
| `DeckView` | デッキ残数 + 手札枚数 + リーチ表示 |
| `AIInfoView` | AI情報パネル（名前、山札数、手札裏面カード表示、手札枚数、リーチ、思考中アイコン） |
| `TurnIndicatorView` | 現在ターンのプレイヤー表示 |
| `ResultDialogView` | リザルトモーダル（勝者、スコア、タイトルへ戻るボタン） |

### UI（3クラス） — 完了

| クラス | 概要 |
|---|---|
| `GameSceneManager` | ゲームループ駆動。Controller/View接続、AI実行、フェーズ遷移、プレイログ出力 |
| `GameUI` | ゲーム中の操作ボタン（出す、パス、確認） |
| `TitleUI` | タイトル画面（AI人数選択 1〜4、スタートボタン、UI自動構築） |

### ユーティリティ（2クラス） — 完了

| クラス | 概要 |
|---|---|
| `CardSpriteLoader` | CardSpriteTableからCard→Sprite解決（静的キャッシュ） |
| `CardSpriteTable` | スプライト検索テーブル（ScriptableObject、Editor拡張で自動生成） |

### Editor拡張（7クラス） — 完了

| クラス | 概要 |
|---|---|
| `CardDataGenerator` | CardData ScriptableObjectの一括生成 |
| `CardSpriteImporter` | カード画像のTextureImporter設定（Sprite、WebGL最適化） |
| `CardSpriteTableBuilder` | CardSpriteTable.assetの生成（英語ファイル名対応） |
| `CardAtlasBuilder` | SpriteAtlas生成（GPUバッチング用） |
| `GameSceneBuilder` | GameScene UIの自動構築（AI×4パネル、場札、手札、ボタン、リザルト） |
| `TitleSceneBuilder` | TitleScene UIの自動構築 |
| `WebGLBuildConfig` | WebGLビルド設定 + ビルド実行 |

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

※ ResultScene は GameScene 内の ResultDialogView モーダルで代替

---

## アセット

| 種別 | 状態 | 詳細 |
|---|---|---|
| カード画像 | 57枚収録済み | `Art/card/` に英語ファイル名で格納（高解像度JPG） |
| CardSpriteTable | 生成済み | `Resources/CardSpriteTable.asset` |
| SpriteAtlas | 生成済み | `Sprites/Cards/CardAtlas.spriteatlas` |
| CardData SO | **未生成** | `Data/Cards/` は空。Editor拡張で一括生成可能 |
| Prefab | **未作成** | `Prefabs/Cards/`, `Prefabs/UI/` は空 |
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

## コンソールログ

ゲーム中のカードプレイ・パスはコンソールにログ出力される。

```
Player:number[earth8] earth8           ← 同数プレイ
AI1:same[wind4+earth4 = 8] earth4     ← 算術プレイ（計算過程付き）
AI2:color[fire3,fire7] fire7           ← 同色プレイ
Player:pass(return:water2)             ← 選択カードを戻してパス
AI3:pass                               ← パス（AIは自動で1枚を戻す）
```

---

## 未実装・TODO

| 予定ver | 項目 | 項目 |
|---|---|---|
|0.4| プレイログ | プレイ中のログを表示 |
|0.45| WebGL調整 | [unityroom](https://unityroom.com/games/magicalcompetition) |
|0.5| 演出 | カードプレイ/勝利時のアニメーション、エフェクトの追加 |
|0.6| サウンド | BGM / SE |
|0.7| 魔法カード | 魔法カードの追加 |
|0.8| デザイン調整 | ボタン、背景、画像の適用 |
|0.9| デバッグ | 進行に影響にある不具合の解消 |
|---| CardData アセット | Editor拡張（CardDataGenerator）で生成可能だが未実行 |
|---| Prefab | CardView等のPrefab化が未完了 |

---

## セットアップ

1. Unity Hub で **Unity 6** (6000.3.7f1) をインストール
2. このリポジトリをクローン
3. Unity Hub → **Open** → プロジェクトフォルダを選択
4. URP および関連パッケージは `Packages/manifest.json` で自動解決される
5. Tools メニューからシーン構築:
   - `Tools > MagicalCompetition > Build TitleScene UI`
   - `Tools > MagicalCompetition > Build GameScene UI`
   - `Tools > MagicalCompetition > Build CardSpriteTable`

---

## ライセンス

Private — All rights reserved.
