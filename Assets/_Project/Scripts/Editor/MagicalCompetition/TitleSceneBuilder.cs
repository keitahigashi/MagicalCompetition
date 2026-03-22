using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using MagicalCompetition.UI;

namespace MagicalCompetition.Editor
{
    public static class TitleSceneBuilder
    {
        [MenuItem("MagicalCompetition/Build TitleScene UI")]
        public static void BuildTitleSceneUI()
        {
            var scenePath = "Assets/_Project/Scenes/TitleScene.unity";
            EditorSceneManager.OpenScene(scenePath);

            // 既存オブジェクトをクリア
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                Object.DestroyImmediate(root);

            // Camera
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener),
                typeof(UnityEngine.Rendering.Universal.UniversalAdditionalCameraData));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.102f, 0.039f, 0.227f); // #1A0A3A
            cam.orthographic = true;

            // TitleUI（Canvas含むUIを全てAwake時に自動生成する）
            var uiGo = new GameObject("TitleUI", typeof(RectTransform));
            var titleUI = uiGo.AddComponent<TitleUI>();

            // start_screen.png を Sprite として _bgSprite にセット
            var bgAssetPath = "Assets/_Project/Art/bg/start_screen.png";
            var importer = AssetImporter.GetAtPath(bgAssetPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }
            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgAssetPath);
            if (bgSprite != null)
            {
                SetPrivateField(titleUI, "_bgSprite", bgSprite);
            }
            else
            {
                Debug.LogWarning("start_screen.png not found or not importable as Sprite.");
            }

            // EventSystem
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("TitleScene UI built successfully!");
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(target, value);
            else
                Debug.LogWarning($"Field '{fieldName}' not found on {target.GetType().Name}");
        }
    }
}
