using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
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

        private readonly LocalizedTexts _texts;
        private readonly ThemePreviews _previews;
        private readonly LoadingScreen _screen;

        public BootFlow(LocalizedTexts texts, ThemePreviews previews, LoadingScreen screen)
        {
            _texts = texts;
            _previews = previews;
            _screen = screen;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            var minimum = UniTask.Delay(TimeSpan.FromSeconds(MinimumSeconds), cancellationToken: cancellation); // no flash when loading is instant
            // Leaving play mode or quitting mid-load cancels the boot. That is not an error, and VContainer would log it as one.
            await Load(
                (1, _ => _texts.Initialize()),
                (1, _ => _previews.Load()),
                (0, _ => minimum),
                (2, progress => SceneManager.LoadSceneAsync(GameScene).ToUniTask(progress, cancellationToken: cancellation)))
                .SuppressCancellationThrow();
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
