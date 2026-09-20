using System;
using Fives.Domain;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services.Interfaces;

namespace Scripts.Services
{
    public class StarService : ICurrencyService, IStorable
    {
        private readonly CurrencyWallet _wallet;

        public StarService(PlayerDataSaveHelper saveHelper)
        {
            _wallet = new CurrencyWallet(Math.Max(0, saveHelper.GetPlayerData().Stars));
        }

        public int GetBalance()
        {
            return _wallet.Balance;
        }

        public void Add(int amount)
        {
            if (!_wallet.TryCredit(amount))
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }
        }

        public bool Spend(int amount)
        {
            return _wallet.TrySpend(amount);
        }

        public void UpdatePlayerData(GameSaveData playerData)
        {
            playerData.Stars = _wallet.Balance;
        }
    }
}
