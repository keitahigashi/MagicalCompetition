# Magical Competition タスク概要

**作成日**: 2026-03-13
**推定工数**: 136時間（17営業日）
**総タスク数**: 34件
**タスク粒度**: 半日（4時間）単位

## 関連文書

- **要件定義書**: [📋 requirements.md](../../spec/magical-competition/requirements.md)
- **設計文書**: [📐 architecture.md](../../design/magical-competition/architecture.md)
- **データフロー**: [🔄 dataflow.md](../../design/magical-competition/dataflow.md)
- **C#型定義**: [📝 interfaces.md](../../design/magical-competition/interfaces.md)
- **コンテキストノート**: [📝 note.md](../../spec/magical-competition/note.md)

## フェーズ構成

| フェーズ | 成果物 | タスク数 | 工数 | ファイル |
|---------|--------|----------|------|----------|
| Phase 1 | Core/Model 全クラス、スプライト・アセット基盤 | 8件 | 32h | [TASK-0001~0008](#phase-1-基盤データモデル構築) |
| Phase 2 | Core/Systems 全クラス（ゲームロジック完成） | 8件 | 32h | [TASK-0009~0016](#phase-2-ゲームロジック実装) |
| Phase 3 | Core/AI + Controllers 全クラス | 8件 | 32h | [TASK-0017~0024](#phase-3-ai制御層実装) |
| Phase 4 | Views + UI + Scenes + WebGLビルド | 10件 | 40h | [TASK-0025~0034](#phase-4-表示層ui統合) |

## タスク番号管理

**使用済みタスク番号**: TASK-0001 ~ TASK-0034
**次回開始番号**: TASK-0035

## 全体進捗

- [x] Phase 1: 基盤・データモデル構築
- [x] Phase 2: ゲームロジック実装
- [x] Phase 3: AI・制御層実装
- [x] Phase 4: 表示層・UI・統合

## マイルストーン

- **M1: データモデル完成**: Core/Model 全クラス + CardData アセット生成完了
- **M2: ゲームロジック完成**: Core/Systems 全クラス + EditMode テスト 80%以上カバレッジ
- **M3: ゲームフロー完成**: AI + Controllers + StateMachine 動作完了
- **M4: リリース準備完了**: 全UI統合 + PlayMode統合テスト + WebGLビルド完了

---

## Phase 1: 基盤・データモデル構築

**工数**: 32時間（8タスク）
**目標**: Core/Model 全クラスの実装とカードアセット基盤の構築
**成果物**: 列挙型、Card、PlayerState、FieldState、GameState、GameConfig、PlayAction、PlayResult + CardDataアセット45枚

### タスク一覧

- [x] [TASK-0001: プロジェクト構造・Assembly Definition・テストインフラ設定](TASK-0001.md) - 4h (DIRECT) 🔵
- [x] [TASK-0002: カードスプライト取り込み・Import設定](TASK-0002.md) - 4h (DIRECT) 🔵
- [x] [TASK-0003: 列挙型（CardColor, GamePhase, PlayType）・Card クラス実装](TASK-0003.md) - 4h (TDD) 🔵
- [x] [TASK-0004: CardData ScriptableObject・エディタ生成ツール](TASK-0004.md) - 4h (DIRECT) 🔵
- [x] [TASK-0005: PlayerState クラス実装](TASK-0005.md) - 4h (TDD) 🔵
- [x] [TASK-0006: FieldState クラス実装](TASK-0006.md) - 4h (TDD) 🔵
- [x] [TASK-0007: GameState・GameConfig クラス実装](TASK-0007.md) - 4h (TDD) 🔵
- [x] [TASK-0008: PlayAction・PlayResult クラス実装](TASK-0008.md) - 4h (TDD) 🔵

### 依存関係

```
TASK-0001 ──→ TASK-0002 ──→ TASK-0004
TASK-0001 ──→ TASK-0003 ──→ TASK-0004
                TASK-0003 ──→ TASK-0005 ──→ TASK-0007
                TASK-0003 ──→ TASK-0006 ──→ TASK-0007
                TASK-0003 ──→ TASK-0008
```

---

## Phase 2: ゲームロジック実装

**工数**: 32時間（8タスク）
**目標**: Core/Systems 全クラスの実装、EditModeテスト80%以上カバレッジ達成
**成果物**: CardDistributor、ArithmeticValidator、PlayValidator、FieldUpdater、DrawSystem、WinChecker、ScoreCalculator、TurnManager

### タスク一覧

- [x] [TASK-0009: CardDistributor 実装](TASK-0009.md) - 4h (TDD) 🔵
- [x] [TASK-0010: ArithmeticValidator 実装](TASK-0010.md) - 4h (TDD) 🔵
- [x] [TASK-0011: PlayValidator - 同数字・同色判定](TASK-0011.md) - 4h (TDD) 🔵
- [x] [TASK-0012: PlayValidator - 計算出し・全有効手探索](TASK-0012.md) - 4h (TDD) 🔵
- [x] [TASK-0013: FieldUpdater 実装](TASK-0013.md) - 4h (TDD) 🔵
- [x] [TASK-0014: DrawSystem 実装](TASK-0014.md) - 4h (TDD) 🔵
- [x] [TASK-0015: WinChecker・ScoreCalculator 実装](TASK-0015.md) - 4h (TDD) 🔵
- [x] [TASK-0016: TurnManager 実装](TASK-0016.md) - 4h (TDD) 🔵

### 依存関係

```
TASK-0003 ──→ TASK-0010 ──→ TASK-0012
TASK-0005 ──→ TASK-0009
TASK-0006 ──→ TASK-0011 ──→ TASK-0012
TASK-0006 ──→ TASK-0013
TASK-0005 ──→ TASK-0014
TASK-0007 ──→ TASK-0015
TASK-0007 ──→ TASK-0016
```

---

## Phase 3: AI・制御層実装

**工数**: 32時間（8タスク）
**目標**: AI戦略 + 全Controller + StateMachine の実装
**成果物**: IAIStrategy、NormalStrategy、AIActionEvaluator、GameStateMachine、GameController、PlayerInputController、AIController、SceneController

### タスク一覧

- [x] [TASK-0017: IAIStrategy・AIActionEvaluator 実装](TASK-0017.md) - 4h (TDD) 🔵
- [x] [TASK-0018: NormalStrategy 実装](TASK-0018.md) - 4h (TDD) 🔵
- [x] [TASK-0019: GameStateMachine 実装](TASK-0019.md) - 4h (TDD) 🔵
- [x] [TASK-0020: GameController - Setup〜Play フェーズ実装](TASK-0020.md) - 4h (TDD) 🔵
- [x] [TASK-0021: GameController - Draw〜End フェーズ実装](TASK-0021.md) - 4h (TDD) 🔵
- [x] [TASK-0022: PlayerInputController 実装](TASK-0022.md) - 4h (TDD) 🔵
- [x] [TASK-0023: AIController 実装](TASK-0023.md) - 4h (TDD) 🔵
- [x] [TASK-0024: SceneController 実装](TASK-0024.md) - 4h (TDD) 🔵

### 依存関係

```
TASK-0012 ──→ TASK-0017 ──→ TASK-0018 ──→ TASK-0023
TASK-0007 ──→ TASK-0019 ──→ TASK-0020 ──→ TASK-0021
TASK-0009 ──→ TASK-0020
TASK-0013 ──→ TASK-0020
TASK-0014 ──→ TASK-0021
TASK-0015 ──→ TASK-0021
TASK-0016 ──→ TASK-0021
TASK-0012 ──→ TASK-0022
TASK-0008 ──→ TASK-0022
TASK-0019 ──→ TASK-0024
```

---

## Phase 4: 表示層・UI・統合

**工数**: 40時間（10タスク）
**目標**: 全View/UIの実装、シーン構築、統合テスト、WebGLビルド完成
**成果物**: CardView、HandView、FieldView、DeckView、AIInfoView、TurnIndicatorView、ResultDialogView、TitleUI、GameUI、TitleScene、GameScene、WebGLビルド

### タスク一覧

- [x] [TASK-0025: CardView 実装](TASK-0025.md) - 4h (TDD) 🔵
- [x] [TASK-0026: HandView 実装](TASK-0026.md) - 4h (TDD) 🔵
- [x] [TASK-0027: FieldView・DeckView 実装](TASK-0027.md) - 4h (TDD) 🔵
- [x] [TASK-0028: AIInfoView・TurnIndicatorView 実装](TASK-0028.md) - 4h (TDD) 🔵
- [x] [TASK-0029: ResultDialogView 実装](TASK-0029.md) - 4h (TDD) 🔵
- [x] [TASK-0030: TitleUI・TitleScene 構築](TASK-0030.md) - 4h (TDD) 🔵
- [x] [TASK-0031: GameUI 実装](TASK-0031.md) - 4h (TDD) 🔵
- [x] [TASK-0032: GameScene レイアウト構築・統合](TASK-0032.md) - 4h (DIRECT) 🔵
- [x] [TASK-0033: PlayMode 統合テスト](TASK-0033.md) - 4h (TDD) 🔵
- [x] [TASK-0034: WebGL ビルド・最適化](TASK-0034.md) - 4h (DIRECT) 🔵

### 依存関係

```
TASK-0004 ──→ TASK-0025 ──→ TASK-0026 ──→ TASK-0032
                TASK-0025 ──→ TASK-0027 ──→ TASK-0032
                TASK-0025 ──→ TASK-0028 ──→ TASK-0032
                TASK-0025 ──→ TASK-0029
TASK-0022 ──→ TASK-0031 ──→ TASK-0032
TASK-0024 ──→ TASK-0030 ──→ TASK-0033
TASK-0032 ──→ TASK-0033 ──→ TASK-0034
```

---

## 信頼性レベルサマリー

### 全タスク統計

- **総タスク数**: 34件
- 🔵 **青信号**: 34件 (100%)
- 🟡 **黄信号**: 0件 (0%)
- 🔴 **赤信号**: 0件 (0%)

### フェーズ別信頼性

| フェーズ | 🔵 青 | 🟡 黄 | 🔴 赤 | 合計 |
|---------|-------|-------|-------|------|
| Phase 1 | 8 | 0 | 0 | 8 |
| Phase 2 | 8 | 0 | 0 | 8 |
| Phase 3 | 8 | 0 | 0 | 8 |
| Phase 4 | 10 | 0 | 0 | 10 |

**品質評価**: ✅ 高品質

### タスクタイプ別統計

| タイプ | 件数 | 工数 |
|-------|------|------|
| TDD | 28件 | 112h |
| DIRECT | 6件 | 24h |
| **合計** | **34件** | **136h** |

## クリティカルパス

```
TASK-0001 → TASK-0003 → TASK-0006 → TASK-0011 → TASK-0012 → TASK-0017 → TASK-0018 → TASK-0023
                                                                                        ↓
TASK-0001 → TASK-0003 → TASK-0005 → TASK-0007 → TASK-0019 → TASK-0020 → TASK-0021 → TASK-0032 → TASK-0033 → TASK-0034
                                                                                        ↑
TASK-0001 → TASK-0002 → TASK-0004 → TASK-0025 → TASK-0026 ──────────────────────────────┘
```

**クリティカルパス工数**: 48時間（12タスク）
**並行作業可能**: Phase内の独立タスクは並行実行可能

## 次のステップ

タスクを実装するには:
- 全タスク順番に実装: `/tsumiki:kairo-implement`
- 特定タスクを実装: `/tsumiki:kairo-implement TASK-0001`
