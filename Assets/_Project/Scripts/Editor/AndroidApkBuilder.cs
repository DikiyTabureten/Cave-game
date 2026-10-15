using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CaveGame.EditorTools
{
    /// <summary>
    /// Сборка APK для Android: сцены из Build Settings (MainMenu первой), без Development Build,
    /// вывод в Builds/Android/CaveGame_&lt;версия&gt;.apk. Подпись — debug keystore Unity по умолчанию.
    /// </summary>
    public static class AndroidApkBuilder
    {
        private const string OutputFolder = "Builds/Android";

        [MenuItem("Tools/Cave Game/Build Android APK")]
        public static void BuildMenu()
        {
            Build();
        }

        public static BuildReport Build()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0 || !scenes[0].EndsWith("MainMenu.unity"))
            {
                Debug.LogError("Сборка Android: первой в Build Settings должна быть сцена MainMenu. " +
                               "Запустите Tools/Cave Game/Build All Scenes.");
                return null;
            }

            // APK, а не AAB: его можно поставить на телефон напрямую.
            EditorUserBuildSettings.buildAppBundle = false;

            Directory.CreateDirectory(OutputFolder);
            string path = $"{OutputFolder}/CaveGame_{PlayerSettings.bundleVersion}.apk";

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = path,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                float megabytes = new FileInfo(path).Length / (1024f * 1024f);
                Debug.Log($"Сборка Android готова: {Path.GetFullPath(path)} ({megabytes:F1} МБ, " +
                          $"{summary.totalTime.TotalSeconds:F0} с).");
            }
            else
            {
                Debug.LogError($"Сборка Android не удалась: {summary.result}, ошибок: {summary.totalErrors}.");
            }
            return report;
        }
    }
}
