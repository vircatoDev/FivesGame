using System.Linq;
using Leopotam.Ecs;
using Scripts.Configs;
using Scripts.Helpers.Factory;
using Scripts.Helpers.StateMachine;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Presenters;
using Scripts.UI.Views;
using VContainer;
using VContainer.Unity;

namespace Scripts.Installers
{
    /// <summary>Child scope of the game scene: the ECS world, the run and the screens. App services come from the parent.</summary>
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(new EcsWorld());
            builder.Register(resolver => resolver.Resolve<GlobalConfig>().StateConfigs.ToDictionary(cfg => cfg.StateName), Lifetime.Singleton);

            builder.Register<GameSession>(Lifetime.Singleton);
            builder.Register<GameStartService>(Lifetime.Singleton);
            builder.Register<ThemeShop>(Lifetime.Singleton);

            builder.Register<ScreenCatalog>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<HeaderPanelView>().As<IHeaderPanelView>();

            builder.Register<MainMenuPresenter>(Lifetime.Transient);
            builder.Register<SelectMenuPresenter>(Lifetime.Transient);
            builder.Register<GamePlayPresenter>(Lifetime.Singleton).AsSelf().As<IBoardHud>();
            builder.Register<GameResultPresenter>(Lifetime.Transient);
            builder.Register<SettingsPresenter>(Lifetime.Transient);

            builder.Register<GameStateMachine>(Lifetime.Singleton);
        }
    }
}
