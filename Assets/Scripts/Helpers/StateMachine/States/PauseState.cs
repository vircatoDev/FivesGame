using Cysharp.Threading.Tasks;
using Scripts.Configs;
using Scripts.Services;
using UnityEngine;

namespace Scripts.Helpers.StateMachine.States
{
    public class PauseState : GameStateBase
    {
        public PauseState(ECSCommandService commandService, StateConfig config) : base(commandService, config)
        {
        }

        public override async UniTask Enter(IGameState prevState)
        {
            await base.Enter(prevState);
            Debug.Log("PauseState: Custom Enter Logic");
        
        }

        public override async UniTask Exit(IGameState nextState)
        {
            Debug.Log("PauseState: Custom Exit Logic");
            await base.Exit(nextState);
        }
    }
}