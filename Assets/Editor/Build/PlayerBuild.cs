using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Fives.Editor
{
    internal static class PlayerBuild
    {
        public static string OutputPath(string defaultPath)
        {
            return GetCommandLineValue("-buildPath") ?? defaultPath;
        }

        public static void Run(BuildTarget target, string outputPath, BuildOptions options = BuildOptions.None)
        {
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                throw new BuildFailedException("No enabled scenes are configured in EditorBuildSettings.");
            }

            var outputDirectory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = target,
                options = options
            });

            var summary = report.summary;
            Debug.Log($"{target} build result: {summary.result}; size: {summary.totalSize}; time: {summary.totalTime}; output: {outputPath}");

            if (summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"{target} build failed with {summary.totalErrors} errors.");
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
