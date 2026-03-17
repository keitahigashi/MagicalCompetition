using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using System.IO;

namespace MagicalCompetition.Editor
{
    public static class CardAtlasBuilder
    {
        private const string AtlasPath = "Assets/_Project/Sprites/Cards/CardAtlas.spriteatlas";
        private const string CardFolder = "Assets/_Project/Art/card";

        [MenuItem("MagicalCompetition/Build Card Atlas")]
        public static void BuildCardAtlas()
        {
            // Load or create the SpriteAtlas asset
            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);
            if (atlas == null)
            {
                atlas = new SpriteAtlas();
                AssetDatabase.CreateAsset(atlas, AtlasPath);
                Debug.Log($"[CardAtlasBuilder] Created new SpriteAtlas at {AtlasPath}");
            }

            // Atlas settings
            SpriteAtlasPackingSettings packingSettings = new SpriteAtlasPackingSettings
            {
                blockOffset = 1,
                enableRotation = false,
                enableTightPacking = false,
                padding = 4
            };
            atlas.SetPackingSettings(packingSettings);

            SpriteAtlasTextureSettings textureSettings = new SpriteAtlasTextureSettings
            {
                readable = false,
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear
            };
            atlas.SetTextureSettings(textureSettings);

            // WebGL platform settings
            TextureImporterPlatformSettings webglPlatform = new TextureImporterPlatformSettings
            {
                name = "WebGL",
                overridden = true,
                maxTextureSize = 2048,
                format = TextureImporterFormat.Automatic,
                textureCompression = TextureImporterCompression.Compressed
            };
            atlas.SetPlatformSettings(webglPlatform);

            // Register the card folder as a packable object
            // This includes all sprites within the folder (magestone 010-058 and back 002)
            UnityEngine.Object folderObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(CardFolder);
            if (folderObject != null)
            {
                atlas.Add(new UnityEngine.Object[] { folderObject });
                Debug.Log($"[CardAtlasBuilder] Registered folder '{CardFolder}' to atlas.");
            }
            else
            {
                Debug.LogWarning($"[CardAtlasBuilder] Folder not found: {CardFolder}");
            }

            EditorUtility.SetDirty(atlas);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CardAtlasBuilder] CardAtlas build complete. Path: {AtlasPath}");
        }
    }
}
