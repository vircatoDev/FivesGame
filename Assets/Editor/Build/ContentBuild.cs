using System.IO;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEngine;

namespace Fives.Editor
{
    /// <summary>Theme bundles and the remote catalog, built on their own or as an update for a released player.</summary>
    internal static class ContentBuild
    {
        /// <summary>
        /// A full content build without the player, into ServerData/[BuildTarget]: the first upload, or bundles for the
        /// editor's "Use Existing Build" play mode. A player build makes the same content by itself.
        /// </summary>
        public static void Build()
        {
            AddressableAssetSettings.BuildPlayerContent(out var result);
            if (!string.IsNullOrEmpty(result.Error))
                throw new BuildFailedException(result.Error);

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            Debug.Log($"Content built into {settings.RemoteCatalogBuildPath.GetValue(settings)} in {result.Duration:F1} s");
        }

        /// <summary>
        /// Rebuilds the changed remote bundles and the catalog for a released player, without a new app version. It
        /// needs the content state that the player build saved for the same platform.
        /// </summary>
        public static void Update()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var state = ContentUpdateScript.GetContentStateDataPath(false);
            if (!File.Exists(state))
                throw new BuildFailedException($"No content state at {state}: build and release the player for this platform first.");

            var result = ContentUpdateScript.BuildContentUpdate(settings, state);
            if (!string.IsNullOrEmpty(result.Error))
                throw new BuildFailedException(result.Error);

            Debug.Log($"Content update built into {settings.RemoteCatalogBuildPath.GetValue(settings)}");
        }
    }
}
