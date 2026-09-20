using Cysharp.Threading.Tasks;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Services;
using Scripts.UI.Presenters;
using UnityEngine;

namespace Scripts.Helpers.StateMachine.States
{
    public class PlayingState : GameStateBase
    {
        private readonly ECSCommandService _commandService;

        public PlayingState(ECSCommandService commandService, StateConfig config, BasePresenter presenter) : base(commandService, config, presenter)
        {
            _commandService = commandService;
        }

        public override async UniTask Enter(IGameState prevState)
        {
            await base.Enter(prevState);
            Debug.Log("Playing: Start gameplay");
            _commandService.CreateSimpleEventCommand<GameStartEvent>().Execute();
      
        }

        public override async UniTask Exit(IGameState newState)
        {
            await base.Exit(newState);
            Debug.Log("Playing: Pause gameplay");
       
        }
    }
}