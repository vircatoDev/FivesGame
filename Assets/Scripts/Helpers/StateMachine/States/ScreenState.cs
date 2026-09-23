using Cysharp.Threading.Tasks;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;
using Scripts.UI.Presenters;

namespace Scripts.Helpers.StateMachine.States
{
    /// <summary>A game state that shows its screen on enter and closes it on exit, as described by its StateConfig.</summary>
    public class ScreenState
    {
        private const int TransitionDelayMs = 300;

        private readonly StateConfig _config;
        private readonly BasePresenter _presenter;
        private GameStateType _prevStateName = GameStateType.MainMenu;

        public ScreenState(EcsWorld world, StateConfig config, BasePresenter presenter)
        {
            World = world;
            _config = config;
            _presenter = presenter;
        }

        public bool IsPopup => _config.IsPopup;
        public string PrefabName => _config.ScreenPrefab;
        public GameStateType StateName => _config.StateName;
        protected EcsWorld World { get; }

        public virtual async UniTask Enter(ScreenState prevState)
        {
            if (prevState != null)
                _prevStateName = prevState.StateName;

            World.Send(new OpenScreenEvent { PrefabName = PrefabName, IsPopup = IsPopup, InitData = _presenter });
            await UniTask.Delay(TransitionDelayMs);
        }

        public async UniTask Exit(ScreenState nextState)
        {
            if (!nextState.IsPopup)
            {
                if (_prevStateName != nextState.StateName)
                    World.Send(new CloseAllScreensEvent { NextScreenPrefabName = nextState.PrefabName });
                else
                    World.Send(new CloseScreenEvent { PrefabName = PrefabName, IsPopup = IsPopup });
            }

            await UniTask.Delay(TransitionDelayMs);
        }
    }
}
