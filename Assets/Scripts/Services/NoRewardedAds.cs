using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Fives.Services
{
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
