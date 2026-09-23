using Scripts.Models;

namespace Scripts.Components
{
    public struct CurrencyChangedEvent
    {
        public Currency Currency;
        public int Balance;
        public int Delta;
        public bool Insufficient;

        public static CurrencyChangedEvent Changed(Currency currency, int balance, int delta) =>
            new CurrencyChangedEvent { Currency = currency, Balance = balance, Delta = delta };

        public static CurrencyChangedEvent NotEnough(Currency currency) =>
            new CurrencyChangedEvent { Currency = currency, Insufficient = true };
    }
}
