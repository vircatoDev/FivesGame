using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fives.Configs;
using Fives.Domain;
using Fives.Models;
using Fives.Services;
using Fives.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Fives.Runtime.Tests
{
    /// <summary>The save kept in memory; counts how many times it was written.</summary>
    internal sealed class MemoryStorage : IStorageService
    {
        public int Saves;
        public GameSaveData Data;
        public void Save<T>(string key, T data) { Data = (GameSaveData)(object)data; Saves++; }
        public T Load<T>(string key, T fallback = default) => Data == null ? fallback : (T)(object)Data;
    }

    internal sealed class FakeClock : IClock
    {
        public DateTime UtcNow { get; set; }
    }

    /// <summary>Frame time driven by the test: every Run advances by DeltaTime only when the test says so.</summary>
    internal sealed class FakeFrameTime : IFrameTime
    {
        public float Time { get; set; }
        public float DeltaTime { get; set; } = 0.1f;
        public float UnscaledDeltaTime { get; set; } = 0.1f;
        public float RealtimeSinceStartup { get; set; }
    }

    /// <summary>Rewarded ads without an ad network: supported or not, ready or not, watched to the reward or not.</summary>
    internal sealed class FakeRewardedAds : IRewardedAds
    {
        public bool IsSupported { get; set; } = true;
        public bool IsReady { get; private set; } = true;
        /// <summary>Whether the next ad is watched to its reward.</summary>
        public bool Watched = true;
        public int Shown;
        public event Action ReadyChanged;

        public void SetReady(bool ready)
        {
            IsReady = ready;
            ReadyChanged?.Invoke();
        }

        public UniTask<bool> Show(CancellationToken cancellation)
        {
            Shown++;
            return UniTask.FromResult(IsReady && Watched);
        }
    }

    /// <summary>Returns the key, with arguments after a colon, so tests do not depend on a language.</summary>
    internal sealed class FakeTexts : ITexts
    {
        public UniTask Ready() => UniTask.CompletedTask;
        public string Get(string key, params object[] args) => args.Length == 0 ? key : key + ":" + string.Join(",", args);
    }

    /// <summary>Hands back one sprite per reference at once and remembers what each owner still holds.</summary>
    internal sealed class FakeSpriteLoader : ISpriteLoader
    {
        private readonly TestObjects _objects;
        private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();
        public readonly Dictionary<object, int> Held = new Dictionary<object, int>();

        public FakeSpriteLoader(TestObjects objects) => _objects = objects;

        public Sprite Of(AssetReferenceSprite reference)
        {
            if (!_sprites.TryGetValue(reference.AssetGUID, out var sprite))
                _sprites[reference.AssetGUID] = sprite = _objects.Sprite(reference.AssetGUID);
            return sprite;
        }

        /// <summary>While set, loads wait for <see cref="FinishLoads"/>, as over a slow network.</summary>
        public bool Slow;
        /// <summary>While set, loads fail, as with a missing bundle.</summary>
        public bool Failing;
        private readonly List<(UniTaskCompletionSource<Sprite> Load, Sprite Sprite)> _pending =
            new List<(UniTaskCompletionSource<Sprite>, Sprite)>();

        public UniTask<Sprite> Load(AssetReferenceSprite sprite, object owner)
        {
            Held[owner] = Held.TryGetValue(owner, out var count) ? count + 1 : 1;
            if (Failing)
                return UniTask.FromException<Sprite>(new InvalidOperationException("The bundle is missing."));
            if (!Slow)
                return UniTask.FromResult(Of(sprite));

            var load = new UniTaskCompletionSource<Sprite>();
            _pending.Add((load, Of(sprite)));
            return load.Task;
        }

        /// <summary>Completes the waiting loads in the order they started.</summary>
        public void FinishLoads()
        {
            var pending = _pending.ToArray();
            _pending.Clear();
            foreach (var (load, sprite) in pending)
                load.TrySetResult(sprite);
        }

        public void Release(object owner) => Held.Remove(owner);

        /// <summary>Previews as the loading screen leaves them: loaded for every theme.</summary>
        public ThemePreviews Previews(GlobalConfig config)
        {
            var previews = new ThemePreviews(config, this);
            previews.Load().GetAwaiter().GetResult();
            return previews;
        }
    }

    /// <summary>Theme downloads without a server: the themes that reached the device, and the next downloads can fail.</summary>
    internal sealed class FakeThemeDownloads : IThemeDownloads
    {
        public readonly HashSet<string> OnDevice = new HashSet<string>();
        /// <summary>How many of the next downloads fail, as without a network.</summary>
        public int Failures;
        public int Downloads;

        public UniTask Download(IReadOnlyList<ThemeConfig> themes, IProgress<float> progress, CancellationToken cancellation)
        {
            Downloads++;
            if (Failures > 0)
            {
                Failures--;
                return UniTask.FromException(new InvalidOperationException("No network."));
            }

            progress?.Report(1f);
            foreach (var theme in themes)
                OnDevice.Add(theme.Id);
            return UniTask.CompletedTask;
        }
    }

    /// <summary>The loading screen: the player presses Retry at once, unless the screen is stuck until the boot is cancelled.</summary>
    internal sealed class FakeLoadingScreen : ILoadingScreen
    {
        public float Progress;
        public int Retries;
        public bool Stuck;

        public void SetProgress(float value) => Progress = value;

        public UniTask WaitForRetry(CancellationToken cancellation)
        {
            Retries++;
            return Stuck ? UniTask.Never(cancellation) : UniTask.CompletedTask;
        }
    }
}
