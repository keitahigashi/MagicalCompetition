using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MagicalCompetition.Views;

namespace MagicalCompetition.Editor
{
    public static class CardSpriteTableBuilder
    {
        // 色名 → (CardColor名, ファイル番号オフセット)
        private static readonly Dictionary<string, (string colorName, int baseNum)> ColorMap =
            new Dictionary<string, (string, int)>
            {
                { "水", ("Water", 10) },
                { "光", ("Light", 20) },
                { "火", ("Fire",  30) },
                { "土", ("Earth", 40) },
                { "風", ("Wind",  50) },
            };

        [MenuItem("Tools/MagicalCompetition/Build CardSpriteTable")]
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

            foreach (var kv in ColorMap)
            {
                var label = kv.Key;
                var (colorName, baseNum) = kv.Value;

                for (int number = 1; number <= 9; number++)
                {
                    int fileNum = baseNum + number - 1;
                    var fileName = $"{fileNum:D3}_omote_カード_魔石_{label}{number}";
                    var path = $"Assets/_Project/Art/card/{fileName}.jpg";
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
            var backPath = "Assets/_Project/Art/card/002_ura_カード_裏面.jpg";
            table.backSprite = AssetDatabase.LoadAssetAtPath<Sprite>(backPath);

            // Resources フォルダに保存
            var resourcesDir = "Assets/_Project/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesDir))
                AssetDatabase.CreateFolder("Assets/_Project", "Resources");

            var assetPath = $"{resourcesDir}/CardSpriteTable.asset";
            AssetDatabase.CreateAsset(table, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"CardSpriteTable built: {table.entries.Count} card sprites + back sprite at {assetPath}");
        }
    }
}
