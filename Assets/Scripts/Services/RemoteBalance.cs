using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Scripts.Configs;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using UnityEngine;

namespace Scripts.Services
{
    /// <summary>
    /// Fetches the "balance" key of Remote Config while the game boots and applies it to <see cref="GameBalance"/>.
    /// The player signs in anonymously: Remote Config needs a player, and no personal data is involved. Without a
    /// network within the timeout, the values cached by the previous session apply, or else the built-in ones.
    /// </summary>
    public sealed class RemoteBalance
    {
        public const string Key = "balance";
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(3);

        private readonly GameBalance _balance;

        public RemoteBalance(GameBalance balance)
        {
            _balance = balance;
        }

        public async UniTask Load(CancellationToken cancellation)
        {
            try
            {
                await Fetch().Timeout(Timeout).AttachExternalCancellation(cancellation);
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                Debug.LogWarning($"Remote Config is unavailable, the cached or built-in balance applies. {exception.Message}");
            }

            // Fetched now, or loaded from the previous session's cache when Remote Config started.
            _balance.Apply(Parse(RemoteConfigService.Instance.appConfig.GetJson(Key)));
        }

        private static async UniTask Fetch()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await RemoteConfigService.Instance.FetchConfigsAsync(new NoAttributes(), new NoAttributes());
        }

        private static BalanceOverrides Parse(string json)
        {
            try
            {
                return BalanceOverrides.FromJson(json);
            }
            catch (JsonException exception)
            {
                Debug.LogWarning($"The \"{Key}\" key of Remote Config is not valid JSON, the built-in balance applies. {exception.Message}");
                return null;
            }
        }

        // Segmentation needs no attributes: the balance is the same for every player.
        private struct NoAttributes
        {
        }
    }
}
