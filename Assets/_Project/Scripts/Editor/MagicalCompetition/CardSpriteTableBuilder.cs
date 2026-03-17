using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MagicalCompetition.Views;

namespace MagicalCompetition.Editor
{
    public static class CardSpriteTableBuilder
    {
        private static readonly (string colorName, string fileLabel, int baseNum)[] Colors =
        {
            ("Water", "water", 10),
            ("Light", "light", 20),
            ("Fire",  "fire",  30),
            ("Earth", "earth", 40),
            ("Wind",  "wind",  50),
        };

        [MenuItem("MagicalCompetition/Build CardSpriteTable")]
        public static void Build()
        {
            // Art/card 内の画像のテクスチャタイプを Sprite に一括変更
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Project/Art/card" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.SaveAndReimport();
                }
            }

            // CardSpriteTable アセットを作成
            var table = ScriptableObject.CreateInstance<CardSpriteTable>();
            table.entries = new List<CardSpriteTable.Entry>();

            foreach (var (colorName, fileLabel, baseNum) in Colors)
            {
                for (int number = 1; number <= 9; number++)
                {
                    int fileNum = baseNum + number - 1;
                    var path = $"Assets/_Project/Art/card/{fileNum:D3}_omote_card_stone_{fileLabel}{number}.jpg";
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                    if (sprite == null)
                    {
                        Debug.LogWarning($"Sprite not found: {path}");
                        continue;
                    }

                    table.entries.Add(new CardSpriteTable.Entry
                    {
                        key = $"{colorName}_{number}",
                        sprite = sprite
                    });
                }
            }

            // 裏面
            var backPath = "Assets/_Project/Art/card/002_ura_card_back.jpg";
            table.backSprite = AssetDatabase.LoadAssetAtPath<Sprite>(backPath);
            if (table.backSprite == null)
                Debug.LogWarning($"Back sprite not found: {backPath}");

            // Resources フォルダに保存
            var resourcesDir = "Assets/_Project/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesDir))
                AssetDatabase.CreateFolder("Assets/_Project", "Resources");

            var assetPath = $"{resourcesDir}/CardSpriteTable.asset";
            AssetDatabase.CreateAsset(table, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"CardSpriteTable built: {table.entries.Count} card sprites, backSprite={table.backSprite != null} at {assetPath}");
        }
    }
}
