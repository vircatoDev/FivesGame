using Scripts.Models;

namespace Scripts.Services.Interfaces
{
    public interface IStorable
    {
        void UpdatePlayerData(GameSaveData playerData);
    }
}