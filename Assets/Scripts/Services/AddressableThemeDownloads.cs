using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fives.Configs;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Fives.Services
{
    /// <summary>
    /// Theme bundles through Addressables. Downloaded bundles stay in the device cache (IndexedDB in a browser),
    /// so a theme downloads once; in the editor's asset database mode nothing needs downloading.
    /// </summary>
    public sealed class AddressableThemeDownloads : IThemeDownloads
    {
        public async UniTask Download(IReadOnlyList<ThemeConfig> themes, IProgress<float> progress, CancellationToken cancellation)
        {
            var download = Addressables.DownloadDependenciesAsync(Keys(themes), Addressables.MergeMode.Union);
            try
            {
                while (!download.IsDone)
                {
                    progress?.Report(download.GetDownloadStatus().Percent);
                    await UniTask.Yield(cancellation);
                }

                if (download.Status != AsyncOperationStatus.Succeeded)
                    throw download.OperationException ?? new InvalidOperationException("The themes did not download.");
                progress?.Report(1f);
            }
            finally
            {
                Addressables.Release(download);
            }
        }

        // A theme's pictures share one bundle, so their keys lead to it.
        private static IEnumerable<object> Keys(IEnumerable<ThemeConfig> themes) =>
            themes.SelectMany(theme => theme.Puzzles).Select(puzzle => puzzle.Image.RuntimeKey);
    }
}
