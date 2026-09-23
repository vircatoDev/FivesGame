using Cysharp.Threading.Tasks;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.UI.Presenters;

namespace Scripts.Helpers.StateMachine.States
{
    /// <summary>The gameplay screen; starts a run once the screen is open.</summary>
    public sealed class PlayingState : ScreenState
    {
        public PlayingState(EcsWorld world, StateConfig config, BasePresenter presenter) : base(world, config, presenter)
        {
        }

        public override async UniTask Enter(ScreenState prevState)
        {
            await base.Enter(prevState);
            World.Send<GameStartEvent>();
        }
    }
}
