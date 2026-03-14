using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using MagicalCompetition.Core.Model;

namespace MagicalCompetition.Editor
{
    public static class CardDataGenerator
    {
        // colorPrefix は 2 桁の文字列。実際のファイルプレフィックスは
        // int.Parse(colorPrefix) * 10 + (number - 1) を D3 ゼロ埋めで生成する。
        // 例: Water("01"), number=1 → 01*10+0=10 → "010"
        //     Fire("03"),  number=9 → 03*10+8=38 → "038"
        private static readonly (CardColor color, string colorPrefix, string jaName)[] ColorMappings =
        {
            (CardColor.Water, "01", "水"),
            (CardColor.Light, "02", "光"),
            (CardColor.Fire,  "03", "火"),
            (CardColor.Earth, "04", "土"),
            (CardColor.Wind,  "05", "風"),
        };

        [MenuItem("MagicalCompetition/Generate Card Data Assets")]
        public static void GenerateCardDataAssets()
        {
            string cardArtPath = "Assets/_Project/Art/card";
            string outputPath  = "Assets/_Project/Data/Cards";

            // 出力ディレクトリ確認・作成
            if (!AssetDatabase.IsValidFolder(outputPath))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Cards");
            }

            // 裏面スプライト読み込み（002_ura_カード_裏面.jpg）
            Sprite backSprite = FindSpriteByPrefix(cardArtPath, "002");
            if (backSprite == null)
            {
                Debug.LogWarning("[CardDataGenerator] Back sprite (prefix '002') not found. backSprite will be null.");
            }

            int count = 0;
            foreach (var (color, colorPrefix, jaName) in ColorMappings)
            {
                for (int number = 1; number <= 9; number++)
                {
                    // 例: Water("01"), number=1 → 10 → "010"
                    //     Water("01"), number=9 → 18 → "018"
                    int prefixNumber = int.Parse(colorPrefix) * 10 + (number - 1);
                    string spritePrefix = prefixNumber.ToString("D3");

                    Sprite cardSprite = FindSpriteByPrefix(cardArtPath, spritePrefix);
                    if (cardSprite == null)
                    {
                        Debug.LogWarning($"[CardDataGenerator] Card sprite not found for prefix '{spritePrefix}' ({color} {number}).");
                    }

                    string assetPath = $"{outputPath}/{color}_{number}.asset";

                    CardData cardData = AssetDatabase.LoadAssetAtPath<CardData>(assetPath);
                    if (cardData == null)
                    {
                        cardData = ScriptableObject.CreateInstance<CardData>();
                        AssetDatabase.CreateAsset(cardData, assetPath);
                    }

                    cardData.color       = color;
                    cardData.number      = number;
                    cardData.cardSprite  = cardSprite;
                    cardData.backSprite  = backSprite;

                    EditorUtility.SetDirty(cardData);
                    count++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CardDataGenerator] {count} CardData assets generated successfully.");
        }

        /// <summary>
        /// 指定フォルダ内で指定プレフィックスで始まる最初の JPG ファイルを Sprite として返す。
        /// ファイルが存在しない場合は null を返す。
        /// </summary>
        private static Sprite FindSpriteByPrefix(string folder, string prefix)
        {
            string[] files = Directory.GetFiles(folder, $"{prefix}*.jpg");
            if (files.Length == 0) return null;

            // パス区切りを Unity 形式に統一
            string assetPath = files[0].Replace("\\", "/");
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }
    }
}
