# プロジェクト技術スタック定義

## 生成情報
- **生成日**: 2026-03-13
- **生成ツール**: init-tech-stack
- **プロジェクトタイプ**: Unityゲーム（2Dカードゲーム）
- **チーム規模**: 個人開発
- **開発期間**: プロトタイプ/MVP (1-2ヶ月)

## プロジェクト要件サマリー
- **パフォーマンス**: 軽負荷（スタンドアロン, 60fps）
- **ターゲット**: WebGL（ブラウザプレイ）
- **技術スキル**: C#/.NET, Unityエンジン
- **学習コスト許容度**: バランス重視
- **予算**: コスト最小化

## ゲームエンジン
- **エンジン**: Unity 6.3 (6000.1.x)
- **レンダリング**: Built-in Render Pipeline（2Dカードゲームのため軽量パイプラインで十分）

### 選択理由
- 最新の安定版で最新機能を活用可能
- WebGL ビルドサポート
- C# 9+ サポート

## 言語
- **言語**: C# 9.0+
- **ランタイム**: .NET Standard 2.1

## アーキテクチャ
- **パターン**: MVC + State Machine + ScriptableObject
- **依存方向**: 一方向（循環参照禁止）

```
Controllers ──→ Core/Systems
Controllers ──→ Views
Views       ──→ Core/Model（読み取り専用）
Views       ──→ Controllers（Eventのみ）

Core/Systems → Views   （禁止）
Core/Model  → UnityEngine（CardData 除く）
```

### 選択理由
- シンプルで理解しやすい構成
- 個人開発・MVP向けのオーバーヘッド最小構成
- ScriptableObject によるデータ駆動設計
- State Machine でゲームフェーズを明確に制御

## UI
- **UIシステム**: uGUI (Canvas)
- **描画方式**: UI Image（Canvas内で全カード・UI要素を管理）
- **レイアウト**: LayoutGroup + RectTransform によるレスポンシブ配置
- **Canvas設定**: Screen Space - Overlay

### 選択理由
- 2Dカードゲームに最適（ワールド空間SpriteRenderer不要）
- RectTransform による柔軟なレイアウト（解像度・アスペクト比対応）
- LayoutGroup で手札の自動配置
- WebGL での安定動作

## データ管理
- **静的データ**: ScriptableObject（カードデータ、AI設定、ルール設定）
- **ランタイムデータ**: Plain C# クラス（GameState, PlayerState）
- **永続化**: PlayerPrefs（設定・スコア保存）

### 設計方針
- ScriptableObject でマスターデータを管理（Inspector で編集可能）
- ゲーム中のステートは純粋な C# クラスで保持（テスタブル）
- データベース不要（スタンドアロンゲーム）

## 開発環境
- **エディタ**: Unity 6.3
- **IDE**: Visual Studio 2022 / Rider / VS Code
- **バージョン管理**: Git
- **Asset Serialization**: Force Text（差分管理可能）

### テストフレームワーク
- **EditMode テスト**: Unity Test Framework (NUnit 3.x)
  - 対象: Core/Model, Core/Systems（ゲームロジック）
- **PlayMode テスト**: Unity Test Framework
  - 対象: UI連携・ゲームフロー統合テスト

### リンター
- **コード分析**: Roslyn Analyzers（Unity推奨）
- **コーディング規約**: .editorconfig による統一

## ビルド・デプロイ
- **ビルドターゲット**: WebGL
- **圧縮**: Brotli（推奨）/ Gzip
- **ホスティング候補**: itch.io / GitHub Pages / unityroom（無料）

## 品質基準
- **テストカバレッジ**: Core/Systems のロジックは 80% 以上
- **フレームレート**: 60fps 安定
- **ビルドサイズ**: WebGL で可能な限り軽量化
- **動作確認ブラウザ**: Chrome, Firefox, Edge

## 推奨ディレクトリ構造

```
Assets/_Project/
├── Animations/
├── Art/                  ← 元素材（既存）
│   ├── card/
│   ├── box/
│   ├── manual/
│   └── booth_thumbnail/
├── Audio/
│   ├── BGM/
│   └── SE/
├── Data/
│   ├── Cards/            ← CardData .asset ファイル
│   ├── Rules/            ← RuleConfig.asset
│   └── AI/               ← AIConfig.asset
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
│   │   ├── Model/        ← Unity非依存データクラス
│   │   ├── Systems/      ← ゲームロジック
│   │   └── AI/           ← AI制御
│   ├── Controllers/      ← MonoBehaviour系コントローラー
│   ├── Views/            ← 表示・アニメーション
│   ├── UI/               ← 画面遷移・ダイアログ
│   ├── Utils/            ← 共通ユーティリティ
│   └── Editor/           ← エディタ拡張
├── Sprites/              ← ゲーム用にリサイズしたスプライト
│   ├── Cards/
│   ├── UI/
│   └── Effects/
└── Tests/
    ├── EditMode/
    └── PlayMode/
```

## セットアップ手順

### 1. Unity プロジェクト設定
- Unity Hub から Unity 6.3 をインストール
- プロジェクトを開く
- Build Settings > WebGL に切り替え

### 2. 主要コマンド（Unity エディタ操作）
- **再生**: Ctrl+P
- **テスト実行**: Window > General > Test Runner
- **ビルド**: File > Build Settings > Build

## 更新履歴
- 2026-03-13: 初回生成 (init-tech-stack により自動生成)
