# Magical Competition データフロー図

**作成日**: 2026-03-13
**関連アーキテクチャ**: [architecture.md](architecture.md)
**関連要件定義**: [requirements.md](../../spec/magical-competition/requirements.md)

**【信頼性レベル凡例】**:
- 🔵 **青信号**: 要件定義書・ユーザヒアリングを参考にした確実なフロー
- 🟡 **黄信号**: 要件定義書・ユーザヒアリングから妥当な推測によるフロー
- 🔴 **赤信号**: 要件定義書・ユーザヒアリングにない推測によるフロー

---

## ゲーム全体フロー 🔵

**信頼性**: 🔵 *要件定義書・ユーザヒアリングより*

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│ TitleScene  │────→│  GameScene   │────→│ TitleScene  │
│             │     │              │     │  (戻る)     │
│ ・AI人数選択 │     │ ・ゲーム進行  │     │             │
│ ・開始ボタン │     │ ・結果表示    │     │             │
└─────────────┘     └──────────────┘     └─────────────┘
```

## ゲームセットアップ フロー 🔵

**信頼性**: 🔵 *要件定義書 REQ-001〜005 より*

```
Setup フェーズ
│
├─ 1. CardDistributor.CreateDeck()
│     45枚の魔石カード生成（5色×9番号）
│
├─ 2. CardDistributor.Shuffle(deck)
│     デッキをシャッフル
│
├─ 3. CardDistributor.Distribute(deck, playerCount)
│     全プレイヤーに均等配布
│     ├─ 余りカードはゲームから除外（REQ-501）
│     └─ 各プレイヤーのカードを山札として設定
│
├─ 4. DrawSystem.InitialDraw(players)
│     各プレイヤーが山札から3枚引いて手札に（REQ-004）
│
└─ 5. FieldUpdater.SetVirtualField(color=Any, number=5)
      場札を「好きな色の5」として疑似設定（REQ-005）
      ├─ プレイヤーの場合: 色選択UIを表示
      └─ AIの場合: NormalStrategy が色を選択
```

## プレイヤーターン フロー 🔵

**信頼性**: 🔵 *要件定義書 REQ-101〜108・ユーザヒアリングより*

```
Play フェーズ（プレイヤーターン）
│
├─ 1. PlayValidator.GetValidPlays(hand, fieldCard)
│     出せるカードの組み合わせを全列挙
│     ├─ 同数字マッチ（1〜3枚）
│     ├─ 同色マッチ（1〜3枚）
│     └─ 計算出し（2〜3枚、ArithmeticValidator使用）
│
├─ 2. HandView.UpdateCardStates(validPlays)
│     出せないカードをグレーアウト（NFR-102）
│
├─ 3. PlayerInputController.WaitForInput()
│     ├─ カード選択（タップで複数選択可）
│     ├─ 「出す」ボタン → PlayValidator.Validate(selectedCards)
│     │   ├─ 合法 → カードプレイ実行
│     │   └─ 不正 → エラー表示、再選択
│     └─ 「パス」ボタン → パス処理へ
│
├─ 4a. カードプレイ実行
│     ├─ PlayerState.RemoveFromHand(cards)
│     ├─ FieldUpdater.Update(lastCard)  ← 最後の1枚で場札更新（REQ-105）
│     ├─ CardView アニメーション（手札→場札移動）
│     └─ → Draw フェーズへ
│
└─ 4b. パス処理
      ├─ PlayerInputController.SelectCardsToReturn()
      │   手札から山札に戻すカード選択（0枚も可）（REQ-106）
      ├─ PlayerState.ReturnToDeck(cards)
      ├─ TurnManager.RecordPass(currentPlayer)
      ├─ 全員連続パス判定
      │   ├─ YES → AllPassReset フェーズへ
      │   └─ NO  → NextTurn フェーズへ
      └─ → NextTurn or AllPassReset
```

## AIターン フロー 🔵

**信頼性**: 🔵 *要件定義書 REQ-301,302・ユーザヒアリングより*

```
Play フェーズ（AIターン）
│
├─ 1. AIInfoView.ShowThinking()
│     「思考中」表示（REQ-302）
│
├─ 2. AIActionEvaluator.FindAllValidPlays(hand, fieldCard)
│     全有効手の探索（全探索）（EDGE-401）
│     ├─ 同数字マッチ
│     ├─ 同色マッチ
│     └─ 計算出し（2〜3枚の全組み合わせ）
│
├─ 3. NormalStrategy.DecideAction(validPlays, gameState)
│     Normal戦略で最適手を選択
│     ├─ 多く出せる手を優先
│     ├─ 出せる手がなければパス
│     └─ パス時は不要カードを山札に戻す
│
├─ 4. AIController.ExecuteAction(action)
│     ├─ カードプレイ or パス実行
│     ├─ アニメーション（思考遅延含む、合計500ms以内）
│     └─ → Draw or NextTurn
│
└─ 5. AIInfoView.HideThinking()
```

## Draw フェーズ フロー 🔵

**信頼性**: 🔵 *要件定義書 REQ-108,109 より*

```
Draw フェーズ
│
├─ 1. DrawSystem.Refill(player)
│     手札が3枚になるまで山札から引く
│     ├─ 山札に十分なカードあり → 3枚まで補充
│     └─ 山札不足 → 残りを全て引く（3枚未満でも終了）
│
├─ 2. 山札チェック
│     ├─ 山札が空になった → PlayerState.SetReach(true)（REQ-109）
│     └─ DeckView.UpdateCount(), AIInfoView.UpdateReach()
│
└─ → CheckWin フェーズへ
```

## CheckWin フェーズ フロー 🔵

**信頼性**: 🔵 *要件定義書 REQ-110,202 より*

```
CheckWin フェーズ
│
├─ 1. WinChecker.Check(currentPlayer)
│     手札 + 山札 = 0 か判定
│
├─ 2a. 勝利
│     ├─ ScoreCalculator.Calculate(winner, allPlayers)
│     │   他プレイヤーの手札＋山札の合計枚数 = 勝者の得点
│     ├─ ResultDialogView.Show(winner, score)
│     │   ゲーム内ダイアログで結果表示
│     └─ → End フェーズへ
│
└─ 2b. 継続
      └─ → NextTurn フェーズへ
```

## 全員パスリセット フロー 🔵

**信頼性**: 🔵 *要件定義書 REQ-107・ユーザヒアリングより*

```
AllPassReset フェーズ
│
├─ 1. TurnManager.ResetPassCount()
│     連続パスカウンタをリセット
│
├─ 2. FieldUpdater.ClearField()
│     場札をクリア
│
└─ 3. → SelectFieldCard フェーズへ
      場札を「好きな色の5」として疑似設定
      ├─ プレイヤーターン: 色選択UIを表示
      └─ AIターン: NormalStrategy が色を選択
```

## UI操作フロー（カードプレイ） 🔵

**信頼性**: 🔵 *ユーザヒアリングより*

```
プレイヤーのカードプレイ操作
│
├─ 1. 手札カードをタップ → 選択状態（カードが少し上に浮く）
│     ├─ 出せないカード: グレーアウト、タップ不可
│     └─ 複数選択可能（同数字出し・同色出し・計算出し用）
│
├─ 2. 選択カード確認
│     ├─ PlayValidator でリアルタイム妥当性チェック
│     └─ 有効な組み合わせ: 「出す」ボタン活性化
│
├─ 3. 「出す」ボタンタップ → 確定
│     ├─ カードアニメーション（手札→場札移動）
│     └─ 場札更新・手札補充へ
│
└─ 「パス」ボタンタップ → パスフロー
      ├─ 山札に戻すカード選択UI表示（0枚でもOK）
      ├─ 「確定」で山札の底に戻す
      └─ ターン終了
```

## データ更新フロー概要 🔵

**信頼性**: 🔵 *アーキテクチャ設計より*

```
[入力/イベント]
      │
      ▼
[Controller]  ← ゲームフロー制御
      │
      ├──→ [Core/Systems]  ← ルール判定・状態更新
      │         │
      │         ▼
      │    [Core/Model]    ← GameState/PlayerState 更新
      │         │
      ▼         ▼
[Views]  ←── データ読み取り ── [Core/Model]
      │
      ▼
[Canvas UI]  ← 画面に反映
```

## 関連文書

- **アーキテクチャ**: [architecture.md](architecture.md)
- **C#型定義**: [interfaces.md](interfaces.md)
- **要件定義**: [requirements.md](../../spec/magical-competition/requirements.md)

## 信頼性レベルサマリー

- 🔵 青信号: 10件 (100%)
- 🟡 黄信号: 0件 (0%)
- 🔴 赤信号: 0件 (0%)

**品質評価**: ✅ 高品質
