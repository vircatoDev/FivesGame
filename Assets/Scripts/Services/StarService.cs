using System;
using Fives.Domain;
using Fives.Models;

namespace Fives.Services
{
    public class StarService : IStorable
    {
        private readonly CurrencyWallet _wallet;

        public StarService(PlayerSave save)
        {
            _wallet = new CurrencyWallet(Math.Max(0, save.GetPlayerData().Stars));
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
