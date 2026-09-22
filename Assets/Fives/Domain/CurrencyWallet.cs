using System;

namespace Fives.Domain
{
    public sealed class CurrencyWallet
    {
        public CurrencyWallet(int initialBalance)
        {
            if (initialBalance < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialBalance));
            }

            Balance = initialBalance;
        }

        public int Balance { get; private set; }

        public bool TryCredit(int amount)
        {
            if (amount <= 0 || Balance > int.MaxValue - amount)
            {
                return false;
            }

            Balance += amount;
            return true;
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || amount > Balance)
            {
                return false;
            }

            Balance -= amount;
            return true;
        }
    }
}
