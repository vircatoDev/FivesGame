using System;
using Fives.Domain;
using NUnit.Framework;

namespace Fives.Domain.Tests
{
    public sealed class CorrectnessTests
    {
        [Test]
        public void Spend_RejectsNonPositiveAmountWithoutChangingBalance()
        {
            var wallet = new CurrencyWallet(100);

            Assert.That(wallet.TrySpend(-10), Is.False);
            Assert.That(wallet.TrySpend(0), Is.False);
            Assert.That(wallet.Balance, Is.EqualTo(100));
        }

        [Test]
        public void Purchase_DebitsPriceExactlyOnce()
        {
            var wallet = new CurrencyWallet(200);

            Assert.That(wallet.TrySpend(60), Is.True);
            Assert.That(wallet.Balance, Is.EqualTo(140));
        }

        [Test]
        public void Reward_CanOnlyBeClaimedOncePerRun()
        {
            var claim = new RewardClaim();

            Assert.That(claim.TryClaim(10, false, out var firstReward), Is.True);
            Assert.That(firstReward, Is.EqualTo(10));
            Assert.That(claim.TryClaim(10, true, out var repeatedReward), Is.False);
            Assert.That(repeatedReward, Is.Zero);
        }

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

        [Test]
        public void BoardMath_UsesConfiguredBoardSize()
        {
            Assert.That(BoardMath.CellCount(3), Is.EqualTo(9));
            Assert.That(BoardMath.CellCount(6), Is.EqualTo(36));
            Assert.That(BoardMath.TileIdAt(5, 5, 6), Is.EqualTo(35));
        }
    }
}
