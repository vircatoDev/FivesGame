using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Scripts.Configs;
using Scripts.Services;
using UnityEngine;

namespace Scripts.UI
{
    /// <summary>The loading screen as menus use it for downloads: progress, and a choice between retrying and going back.</summary>
    public interface IDownloadScreen
    {
        /// <summary>Covers the screen with the progress at zero.</summary>
        void Show();

        void SetProgress(float value);

        /// <summary>Shows that the download failed; true when the player retries, false when they go back.</summary>
        UniTask<bool> AskRetry(CancellationToken cancellation);

        UniTask Hide();
    }

    /// <summary>
    /// Makes sure a theme is on the device before a menu opens or starts it. The boot downloads every theme, so a theme
    /// normally passes at once; one that missed it, because the game started offline, downloads here with the loading
    /// screen's progress, and after a failure the player may retry or go back.
    /// </summary>
    public sealed class ThemeDownloadGate
    {
        private readonly IThemeDownloads _downloads;
        private readonly IDownloadScreen _screen;

        public ThemeDownloadGate(IThemeDownloads downloads, IDownloadScreen screen)
        {
            _downloads = downloads;
            _screen = screen;
        }

        /// <summary>True once the theme is on the device, false when the player gave up.</summary>
        public async UniTask<bool> Ensure(ThemeConfig theme, CancellationToken cancellation)
        {
            if (await _downloads.IsDownloaded(theme))
                return true;

            _screen.Show();
            try
            {
                while (true)
                {
                    try
                    {
                        await _downloads.Download(new[] { theme }, Progress.Create<float>(_screen.SetProgress), cancellation);
                        return true;
                    }
                    catch (Exception exception) when (!(exception is OperationCanceledException))
                    {
                        Debug.LogWarning($"Theme {theme.Id} did not download. {exception.Message}");
                        if (!await _screen.AskRetry(cancellation))
                            return false;
                    }
                }
            }
            finally
            {
                _screen.Hide().Forget();
            }
        }
    }
}
