using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fives.Editor
{
    public static class AndroidBuild
    {
        private const string DefaultOutputPath = "Builds/Android/FivesGame.aab";

        [MenuItem("FivesGame/Build/Android App Bundle")]
        public static void BuildFromMenu()
        {
            BuildRelease();
        }

        public static void BuildRelease()
        {
            var outputPath = GetCommandLineValue("-buildPath") ?? DefaultOutputPath;
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new BuildFailedException("No enabled scenes are configured in EditorBuildSettings.");
            }

            if (!outputPath.EndsWith(".aab", StringComparison.OrdinalIgnoreCase))
            {
                throw new BuildFailedException("Android store builds must use an .aab output path.");
            }

            var outputDirectory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                throw new BuildFailedException("Unable to switch the active build target to Android.");
            }

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.vircatodev.fivesgame");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            EditorUserBuildSettings.buildAppBundle = true;

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.CleanBuildCache
            });

            var summary = report.summary;
            Debug.Log($"Android build result: {summary.result}; size: {summary.totalSize}; time: {summary.totalTime}; output: {outputPath}");

            if (summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"Android build failed with {summary.totalErrors} errors.");
            }
        }

        private static string GetCommandLineValue(string key)
        {
            var args = Environment.GetCommandLineArgs();
            for (var index = 0; index < args.Length - 1; index++)
            {
                if (string.Equals(args[index], key, StringComparison.Ordinal))
                {
                    return args[index + 1];
                }
            }

            return null;
        }
    }
}
