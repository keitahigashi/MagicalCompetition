# ClaudeCode タスクプロンプト
## Magical Competition — タイトル画面UI実装

---

## 概要

Unity 6 / URP プロジェクトの **既存 Scene** に、「Magical Competition」タイトル画面の UI を uGUI (Canvas) で実装してください。
特に指定が無い部分は既存の動きを踏襲してください
---

## 環境

| 項目 | 値 |
|---|---|
| Unity バージョン | Unity 6 (6000.x) |
| レンダリングパイプライン | URP |
| UI システム | uGUI (Canvas + CanvasScaler) |
| 対象 Scene | 既存 Scene に追加（新規 Scene 作成不要） |

---

## アセットパス

| アセット | パス |
|---|---|
| 背景魔法陣画像 | `Assets\_Project\Art\bg\start_screen.png` |
| フォント（ゴールド英字） | プロジェクト内の既存フォントを使用。なければ Unity デフォルト |

---

## 実装仕様

### 1. Canvas 設定

- Canvas の **Render Mode** は `Screen Space - Overlay`
- **CanvasScaler** の設定：
  - UI Scale Mode: `Scale With Screen Size`
  - Reference Resolution: `1920 x 1080`
  - Match: `0.5`（Width と Height の中間）

---

### 2. 階層構造（Hierarchy）
既存を踏襲

---

### 3. ビジュアル仕様

#### カラー定義

| 要素 | カラー（RGBA） |
|---|---|
| 背景オーバーレイ（不要なら省略） | `#1A0A3A FF`（深い紺紫） |
| タイトル文字色 | `#D4AF37 FF`（ゴールド） |
| サブタイトル文字色 | `#E8D9B0 FF`（薄いゴールドホワイト） |
| パネル背景 | `#2D1B5E CC`（半透明パープル） |
| パネルボーダー | `#D4AF37 FF`（ゴールド） |
| ボタン（◀▶）背景 | `#3D2878 FF`（濃いパープル） |
| ゲーム開始ボタン背景 | `#6A3DB8 FF`（明るめパープル）＋ゴールドボーダー |
| フッターテキスト | `#B8A090 AA`（薄いグレーゴールド） |

#### フォントサイズ（1080p 基準）

| 要素 | サイズ |
|---|---|
| タイトル（Magical Competition） | 80pt |
| サブタイトル（マジカルコンペ） | 28pt |
| ラベル（対戦AI人数） | 24pt |
| AI人数カウント（3体） | 36pt |
| ゲーム開始ボタン | 32pt |
| フッター | 18pt |

---

### 5. ゲーム開始ボタン ロジック

```csharp
// "ゲーム開始" ボタン押下時の処理
// 1. aiCount を PlayerPrefs に保存
// 2. フェードアウトアニメーション（黒・1秒）
// 3. SceneManager.LoadScene("GameScene") でシーン遷移
//    ※ "GameScene" は仮名。実際のシーン名に変更すること。
```

---

### 6. フェードアウト実装

- `Canvas` 直下に `FadePanel`（全画面の黒い Image）を追加
  - 通常時は `alpha = 0`、最前面に配置（Sibling Index 最大）
- `DOTween` が利用可能な場合は `DOFade` を使用
- 利用不可の場合は `Coroutine` + `Mathf.Lerp` で実装

```csharp
// フェードアウト後にシーンロード
IEnumerator FadeAndLoad(string sceneName) {
    // FadePanel の alpha を 0→1 に 1秒かけて変化
    // 完了後 SceneManager.LoadScene(sceneName)
}
```
