using UnityEditor;
using UnityEditor.SceneManagement;

namespace Fives.Editor
{
    /// <summary>
    /// Play mode always starts from the Boot scene, as a build does: the game scene's scope needs the app scope,
    /// which only Boot creates. Pressing Play in any open scene still boots first, then loads the game scene.
    /// </summary>
    [InitializeOnLoad]
    internal static class BootScenePlayMode
    {
        private const string BootScene = "Assets/Scenes/Boot.unity";

        static BootScenePlayMode() =>
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootScene);
    }
}
