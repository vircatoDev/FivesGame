using UnityEditor;
using UnityEditor.Build;

namespace Fives.Editor
{
    public static class WebGLBuild
    {
        private const string DefaultOutputPath = "Builds/WebGL";

        [MenuItem("FivesGame/Build/WebGL")]
        public static void Build()
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                throw new BuildFailedException("Unable to switch the active build target to WebGL.");
            }

            // GitHub Pages cannot send Content-Encoding headers, so the loader decompresses in JS.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;

            PlayerBuild.Run(BuildTarget.WebGL, PlayerBuild.OutputPath(DefaultOutputPath));
        }
    }
}
