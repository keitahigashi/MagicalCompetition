using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System.IO;

namespace MagicalCompetition.Editor
{
    public static class WebGLBuildConfig
    {
        [MenuItem("Tools/MagicalCompetition/Configure WebGL Settings")]
        public static void ConfigureWebGLSettings()
        {
            // プラットフォーム切り替え確認
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                Debug.Log("Switching build target to WebGL...");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            }

            // Player Settings
            PlayerSettings.companyName = "MagicalCompetition";
            PlayerSettings.productName = "Magical Competition";

            // WebGL固有設定
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.runInBackground = false;

            // ストリッピング設定
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.Medium);

            // 解像度設定
            PlayerSettings.defaultWebScreenWidth = 1920;
            PlayerSettings.defaultWebScreenHeight = 1080;

            Debug.Log("WebGL settings configured successfully!");
            Debug.Log($"  Compression: Brotli");
            Debug.Log($"  Exception Support: Explicitly Thrown Only");
            Debug.Log($"  Strip Engine Code: Enabled");
            Debug.Log($"  Managed Stripping: Medium");
            Debug.Log($"  Data Caching: Enabled");
        }

        [MenuItem("Tools/MagicalCompetition/Build WebGL")]
        public static void BuildWebGL()
        {
            ConfigureWebGLSettings();

            var buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Build/WebGL");

            if (!Directory.Exists(buildPath))
                Directory.CreateDirectory(buildPath);

            var scenes = new[]
            {
                "Assets/_Project/Scenes/TitleScene.unity",
                "Assets/_Project/Scenes/GameScene.unity"
            };

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            Debug.Log($"Starting WebGL build to: {buildPath}");
            var report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                var totalSize = report.summary.totalSize;
                Debug.Log($"WebGL build succeeded! Total size: {totalSize / (1024 * 1024f):F2} MB");

                if (totalSize > 30 * 1024 * 1024)
                    Debug.LogWarning($"Build size exceeds 30MB target! Consider optimizing assets.");
                else
                    Debug.Log("Build size is within 30MB target.");
            }
            else
            {
                Debug.LogError($"WebGL build failed: {report.summary.result}");
                foreach (var step in report.steps)
                {
                    foreach (var msg in step.messages)
                    {
                        if (msg.type == LogType.Error)
                            Debug.LogError($"  {msg.content}");
                    }
                }
            }
        }
    }
}
