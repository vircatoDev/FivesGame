using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Scripts.Configs;
using Scripts.Services;
using Scripts.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Scripts.Boot
{
    /// <summary>
    /// Entry point of the Boot scene: prepares what the first screens read synchronously, then loads the game scene.
    /// The loading screen stays up until GameStartup has started the game.
    /// </summary>
    public sealed class BootFlow : IAsyncStartable
    {
        private const string GameScene = "MainGame";
        private const float MinimumSeconds = 0.5f;

        private readonly RemoteBalance _balance;
        private readonly Func<LocalizedTexts> _texts;
        private readonly ThemePreviews _previews;
        private readonly IThemeDownloads _downloads;
        private readonly GlobalConfig _config;
        private readonly LoadingScreen _screen;

        /// <param name="texts">Created only after the balance is loaded: the texts read the save, and a new save takes its starting values from the balance.</param>
        public BootFlow(RemoteBalance balance, Func<LocalizedTexts> texts, ThemePreviews previews, IThemeDownloads downloads,
            GlobalConfig config, LoadingScreen screen)
        {
            _balance = balance;
            _texts = texts;
            _previews = previews;
            _downloads = downloads;
            _config = config;
            _screen = screen;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            var minimum = UniTask.Delay(TimeSpan.FromSeconds(MinimumSeconds), cancellationToken: cancellation); // no flash when loading is instant
            // Leaving play mode or quitting mid-load cancels the boot. That is not an error, and VContainer would log it as one.
            await Load(
                (1, _ => _balance.Load(cancellation)),
                (1, _ => _texts().Initialize()),
                (1, _ => _previews.Load()),
                (3, progress => DownloadThemes(progress, cancellation)),
                (0, _ => minimum),
                (2, progress => SceneManager.LoadSceneAsync(GameScene).ToUniTask(progress, cancellationToken: cancellation)))
                .SuppressCancellationThrow();
        }

        // Every theme downloads here, so the menus open themes without waiting. Offline, the game starts anyway: the
        // themes already on the device work, and ThemeDownloadGate retries the others when a menu opens one.
        private async UniTask DownloadThemes(IProgress<float> progress, CancellationToken cancellation)
        {
            try
            {
                await _downloads.Download(_config.Themes, progress, cancellation);
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                Debug.LogWarning($"The themes did not download at start; the menus will retry. {exception.Message}");
            }
        }

        // Runs the steps in order and fills the bar by each step's weight, including progress inside a step.
        // A failed step is logged and skipped, so its feature falls back to defaults instead of blocking the game.
        private async UniTask Load(params (float Weight, Func<IProgress<float>, UniTask> Step)[] steps)
        {
            var total = steps.Sum(step => step.Weight);
            var done = 0f;
            foreach (var (weight, step) in steps)
            {
                var start = done;
                try
                {
                    await step(Progress.Create<float>(value => _screen.SetProgress((start + value * weight) / total)));
                }
                catch (Exception exception) when (!(exception is OperationCanceledException))
                {
                    Debug.LogException(exception);
                }

                done += weight;
                _screen.SetProgress(done / total);
            }
        }
    }
}
