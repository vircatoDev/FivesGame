#if UNITY_ANDROID
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Scripts.Configs;
using Unity.Services.LevelPlay;
using UnityEngine;
using VContainer.Unity;

namespace Scripts.Services
{
    /// <summary>
    /// Rewarded ads through LevelPlay with Unity Ads, in child-directed mode: every player is treated as a child
    /// (COPPA, Google Play Families), so ads are contextual and the device's advertising ID is never used.
    /// One ad is kept loaded; after it is shown or fails, the next one loads.
    /// </summary>
    public sealed class LevelPlayRewardedAds : IRewardedAds, IStartable, IDisposable
    {
        private static readonly TimeSpan LateRewardWait = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(30);

        private readonly string _appKey;
        private readonly string _adUnitId;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private LevelPlayRewardedAd _ad;
        private UniTaskCompletionSource<bool> _showing;
        private bool _rewarded;
        private bool _closed;

        public LevelPlayRewardedAds(GlobalConfig config)
        {
            _appKey = config.AndroidAdsAppKey;
            _adUnitId = config.AndroidRewardedAdUnitId;
#if UNITY_EDITOR
            // The editor SDK accepts any key and shows LevelPlay's mock ads, so the x2 flow can be tried in play mode
            // on the Android platform before the app is registered in LevelPlay.
            if (string.IsNullOrEmpty(_appKey) || string.IsNullOrEmpty(_adUnitId))
                (_appKey, _adUnitId) = ("editor-mock", "editor-mock-rewarded");
#endif
        }

        public bool IsSupported => !string.IsNullOrEmpty(_appKey) && !string.IsNullOrEmpty(_adUnitId);
        public bool IsReady => _ad != null && _showing == null && _ad.IsAdReady();
        public event Action ReadyChanged;

        public void Start()
        {
            if (!IsSupported)
            {
                Debug.LogWarning("No LevelPlay app key or rewarded ad unit in GlobalConfig: the doubled reward is not offered.");
                return;
            }

            // Child-directed settings go before Init, or the first requests would be made without them.
            LevelPlayPrivacySettings.SetCOPPA(true);
            LevelPlay.SetMetaData("is_deviceid_optout", "true");
            LevelPlay.SetMetaData("UnityAds_coppa", "true");
            LevelPlay.OnInitSuccess += OnInitialized;
            LevelPlay.OnInitFailed += error => Debug.LogWarning($"LevelPlay did not start: {error}");
            LevelPlay.Init(_appKey);
        }

        public async UniTask<bool> Show(CancellationToken cancellation)
        {
            if (!IsReady)
                return false;

            _rewarded = false;
            _closed = false;
            _showing = new UniTaskCompletionSource<bool>();
            var showing = _showing.Task;
            ReadyChanged?.Invoke();
            _ad.ShowAd();
            try
            {
                return await showing.AttachExternalCancellation(cancellation);
            }
            finally
            {
                _showing = null;
                ReadyChanged?.Invoke();
            }
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
            LevelPlay.OnInitSuccess -= OnInitialized;
            _ad?.DestroyAd();
        }

        private void OnInitialized(LevelPlayConfiguration configuration)
        {
            _ad = new LevelPlayRewardedAd(_adUnitId);
            _ad.OnAdLoaded += _ => ReadyChanged?.Invoke();
            _ad.OnAdLoadFailed += error => RetryLoad(error.ToString()).Forget();
            _ad.OnAdDisplayFailed += (_, error) => Finish(false, error.ToString());
            _ad.OnAdRewarded += (_, _) => OnRewarded();
            _ad.OnAdClosed += _ => OnClosed();
            _ad.LoadAd();
        }

        // The reward and the close arrive in either order; a reward that comes shortly after the close still counts.
        private void OnRewarded()
        {
            _rewarded = true;
            if (_closed)
                Finish(true);
        }

        private void OnClosed()
        {
            _closed = true;
            if (_rewarded)
                Finish(true);
            else
                FinishWithoutLateReward().Forget();
        }

        private async UniTaskVoid FinishWithoutLateReward()
        {
            await UniTask.Delay(LateRewardWait, ignoreTimeScale: true, cancellationToken: _lifetime.Token);
            Finish(_rewarded);
        }

        private void Finish(bool rewarded, string failure = null)
        {
            if (_showing == null)
                return; // already finished: a display failure may still be followed by a close
            if (failure != null)
                Debug.LogWarning($"The rewarded ad was not shown: {failure}");
            _showing.TrySetResult(rewarded);
            _ad.LoadAd();
        }

        private async UniTaskVoid RetryLoad(string error)
        {
            Debug.LogWarning($"No rewarded ad loaded, retrying in {RetryDelay.TotalSeconds} s: {error}");
            await UniTask.Delay(RetryDelay, ignoreTimeScale: true, cancellationToken: _lifetime.Token);
            _ad.LoadAd();
        }
    }
}
#endif
