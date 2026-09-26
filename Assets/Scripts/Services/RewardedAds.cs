using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Scripts.Services
{
    /// <summary>A rewarded ad for the doubled result reward.</summary>
    public interface IRewardedAds
    {
        /// <summary>False where the game shows no ads (WebGL, no ad keys): the x2 button is hidden.</summary>
        bool IsSupported { get; }

        /// <summary>An ad is loaded and can be shown now.</summary>
        bool IsReady { get; }

        /// <summary>Raised when <see cref="IsReady"/> changes.</summary>
        event Action ReadyChanged;

        /// <summary>Shows the ad; true only when the player watched it to its reward.</summary>
        UniTask<bool> Show(CancellationToken cancellation);
    }

    /// <summary>Platforms without ads: the doubled reward is not offered.</summary>
    public sealed class NoRewardedAds : IRewardedAds
    {
        public bool IsSupported => false;
        public bool IsReady => false;

        public event Action ReadyChanged
        {
            add { }
            remove { }
        }

        public UniTask<bool> Show(CancellationToken cancellation) => UniTask.FromResult(false);
    }
}
