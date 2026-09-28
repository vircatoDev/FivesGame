using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Fives.Services
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
}
