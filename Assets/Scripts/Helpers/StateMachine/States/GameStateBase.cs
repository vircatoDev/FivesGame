using Cysharp.Threading.Tasks;
using Scripts.Commands;
using Scripts.Configs;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Presenters;
using UnityEngine;

namespace Scripts.Helpers.StateMachine.States
{
    public abstract class GameStateBase : IGameState
    {
        public bool IsPopup => _config.IsPopup;
        public string PrefabName => _config.ScreenPrefab;
        public GameStateType StateName => _config.StateName;

        private GameStateType _prevStateName = GameStateType.MainMenu;
        private readonly ECSCommandService _commandService;
        private readonly StateConfig _config;
        private readonly BasePresenter _presenter;

        public GameStateBase(ECSCommandService commandService, StateConfig config, BasePresenter presenter = null)
        {
            _commandService = commandService;
            _config = config;
            _presenter = presenter;
        }

        public virtual async UniTask Enter(IGameState prevState)
        {
            Debug.Log($"Entering state: {_config.StateName}");

            if (prevState != null)
                _prevStateName = (prevState as GameStateBase).StateName;

            _commandService.CreateCommand<OpenScreenCommand>(_config.ScreenPrefab, _config.IsPopup, _presenter).Execute();

            await UniTask.Delay(300);
        }

        public virtual async UniTask Exit(IGameState nextState)
        {
            Debug.Log($"Exiting state: {_config.StateName}");

            var nextStateBase = (nextState as GameStateBase);

            if (nextStateBase.IsPopup)
            {
                await UniTask.Delay(300);
                return;
            }

            if (_prevStateName != nextStateBase.StateName && ShouldCloseAllScreens(nextState))
            {
                _commandService.CreateCommand<CloseAllScreensCommand>(nextStateBase.PrefabName).Execute();
            }
            else
            {
                _commandService.CreateCommand<CloseScreenCommand>(_config.ScreenPrefab, _config.IsPopup).Execute();
            }

            await UniTask.Delay(300);
        }

        private bool ShouldCloseAllScreens(IGameState nextState)
        {
            return nextState is GameStateBase { IsPopup: false } || nextState is null;
        }
    }
}