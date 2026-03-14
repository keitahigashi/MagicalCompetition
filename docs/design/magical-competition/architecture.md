# Magical Competition アーキテクチャ設計

**作成日**: 2026-03-13
**関連要件定義**: [requirements.md](../../spec/magical-competition/requirements.md)
**ヒアリング記録**: [design-interview.md](design-interview.md)

**【信頼性レベル凡例】**:
- 🔵 **青信号**: 要件定義書・tech-stack.md・ユーザヒアリングを参考にした確実な設計
- 🟡 **黄信号**: 要件定義書・tech-stack.md・ユーザヒアリングから妥当な推測による設計
- 🔴 **赤信号**: 要件定義書・tech-stack.md・ユーザヒアリングにない推測による設計

---

## システム概要 🔵

**信頼性**: 🔵 *要件定義書・tech-stack.md より*

2Dカードゲーム「マジカルコンペ」のデジタル版。Unity 6.3 + uGUI Canvas で構築し、WebGL ビルドとしてブラウザ上で動作する。MVC + State Machine + ScriptableObject アーキテクチャを採用。

## アーキテクチャパターン 🔵

**信頼性**: 🔵 *CLAUDE.md・tech-stack.md より*

- **パターン**: MVC + State Machine + ScriptableObject
- **選択理由**:
  - シンプルで個人開発に適した構成
  - State Machine でゲームフェーズを明確に制御
  - ScriptableObject によるデータ駆動設計（Inspector で編集可能）
  - Core/Model が Unity 非依存のためテスタブル

## 依存関係ルール 🔵

**信頼性**: 🔵 *CLAUDE.md・tech-stack.md より*

```
Controllers ──→ Core/Systems    （ロジック呼び出し）
Controllers ──→ Core/AI         （AI実行）
Controllers ──→ Views           （表示更新指示）
Views       ──→ Core/Model      （読み取り専用参照）
Views       ──→ Controllers     （Event通知のみ）

❌ Core/Systems → Views          （禁止）
❌ Core/Systems → Controllers    （禁止）
❌ Core/Model  → UnityEngine     （CardData.cs 除く）
```

## コンポーネント構成

### Core/Model（データ層） 🔵

**信頼性**: 🔵 *要件定義書・tech-stack.md より*

Unity 非依存の純粋 C# クラス。テスト容易性を確保。

| クラス | 責務 | 関連要件 |
|---|---|---|
| `CardData` | カードの静的データ（ScriptableObject） | REQ-001 |
| `Card` | カードのランタイム表現（色・番号） | REQ-101〜103 |
| `PlayerState` | プレイヤーの状態（手札・山札・リーチ） | REQ-003,004,109 |
| `FieldState` | 場札の状態（現在の色・番号） | REQ-005,105 |
| `GameState` | ゲーム全体の状態（全プレイヤー・フェーズ・パス連続回数） | REQ-110,201 |
| `GameConfig` | ゲーム設定（プレイヤー数等） | REQ-002 |
| `PlayResult` | カードプレイの結果データ | REQ-202 |

### Core/Systems（ロジック層） 🔵

**信頼性**: 🔵 *要件定義書より*

ゲームルールの実装。Unity 非依存。EditMode テスト必須。

| クラス | 責務 | 関連要件 |
|---|---|---|
| `CardDistributor` | カードのシャッフル・配布 | REQ-001,501 |
| `PlayValidator` | カードプレイの合法性判定（同数字・同色・計算出し） | REQ-101〜104 |
| `ArithmeticValidator` | 計算出しの妥当性判定（マイナス禁止・0許容） | REQ-103,104 |
| `FieldUpdater` | 場札の更新処理 | REQ-105,107 |
| `DrawSystem` | 手札補充処理 | REQ-108,109 |
| `WinChecker` | 勝利判定 | REQ-110 |
| `ScoreCalculator` | 得点計算 | REQ-202 |
| `TurnManager` | ターン進行・パス管理 | REQ-106,107 |

### Core/AI（AI層） 🔵

**信頼性**: 🔵 *要件定義書 REQ-301 より*

| クラス | 責務 | 関連要件 |
|---|---|---|
| `IAIStrategy` | AI戦略インターフェース | REQ-301 |
| `NormalStrategy` | Normal難易度の戦略実装 | REQ-301 |
| `AIActionEvaluator` | 有効手の全探索・評価 | EDGE-401 |

### Controllers（制御層） 🔵

**信頼性**: 🔵 *tech-stack.md アーキテクチャより*

MonoBehaviour 系。ゲームフローの制御と入力処理。

| クラス | 責務 | 関連要件 |
|---|---|---|
| `GameController` | ゲーム全体のフロー制御（State Machine 駆動） | REQ-602 |
| `GameStateMachine` | ゲームフェーズの状態遷移管理 | REQ-602 |
| `PlayerInputController` | プレイヤーの入力処理（カード選択・確定・パス） | NFR-101 |
| `AIController` | AIターンの実行制御 | REQ-302 |
| `SceneController` | シーン遷移管理 | REQ-401 |

### Views（表示層） 🔵

**信頼性**: 🔵 *要件定義書 REQ-403・ユーザヒアリングより*

uGUI Canvas 上の表示コンポーネント。

| クラス | 責務 | 関連要件 |
|---|---|---|
| `CardView` | 個別カードの表示（UI Image） | REQ-601 |
| `HandView` | プレイヤー手札の表示・選択UI | NFR-101,102 |
| `FieldView` | 場札エリアの表示 | NFR-104 |
| `DeckView` | 山札の表示（残り枚数） | REQ-403 |
| `AIInfoView` | AI情報の表示（手札枚数・山札枚数・リーチ） | REQ-403 |
| `TurnIndicatorView` | 現在のターンプレイヤー表示 | NFR-103 |
| `ResultDialogView` | ゲーム結果ダイアログ | REQ-404 |

### UI（画面管理） 🔵

**信頼性**: 🔵 *要件定義書 REQ-401,402 より*

| クラス | 責務 | 関連要件 |
|---|---|---|
| `TitleUI` | タイトル画面（AI人数選択・開始ボタン） | REQ-402 |
| `GameUI` | ゲーム画面の操作ボタン（出す・パス） | NFR-101 |

## ゲームフェーズ State Machine 🔵

**信頼性**: 🔵 *要件定義書・ユーザヒアリングより*

```
                    ┌─────────────────────────────┐
                    ▼                             │
  Setup ──→ SelectFieldCard ──→ Play ──→ Draw ──→ CheckWin ──→ NextTurn
                    ▲              │                  │
                    │              │                  ▼
                    │              │                 End
                    │              ▼
                    └────── AllPassReset
```

| フェーズ | 説明 | 遷移条件 |
|---|---|---|
| `Setup` | カード配布・手札ドロー | 配布完了 → SelectFieldCard |
| `SelectFieldCard` | 初期場札/リセット後の「好きな色の5」疑似設定 | 設定完了 → Play |
| `Play` | カードを出す or パス | カード出し → Draw / パス → NextTurn or AllPassReset |
| `Draw` | 手札を3枚まで補充 | 補充完了 → CheckWin |
| `CheckWin` | 手札+山札=0か判定 | 勝利 → End / 継続 → NextTurn |
| `NextTurn` | 次のプレイヤーへ | 次PL → Play |
| `AllPassReset` | 全員パス時の場札リセット | リセット完了 → SelectFieldCard |
| `End` | ゲーム終了・結果表示 | タイトルへ戻る |

## ディレクトリ構造 🔵

**信頼性**: 🔵 *CLAUDE.md・tech-stack.md より*

```
Assets/_Project/
├── Art/                      ← 元素材（既存・変更不可）
│   ├── card/                 ← カード画像 JPG
│   └── manual/               ← マニュアル画像
├── Data/
│   ├── Cards/                ← CardData .asset (ScriptableObject)
│   └── Rules/                ← GameConfig .asset
├── Prefabs/
│   ├── Cards/                ← CardView.prefab
│   └── UI/                   ← 各種UIプレハブ
├── Scenes/
│   ├── TitleScene.unity
│   └── GameScene.unity
├── Scripts/
│   ├── Core/
│   │   ├── Model/            ← Card, PlayerState, GameState, FieldState 等
│   │   ├── Systems/          ← PlayValidator, CardDistributor, WinChecker 等
│   │   └── AI/               ← IAIStrategy, NormalStrategy, AIActionEvaluator
│   ├── Controllers/          ← GameController, GameStateMachine, PlayerInputController
│   ├── Views/                ← CardView, HandView, FieldView, DeckView 等
│   ├── UI/                   ← TitleUI, GameUI, ResultDialogView
│   ├── Utils/                ← 共通ユーティリティ
│   └── Editor/               ← エディタ拡張（CardData生成ツール等）
├── Sprites/                  ← リサイズ済みスプライト
│   ├── Cards/
│   └── UI/
└── Tests/
    ├── EditMode/             ← Core/Model, Core/Systems テスト
    └── PlayMode/             ← UI連携テスト
```

## データ管理方針 🔵

**信頼性**: 🔵 *tech-stack.md より*

| データ種別 | 管理方法 | 例 |
|---|---|---|
| 静的データ | ScriptableObject | CardData（色・番号・スプライト） |
| ゲーム設定 | ScriptableObject | GameConfig（デフォルトプレイヤー数等） |
| ランタイム状態 | Plain C# クラス | GameState, PlayerState, FieldState |
| 永続化 | なし（1ラウンド制のため不要） | - |

## 非機能要件の実現方法

### パフォーマンス 🔵

**信頼性**: 🔵 *要件定義書 NFR-001〜004 より*

- **60fps維持**: uGUI Canvas + Screen Space Overlay（カメラ不要、描画負荷最小）
- **AI思考500ms以内**: 手札最大3枚のため組み合わせ探索は高速（最大 C(3,2)+C(3,3) = 4通り）
- **ビルドサイズ30MB以下**: カード画像をSprite Atlasで最適化、Brotli圧縮

### カードアニメーション 🔵

**信頼性**: 🔵 *ユーザヒアリングより*

- **方式**: DOTween（または純正Coroutine）による移動+フェードアニメーション
- **対象**: カード移動（手札→場札）、カード配布、手札補充
- **制約**: アニメーション中は入力を無効化

### テスト戦略 🔵

**信頼性**: 🔵 *CLAUDE.md・tech-stack.md より*

- **EditMode テスト**: Core/Model, Core/Systems の全ロジック（カバレッジ80%以上）
  - PlayValidator: 同数字・同色・計算出しの判定テスト
  - ArithmeticValidator: マイナス禁止・0許容のテスト
  - CardDistributor: 配布・余りカード除外テスト
  - WinChecker: 勝利判定テスト
  - TurnManager: パス管理・全員パスリセットテスト
- **PlayMode テスト**: UI連携・ゲームフロー統合テスト

## 技術的制約 🔵

**信頼性**: 🔵 *CLAUDE.md・tech-stack.md より*

- WebGL ビルドのため `System.IO` の一部API使用不可
- WebGL ではマルチスレッド非対応（AI処理はメインスレッドで実行）
- カード画像は既存の JPG ファイル（1890x2835px）を Sprite にインポート（PPU=2000）
- Canvas は Screen Space - Overlay のみ使用

## 関連文書

- **データフロー**: [dataflow.md](dataflow.md)
- **C#型定義**: [interfaces.md](interfaces.md)
- **要件定義**: [requirements.md](../../spec/magical-competition/requirements.md)

## 信頼性レベルサマリー

- 🔵 青信号: 24件 (100%)
- 🟡 黄信号: 0件 (0%)
- 🔴 赤信号: 0件 (0%)

**品質評価**: ✅ 高品質
