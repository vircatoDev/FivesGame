using System;
using NUnit.Framework;

namespace Fives.Domain.Tests
{
    /// <summary>Звёзды, энергия и награда за партию.</summary>
    public sealed class EconomyRulesTests
    {
        /// <summary>Потратить ноль или отрицательную сумму нельзя, баланс не меняется.</summary>
        [Test]
        public void Spend_RejectsNonPositiveAmountWithoutChangingBalance()
        {
            var wallet = new CurrencyWallet(100);

            Assert.That(wallet.TrySpend(-10), Is.False);
            Assert.That(wallet.TrySpend(0), Is.False);
            Assert.That(wallet.Balance, Is.EqualTo(100));
        }

        /// <summary>
        /// За 2,5 часа при восстановлении по единице в час возвращаются две единицы, а оставшиеся полчаса идут в счёт
        /// следующей.
        /// </summary>
        [Test]
        public void EnergyRecovery_PreservesFractionalRemainder()
        {
            var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var wallet = new EnergyWallet(0, 10, TimeSpan.FromHours(1), start);

            var recovered = wallet.Recover(start.AddHours(2.5));

            Assert.That(recovered, Is.EqualTo(2));
            Assert.That(wallet.Balance, Is.EqualTo(2));
            Assert.That(wallet.LastRecoveryUtc, Is.EqualTo(start.AddHours(2)));
            Assert.That(wallet.TimeUntilNextRecovery(start.AddHours(2.5)), Is.EqualTo(TimeSpan.FromMinutes(30)));
        }

        /// <summary>Награду за партию можно забрать один раз, повторная попытка, даже x2, ничего не даёт.</summary>
        [Test]
        public void Reward_CanOnlyBeClaimedOncePerRun()
        {
            var claim = new RewardClaim();

            Assert.That(claim.TryClaim(10, false, out var firstReward), Is.True);
            Assert.That(firstReward, Is.EqualTo(10));
            Assert.That(claim.TryClaim(10, true, out var repeatedReward), Is.False);
            Assert.That(repeatedReward, Is.Zero);
        }
    }
}
