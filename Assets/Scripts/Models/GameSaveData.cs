using Scripts.Services;
using UnityEngine;

namespace Scripts.Models
{
    [System.Serializable]
    public class GameSaveData
    {
        [SerializeField]
        public int Stars;
        [SerializeField]
        public EnergyData Energy;
        [SerializeField]
        public PlayerProgressData PlayerProgress;
        [SerializeField]
        public SoundSettingsData SoundSettings;
    }
}