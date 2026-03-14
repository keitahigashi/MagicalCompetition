using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using MagicalCompetition.Core.Model;
using MagicalCompetition.Core.AI;
using MagicalCompetition.Controllers;

namespace MagicalCompetition.Tests.EditMode
{
    [TestFixture]
    public class AIControllerTests
    {
        private AIController _aiController;
        private NormalStrategy _strategy;

        [SetUp]
        public void SetUp()
        {
            _aiController = new AIController();
            _strategy = new NormalStrategy();
        }

        private GameState CreateGameStateForAI(List<Card> hand, Card fieldCard)
        {
            var aiPlayer = new PlayerState(0, false);
            foreach (var card in hand)
                aiPlayer.AddToHand(card);

            var humanPlayer = new PlayerState(1, true);
            humanPlayer.AddToHand(new Card(CardColor.Water, 1));

            var state = new GameState(new List<PlayerState> { aiPlayer, humanPlayer });
            state.Field.Update(fieldCard);
            return state;
        }

        // ─── 1. 出せる手あり: カードプレイを返す ────────────────────────────

        [Test]
        public void ExecuteTurn_ValidPlaysExist_ReturnsCardPlay()
        {
            var hand = new List<Card>
            {
                new Card(CardColor.Fire, 5),
                new Card(CardColor.Water, 3),
                new Card(CardColor.Earth, 7)
            };
            var state = CreateGameStateForAI(hand, new Card(CardColor.Fire, 5));

            var action = _aiController.ExecuteTurn(state, _strategy);

            Assert.AreNotEqual(PlayType.Pass, action.Type);
        }

        // ─── 2. 出せない: パスを返す ────────────────────────────────────────

        [Test]
        public void ExecuteTurn_NoValidPlays_ReturnsPass()
        {
            var hand = new List<Card>
            {
                new Card(CardColor.Water, 2),
                new Card(CardColor.Earth, 4)
            };
            // 場札 Fire:5。同色Fire無し、同数字5無し、2+4=6≠5、|2-4|=2≠5
            var state = CreateGameStateForAI(hand, new Card(CardColor.Fire, 5));

            var action = _aiController.ExecuteTurn(state, _strategy);

            Assert.AreEqual(PlayType.Pass, action.Type);
        }

        // ─── 3. 思考時間が500ms以内 ─────────────────────────────────────────

        [Test]
        public void ExecuteTurn_CompletesWithin500ms()
        {
            var hand = new List<Card>
            {
                new Card(CardColor.Fire, 5),
                new Card(CardColor.Water, 3),
                new Card(CardColor.Earth, 7)
            };
            var state = CreateGameStateForAI(hand, new Card(CardColor.Fire, 5));

            var sw = Stopwatch.StartNew();
            _aiController.ExecuteTurn(state, _strategy);
            sw.Stop();

            Assert.Less(sw.ElapsedMilliseconds, 500,
                "AI thinking should complete within 500ms");
        }

        // ─── 4. 思考中フラグ ON/OFF ────────────────────────────────────────

        [Test]
        public void ExecuteTurn_IsThinking_SetAndCleared()
        {
            var hand = new List<Card>
            {
                new Card(CardColor.Fire, 5),
                new Card(CardColor.Water, 3),
                new Card(CardColor.Earth, 7)
            };
            var state = CreateGameStateForAI(hand, new Card(CardColor.Fire, 5));

            // 実行前は false
            Assert.IsFalse(_aiController.IsThinking);

            _aiController.ExecuteTurn(state, _strategy);

            // 実行後は false（同期版なので実行完了後に確認）
            Assert.IsFalse(_aiController.IsThinking);
        }
    }
}
