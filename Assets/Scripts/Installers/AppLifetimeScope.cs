using Fives.Boot;
using Fives.Configs;
using Fives.Domain;
using Fives.Services;
using Fives.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Fives.Installers
{
    /// <summary>
    /// Root scope of the Boot scene. It lives for the whole app, so its services (player data, language, content)
    /// are prepared once and shared with the game scene's child scope.
    /// </summary>
    public class AppLifetimeScope : LifetimeScope
    {
        [SerializeField] private GlobalConfig globalConfig;
        [SerializeField] private LoadingScreen loadingScreen;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(globalConfig);
            builder.Register<GameBalance>(Lifetime.Singleton);
            builder.Register<RemoteBalance>(Lifetime.Singleton);
            builder.RegisterComponent(loadingScreen).AsSelf().As<ILoadingScreen>();

            builder.Register<IStorageService, StorageService>(Lifetime.Singleton);
            builder.Register<PlayerSave>(Lifetime.Singleton);
            builder.Register<IClock, SystemClock>(Lifetime.Singleton);
            builder.Register<SoundService>(Lifetime.Singleton);
            builder.Register<LanguageService>(Lifetime.Singleton);
            builder.Register<LocalizedTexts>(Lifetime.Singleton).As<ITexts>().AsSelf();
            builder.Register<AddressableSpriteLoader>(Lifetime.Singleton).As<ISpriteLoader>();
            builder.Register<ThemePreviews>(Lifetime.Singleton);
            builder.Register<AddressableThemeDownloads>(Lifetime.Singleton).As<IThemeDownloads>();
#if UNITY_ANDROID
            // Starts LevelPlay with the app, in child-directed mode; the result screen offers x2 once an ad is loaded.
            builder.RegisterEntryPoint<LevelPlayRewardedAds>().As<IRewardedAds>();
#else
            builder.Register<NoRewardedAds>(Lifetime.Singleton).As<IRewardedAds>();
#endif
            builder.Register<EnergyService>(Lifetime.Singleton);
            builder.Register<StarService>(Lifetime.Singleton);
            builder.Register<PlayerProgressService>(Lifetime.Singleton);

            builder.RegisterEntryPoint<BootFlow>();
        }
    }
}
