using System;

namespace Fives.Domain
{
    public sealed class EnergyWallet
    {
        public EnergyWallet(int balance, int maxBalance, TimeSpan recoveryInterval, DateTime lastRecoveryUtc)
        {
            if (maxBalance <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBalance));
            }

            if (balance < 0 || balance > maxBalance)
            {
                throw new ArgumentOutOfRangeException(nameof(balance));
            }

            if (recoveryInterval <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(recoveryInterval));
            }

            Balance = balance;
            MaxBalance = maxBalance;
            RecoveryInterval = recoveryInterval;
            LastRecoveryUtc = NormalizeUtc(lastRecoveryUtc);
        }

        public int Balance { get; private set; }
        public int MaxBalance { get; }
        public TimeSpan RecoveryInterval { get; }
        public DateTime LastRecoveryUtc { get; private set; }
        public bool IsFull => Balance >= MaxBalance;

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || amount > Balance)
            {
                return false;
            }

            Balance -= amount;
            return true;
        }

        public bool TryCredit(int amount, DateTime nowUtc)
        {
            if (amount <= 0)
            {
                return false;
            }

            Balance += Math.Min(amount, MaxBalance - Balance);
            if (IsFull)
            {
                LastRecoveryUtc = NormalizeUtc(nowUtc);
            }

            return true;
        }

        public int Recover(DateTime nowUtc)
        {
            nowUtc = NormalizeUtc(nowUtc);
            if (nowUtc < LastRecoveryUtc)
            {
                LastRecoveryUtc = nowUtc;
                return 0;
            }

            if (IsFull)
            {
                LastRecoveryUtc = nowUtc;
                return 0;
            }

            var elapsed = nowUtc - LastRecoveryUtc;
            var availableUnits = (int)(elapsed.Ticks / RecoveryInterval.Ticks);
            if (availableUnits <= 0)
            {
                return 0;
            }

            var recovered = Math.Min(availableUnits, MaxBalance - Balance);
            Balance += recovered;
            LastRecoveryUtc = LastRecoveryUtc.AddTicks(RecoveryInterval.Ticks * recovered);

            if (IsFull)
            {
                LastRecoveryUtc = nowUtc;
            }

            return recovered;
        }

        public TimeSpan TimeUntilNextRecovery(DateTime nowUtc)
        {
            if (IsFull)
            {
                return TimeSpan.FromMinutes(1);
            }

            var elapsed = NormalizeUtc(nowUtc) - LastRecoveryUtc;
            var remaining = RecoveryInterval - elapsed;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        private static DateTime NormalizeUtc(DateTime value)
        {
            if (value == default)
            {
                return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }

            if (value.Kind == DateTimeKind.Utc)
            {
                return value;
            }

            if (value.Kind == DateTimeKind.Local)
            {
                return value.ToUniversalTime();
            }

            return DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime();
        }
    }
}
