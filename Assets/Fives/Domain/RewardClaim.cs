using System;

namespace Fives.Domain
{
    public sealed class RewardClaim
    {
        public bool IsClaimed { get; private set; }

        public bool TryClaim(int baseAmount, bool doubleReward, out int grantedAmount)
        {
            if (baseAmount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseAmount));
            }

            if (IsClaimed)
            {
                grantedAmount = 0;
                return false;
            }

            IsClaimed = true;
            grantedAmount = doubleReward ? checked(baseAmount * 2) : baseAmount;
            return true;
        }

        public void Reset()
        {
            IsClaimed = false;
        }
    }
}
