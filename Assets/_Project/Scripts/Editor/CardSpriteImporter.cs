using UnityEditor;
using UnityEngine;
using System.IO;

namespace MagicalCompetition.Editor
{
    public static class CardSpriteImporter
    {
        [MenuItem("MagicalCompetition/Import Card Sprites")]
        public static void ImportCardSprites()
        {
            string cardFolder = "Assets/_Project/Art/card";
            string[] jpgFiles = Directory.GetFiles(cardFolder, "*.jpg");

            int count = 0;
            foreach (string filePath in jpgFiles)
            {
                string assetPath = filePath.Replace("\\", "/");
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 2000;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = false;

                // WebGL最適化設定
                TextureImporterPlatformSettings webglSettings = importer.GetPlatformTextureSettings("WebGL");
                webglSettings.overridden = true;
                webglSettings.maxTextureSize = 1024;
                webglSettings.format = TextureImporterFormat.Automatic;
                webglSettings.textureCompression = TextureImporterCompression.Compressed;
                importer.SetPlatformTextureSettings(webglSettings);

                // Default設定
                TextureImporterPlatformSettings defaultSettings = importer.GetDefaultPlatformTextureSettings();
                defaultSettings.maxTextureSize = 1024;
                defaultSettings.textureCompression = TextureImporterCompression.Compressed;
                importer.SetPlatformTextureSettings(defaultSettings);

                importer.SaveAndReimport();
                count++;
            }

            Debug.Log($"[CardSpriteImporter] {count} card sprites imported successfully.");
        }
    }
}
