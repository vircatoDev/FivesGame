using System.Linq;
using Leopotam.Ecs;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Helpers.Factory;
using Scripts.Helpers.StateMachine;
using Scripts.Models;
using Scripts.Services;
using Scripts.Services.Interfaces;
using Scripts.UI.Presenters;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scripts.Installers
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GlobalConfig globalConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            var ecsWorld = new EcsWorld();
            builder.RegisterInstance(ecsWorld);
        
            builder.RegisterInstance(globalConfig);
            builder.RegisterInstance(globalConfig.StateConfigs.ToDictionary(cfg => cfg.StateName));

            builder.Register<PlayerDataSaveHelper>(Lifetime.Singleton);
            builder.Register<IStorageService, StorageService>(Lifetime.Singleton);
            builder.Register<GameSession>(Lifetime.Singleton);
            builder.Register<SoundService>(Lifetime.Singleton);
            builder.Register<EnergyService>(Lifetime.Singleton);
            builder.Register<StarService>(Lifetime.Singleton);
            builder.Register<PlayerProgressService>(Lifetime.Singleton);
            builder.Register<ECSCommandService>(Lifetime.Singleton);

            builder.Register<PresenterFactory>(Lifetime.Singleton);
            builder.Register<GameStateFactory>(Lifetime.Singleton);
        
            builder.Register<MainMenuPresenter>(Lifetime.Transient);
            builder.Register<SelectMenuPresenter>(Lifetime.Transient);
            builder.Register<GamePlayPresenter>(Lifetime.Transient);
            builder.Register<GameResultPresenter>(Lifetime.Transient);
            builder.Register<SettingsPresenter>(Lifetime.Transient);

            builder.Register<GameStateMachine>(Lifetime.Singleton);
        }
    }
}