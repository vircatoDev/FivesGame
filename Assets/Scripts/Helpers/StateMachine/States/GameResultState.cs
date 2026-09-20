using Cysharp.Threading.Tasks;
using Scripts.Configs;
using Scripts.Services;
using Scripts.UI.Presenters;
using UnityEngine;

namespace Scripts.Helpers.StateMachine.States
{
    class GameResultState : GameStateBase
    {
        public GameResultState(ECSCommandService commandService,StateConfig config, BasePresenter presenter) : base(commandService, config,
            presenter)
        {
        }

        public override async UniTask Enter(IGameState prevState)
        {
            await base.Enter(prevState);
            Debug.Log("GameResultState: Custom Enter Logic");
        }

        public override async UniTask Exit(IGameState nextState)
        {
            Debug.Log("GameResultState: Custom Exit Logic");
            await base.Exit(nextState);
        }
    }
}