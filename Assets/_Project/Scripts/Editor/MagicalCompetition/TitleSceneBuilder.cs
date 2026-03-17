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
            var camGo = new GameObject("Main Camera", typeof(Camera),
                typeof(UnityEngine.Rendering.Universal.UniversalAdditionalCameraData));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.06f, 0.20f);
            cam.orthographic = true;

            // TitleUI（Canvas含むUIを全てAwake時に自動生成する）
            var uiGo = new GameObject("TitleUI", typeof(RectTransform));
            uiGo.AddComponent<TitleUI>();

            // EventSystem
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("TitleScene UI built successfully!");
        }
    }
}
