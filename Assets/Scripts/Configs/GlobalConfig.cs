using System.Collections.Generic;
using Scripts.Models;
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

        [Header("Player Progress Settings")] 
        public string[] DefaultUnlockedThemes;

        [Header("Sound Settings")] 
        public List<GameSoundCollection> AudioClipsCollection;

        public string BoardPrefab;
        public string TilePrefab;
    }
}