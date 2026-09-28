using System.Collections.Generic;
using System.Linq;
using Fives.Configs;
using Fives.Models;
using Fives.Services;
using Fives.UI;
using Fives.UI.Navigation;
using Fives.UI.Presenters;
using Fives.UI.Views;
using Leopotam.Ecs;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Fives.Installers
{
    /// <summary>Child scope of the game scene: the ECS world, the run and the screens. App services come from the parent.</summary>
    public class GameLifetimeScope : LifetimeScope
    {
        [Tooltip("Parent of the full screens.")]
        [SerializeField] private Transform screenLayer;
        [Tooltip("Parent of the popups, drawn over the screens.")]
        [SerializeField] private Transform popupLayer;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(new EcsWorld());
            builder.Register<IReadOnlyDictionary<GameStateType, StateConfig>>(
                resolver => resolver.Resolve<GlobalConfig>().StateConfigs.ToDictionary(config => config.StateName), Lifetime.Singleton);

            builder.Register<GameSession>(Lifetime.Singleton);
            builder.Register<GameStartService>(Lifetime.Singleton);
            builder.Register<ThemeShop>(Lifetime.Singleton);

            builder.RegisterInstance(new ScreenLayers(screenLayer, popupLayer));
            builder.Register<ScreenNavigator>(Lifetime.Singleton).As<IScreenNavigator>();
            builder.Register<GameStateMachine>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<HeaderPanelView>().As<IHeaderPanelView>();

            builder.Register<MainMenuPresenter>(Lifetime.Transient);
            builder.Register<SelectMenuPresenter>(Lifetime.Transient);
            builder.Register<GamePlayPresenter>(Lifetime.Singleton).AsSelf().As<IBoardHud>();
            builder.Register<GameResultPresenter>(Lifetime.Transient);
            builder.Register<SettingsPresenter>(Lifetime.Transient);
        }
    }
}
