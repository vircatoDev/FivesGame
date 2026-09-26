using System.Collections.Generic;
using Newtonsoft.Json;

namespace Scripts.Configs
{
    /// <summary>
    /// The numbers of the economy. The built-in values live in GlobalConfig and the themes; Remote Config may override
    /// any of them once, while the game boots, and nothing changes during a session. An override out of its range is
    /// ignored, so a typo on the server cannot leave the player without energy or with negative prices.
    /// </summary>
    public sealed class GameBalance
    {
        private readonly GlobalConfig _config;
        private BalanceOverrides _overrides = new BalanceOverrides();

        public GameBalance(GlobalConfig config)
        {
            _config = config;
        }

        public int InitialEnergy => NonNegative(_overrides.InitialEnergy) ?? _config.InitialEnergy;
        public int MaxEnergy => Positive(_overrides.MaxEnergy) ?? _config.MaxEnergy;
        public float EnergyRecoveryHours => Positive(_overrides.EnergyRecoveryHours) ?? _config.EnergyRecoveryIntervalHours;
        public int InitialStars => NonNegative(_overrides.InitialStars) ?? _config.InitialStars;
        public int RewardStars => NonNegative(_overrides.RewardStars) ?? _config.RewardStars;
        public int HintPrice => NonNegative(_overrides.HintPrice) ?? _config.HintPrice;

        public int PriceOf(ThemeConfig theme) =>
            _overrides.ThemePrices != null && _overrides.ThemePrices.TryGetValue(theme.Id, out var price) && price >= 0
                ? price
                : theme.UnlockCost;

        /// <summary>Replaces the previous overrides; null restores the built-in values.</summary>
        public void Apply(BalanceOverrides overrides) => _overrides = overrides ?? new BalanceOverrides();

        private static int? NonNegative(int? value) => value >= 0 ? value : null;
        private static int? Positive(int? value) => value > 0 ? value : null;
        private static float? Positive(float? value) => value > 0 ? value : null;
    }

    /// <summary>
    /// The JSON of the "balance" key in Remote Config. Every field is optional; a missing one keeps the built-in value.
    /// For example: <c>{ "hintPrice": 4, "themePrices": { "cats": 60 } }</c>.
    /// </summary>
    public sealed class BalanceOverrides
    {
        public int? InitialEnergy;
        public int? MaxEnergy;
        public float? EnergyRecoveryHours;
        public int? InitialStars;
        public int? RewardStars;
        public int? HintPrice;
        /// <summary>Theme id to price.</summary>
        public Dictionary<string, int> ThemePrices;

        /// <summary>Field names match regardless of case. Throws JsonException for invalid JSON.</summary>
        public static BalanceOverrides FromJson(string json) => JsonConvert.DeserializeObject<BalanceOverrides>(json);
    }
}
