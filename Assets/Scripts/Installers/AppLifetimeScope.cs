using Fives.Domain;
using Scripts.Boot;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Services;
using Scripts.Services.Interfaces;
using Scripts.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scripts.Installers
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
            builder.RegisterComponent(loadingScreen);

            builder.Register<IStorageService, StorageService>(Lifetime.Singleton);
            builder.Register<PlayerDataSaveHelper>(Lifetime.Singleton);
            builder.Register<IClock, SystemClock>(Lifetime.Singleton);
            builder.Register<SoundService>(Lifetime.Singleton);
            builder.Register<LanguageService>(Lifetime.Singleton);
            builder.Register<LocalizedTexts>(Lifetime.Singleton).As<ITexts>().AsSelf();
            // BootFlow creates the texts only after the balance is loaded: through the language they read the save,
            // and a new save takes its starting stars and energy from the balance.
            builder.RegisterFactory<LocalizedTexts>(resolver => () => resolver.Resolve<LocalizedTexts>(), Lifetime.Singleton);
            builder.Register<AddressableSpriteLoader>(Lifetime.Singleton).As<ISpriteLoader>();
            builder.Register<ThemePreviews>(Lifetime.Singleton);
            builder.Register<EnergyService>(Lifetime.Singleton);
            builder.Register<StarService>(Lifetime.Singleton);
            builder.Register<PlayerProgressService>(Lifetime.Singleton);

            builder.RegisterEntryPoint<BootFlow>();
        }
    }
}
