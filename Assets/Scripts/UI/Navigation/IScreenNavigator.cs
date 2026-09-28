using Fives.Models;

namespace Fives.UI.Navigation
{
    /// <summary>Opens and closes screens for the state machine. The ECS decides the game state; this only shows it.</summary>
    public interface IScreenNavigator
    {
        void Open(GameStateType state);
        void Close(GameStateType state);
        void CloseAll();
    }
}
