using Cysharp.Threading.Tasks;

namespace Scripts.Helpers.StateMachine.States
{
    public interface IGameState {
        UniTask  Enter(IGameState prevState);
        UniTask Exit(IGameState newState);
    }
}