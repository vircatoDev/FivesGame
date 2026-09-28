using System.Threading;
using Cysharp.Threading.Tasks;

namespace Fives.UI
{
    /// <summary>The loading screen as the boot uses it: the progress, and a retry when the themes do not download.</summary>
    public interface ILoadingScreen
    {
        void SetProgress(float value);

        /// <summary>Shows that the download failed and completes when the player presses Retry.</summary>
        UniTask WaitForRetry(CancellationToken cancellation);
    }
}
