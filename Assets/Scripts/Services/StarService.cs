using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services.Interfaces;

namespace Scripts.Services
{
    public class StarService : ICurrencyService, IStorable
    {
        private int _currentStars;

        public StarService(PlayerDataSaveHelper saveHelper)
        {
            _currentStars = saveHelper.GetPlayerData().Stars;
        }

        public int GetBalance()
        {
            return _currentStars;
        }

        public void Add(int amount)
        {
            _currentStars += amount;
        }

        public bool Spend(int amount)
        {
            if (_currentStars >= amount)
            {
                _currentStars -= amount;
                return true;
            }

            return false;
        }

        public void UpdatePlayerData(GameSaveData playerData)
        {
            playerData.Stars = _currentStars;
        }
    }
}