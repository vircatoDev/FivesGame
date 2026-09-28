using Fives.Models;

namespace Fives.Services
{
    /// <summary>A service whose state goes into the player's save; <see cref="PlayerSave.SaveAll"/> collects them.</summary>
    public interface IStorable
    {
        void UpdatePlayerData(GameSaveData playerData);
    }
}
