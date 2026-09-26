using System.Collections.Generic;
using Scripts.Models;
using Scripts.UI;
using UnityEngine;

namespace Scripts.Configs
{
    [CreateAssetMenu(menuName = "Game/Global Config")]
    public class GlobalConfig : ScriptableObject
    {
        [Header("Game Modes")] 
        public GameSettings[] GameModes;

        [Header("State Configurations")] 
        public StateConfig[] StateConfigs;

        [Header("Themes and Images")] 
        public List<ThemeConfig> Themes;

        [Header("Energy Settings")] 
        public int InitialEnergy = 5;
        public int MaxEnergy = 10;
        public float EnergyRecoveryIntervalHours = 1f;

        [Header("Star Settings")] 
        public int InitialStars = 100;
        public int RewardStars = 10;
        public int HintPrice = 5;

        [Header("Languages")]
        [Tooltip("Language codes in the order of the settings flags; the first one is the fallback.")]
        public string[] Languages = { "en", "ru", "fr", "it", "de", "es" };

        [Header("Player Progress Settings")] 
        public string[] DefaultUnlockedThemes;

        [Header("Sound Settings")] 
        public List<GameSoundCollection> AudioClipsCollection;

        [Header("Ads (LevelPlay, Android)")]
        [Tooltip("App key of the Android app in the LevelPlay dashboard. Empty: no ads, the x2 reward is hidden.")]
        public string AndroidAdsAppKey;
        [Tooltip("Rewarded ad unit id from the LevelPlay dashboard.")]
        public string AndroidRewardedAdUnitId;

        [Header("Board")]
        public BoardView BoardPrefab;
        public TileUiProvider TilePrefab;
    }
}