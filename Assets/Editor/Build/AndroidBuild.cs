using System;
using UnityEditor;
using UnityEditor.Build;

namespace Fives.Editor
{
    public static class AndroidBuild
    {
        private const string DefaultOutputPath = "Builds/Android/FivesGame.apk";

        [MenuItem("FivesGame/Build/Android APK")]
        public static void BuildFromMenu()
        {
            BuildRelease();
        }

        public static void BuildRelease()
        {
            var outputPath = PlayerBuild.OutputPath(DefaultOutputPath);
            var appBundle = outputPath.EndsWith(".aab", StringComparison.OrdinalIgnoreCase);
            if (!appBundle && !outputPath.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
            {
                throw new BuildFailedException("Android builds must use an .apk or .aab output path.");
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
            EditorUserBuildSettings.buildAppBundle = appBundle;

            PlayerBuild.Run(BuildTarget.Android, outputPath, BuildOptions.CleanBuildCache);
        }
    }
}
