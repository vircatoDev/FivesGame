using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Fives.Editor
{
    /// <summary>
    /// Remote content in the editor. The play mode choice is Addressables' own and stays in this machine's Library;
    /// "Download From Server" plays the content built for the active platform and downloads the theme bundles from
    /// GitHub Pages, as a build does.
    /// </summary>
    public static class ContentMenu
    {
        private const string Root = "FivesGame/Content/";
        private const string ServerItem = Root + "Play Mode: Download From Server";
        private const string DatabaseItem = Root + "Play Mode: Asset Database";

        [MenuItem(ServerItem, priority = 1)]
        public static void UseServer()
        {
            UsePlayMode<BuildScriptPackedPlayMode>();
            if (!File.Exists(Path.Combine(Addressables.BuildPath, "settings.json")))
                Debug.LogWarning($"No content is built for {EditorUserBuildSettings.activeBuildTarget}: run {Root}Build Content first.");
        }

        [MenuItem(ServerItem, true)]
        private static bool UseServerCheck() => CheckPlayMode<BuildScriptPackedPlayMode>(ServerItem);

        [MenuItem(DatabaseItem, priority = 2)]
        public static void UseDatabase() => UsePlayMode<BuildScriptFastMode>();

        [MenuItem(DatabaseItem, true)]
        private static bool UseDatabaseCheck() => CheckPlayMode<BuildScriptFastMode>(DatabaseItem);

        /// <summary>Rebuilds the bundles for the active platform; needed after the pictures change. Publish them with tools/unity.sh publish-content.</summary>
        [MenuItem(Root + "Build Content", priority = 20)]
        public static void BuildContent() => ContentBuild.Build();

        [MenuItem(Root + "Build Content", true)]
        private static bool BuildContentCheck() => !EditorApplication.isPlayingOrWillChangePlaymode;

        /// <summary>Forgets the downloaded bundles and the cached remote catalog, so the next play downloads them again.</summary>
        [MenuItem(Root + "Clear Downloaded Content", priority = 21)]
        public static void ClearDownloadedContent()
        {
            var catalogs = Path.Combine(Application.persistentDataPath, "com.unity.addressables");
            if (Directory.Exists(catalogs))
                Directory.Delete(catalogs, true);

            if (Caching.ClearCache())
                Debug.Log("Downloaded theme bundles and the cached catalog are cleared: the next play downloads them again.");
            else
                Debug.LogWarning("Some bundles are still loaded and stay in the cache: stop play mode and clear again.");
        }

        [MenuItem(Root + "Clear Downloaded Content", true)]
        private static bool ClearDownloadedContentCheck() => !EditorApplication.isPlaying;

        private static void UsePlayMode<T>()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            settings.ActivePlayModeDataBuilderIndex = settings.DataBuilders.FindIndex(builder => builder is T);
            Debug.Log($"Addressables play mode: {settings.ActivePlayModeDataBuilder.Name}");
        }

        private static bool CheckPlayMode<T>(string item)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            Menu.SetChecked(item, settings != null && settings.ActivePlayModeDataBuilder is T);
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }
    }
}
