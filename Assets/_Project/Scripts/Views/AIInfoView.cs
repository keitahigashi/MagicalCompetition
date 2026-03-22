using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Views
{
    /// <summary>
    /// AIプレイヤーの情報表示コンポーネント。
    /// AI名・手札（裏面カード）・山札枚数・リーチ状態・思考中表示を管理する。
    /// </summary>
    public class AIInfoView : MonoBehaviour
    {
        [SerializeField] private Text _aiNameText;
        [SerializeField] private Text _handCountText;
        [SerializeField] private Text _deckCountText;
        [SerializeField] private Text _totalCountText;
        [SerializeField] private GameObject _reachIcon;
        [SerializeField] private GameObject _thinkingIcon;
        [SerializeField] private Transform _handContainer;

        private readonly List<GameObject> _cardBacks = new List<GameObject>();
        private Sprite _backSprite;

        /// <summary>AIプレイヤーの情報を更新する。</summary>
        public void UpdateInfo(PlayerState aiPlayer)
        {
            if (_aiNameText != null)
                _aiNameText.text = $"AI{aiPlayer.PlayerId}";

            if (_handCountText != null)
                _handCountText.text = $"{aiPlayer.Hand.Count}枚";

            if (_deckCountText != null)
                _deckCountText.text = $"山札:{aiPlayer.Deck.Count}";

            // トータルカード数（手札＋山札）表示
            if (_totalCountText != null)
            {
                int totalCards = aiPlayer.Hand.Count + aiPlayer.Deck.Count;
                _totalCountText.text = $"計:{totalCards}枚";
                // 残少時（5枚以下）に警告色で強調
                _totalCountText.color = totalCards <= 5
                    ? new Color(1f, 0.42f, 0.42f) // #FF6B6B
                    : new Color(0.7f, 0.7f, 0.7f);
            }

            if (_reachIcon != null)
                _reachIcon.SetActive(aiPlayer.IsReach);

            UpdateHandCards(aiPlayer.Hand.Count);
        }

        /// <summary>手札枚数に応じて裏面カードの表示数を更新する。</summary>
        private void UpdateHandCards(int count)
        {
            if (_handContainer == null) return;

            if (_backSprite == null)
                _backSprite = CardSpriteLoader.GetBackSprite();

            // 現在の表示数と合わせる
            while (_cardBacks.Count < count)
            {
                var go = new GameObject($"CardBack{_cardBacks.Count}", typeof(RectTransform));
                go.transform.SetParent(_handContainer, false);

                var img = go.AddComponent<Image>();
                if (_backSprite != null)
                {
                    img.sprite = _backSprite;
                    img.color = Color.white;
                }
                else
                {
                    img.color = new Color(0.3f, 0.3f, 0.6f);
                }
                // AspectRatioFitter で縦に合わせた幅を自動算出
                var arf = go.AddComponent<AspectRatioFitter>();
                arf.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                arf.aspectRatio = 0.66f; // カード比率 2:3

                _cardBacks.Add(go);
            }

            while (_cardBacks.Count > count)
            {
                var last = _cardBacks[_cardBacks.Count - 1];
                _cardBacks.RemoveAt(_cardBacks.Count - 1);
                Destroy(last);
            }
        }

        /// <summary>手札エリアのワールド座標を返す（アニメーション元）。</summary>
        public Vector3 HandWorldPosition
        {
            get
            {
                if (_handContainer != null)
                    return _handContainer.position;
                return transform.position;
            }
        }

        /// <summary>山札テキストのワールド座標を返す（パス時の移動先）。</summary>
        public Vector3 DeckWorldPosition
        {
            get
            {
                if (_deckCountText != null)
                    return _deckCountText.transform.position;
                return transform.position;
            }
        }

        /// <summary>手札裏面カードを指定枚数だけ即座に減らす（アニメーション用）。</summary>
        public void RemoveCardBacks(int count)
        {
            for (int i = 0; i < count && _cardBacks.Count > 0; i++)
            {
                var last = _cardBacks[_cardBacks.Count - 1];
                _cardBacks.RemoveAt(_cardBacks.Count - 1);
                Destroy(last);
            }
        }

        /// <summary>手札裏面カードを指定枚数だけ追加する（補充アニメーション用）。</summary>
        public void AddCardBacks(int count)
        {
            if (_handContainer == null) return;
            if (_backSprite == null)
                _backSprite = CardSpriteLoader.GetBackSprite();

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"CardBack{_cardBacks.Count}", typeof(RectTransform));
                go.transform.SetParent(_handContainer, false);

                var img = go.AddComponent<Image>();
                if (_backSprite != null)
                {
                    img.sprite = _backSprite;
                    img.color = Color.white;
                }
                else
                {
                    img.color = new Color(0.3f, 0.3f, 0.6f);
                }

                var arf = go.AddComponent<AspectRatioFitter>();
                arf.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
                arf.aspectRatio = 0.66f;

                _cardBacks.Add(go);
            }
        }

        /// <summary>最後の手札裏面カードのワールド座標を返す。</summary>
        public Vector3 LastCardBackWorldPosition
        {
            get
            {
                if (_cardBacks.Count > 0)
                    return _cardBacks[_cardBacks.Count - 1].transform.position;
                return HandWorldPosition;
            }
        }

        /// <summary>思考中アイコンを表示する。</summary>
        public void ShowThinking()
        {
            if (_thinkingIcon != null)
                _thinkingIcon.SetActive(true);
        }

        /// <summary>思考中アイコンを非表示にする。</summary>
        public void HideThinking()
        {
            if (_thinkingIcon != null)
                _thinkingIcon.SetActive(false);
        }
    }
}
