using Cysharp.Threading.Tasks;
using Scripts.Configs;
using Scripts.Services;
using Scripts.UI.Presenters;
using UnityEngine;

namespace Scripts.Helpers.StateMachine.States
{
    public class SettingsMenuState : GameStateBase
    {
        public SettingsMenuState(ECSCommandService commandService, StateConfig config, BasePresenter presenter) : base(
            commandService, config,
            presenter)
        {
        }

        public override async UniTask Enter(IGameState prevState)
        {
            await base.Enter(prevState);
            Debug.Log("SettingsMenu: Custom Enter Logic");
        }

        public override async UniTask Exit(IGameState nextState)
        {
            await base.Exit(nextState);
            Debug.Log("SettingsMenu: Custom Exit Logic");
        }
    }
}