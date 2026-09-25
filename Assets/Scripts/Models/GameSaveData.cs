using Scripts.Services;
using UnityEngine;

namespace Scripts.Models
{
    [System.Serializable]
    public class GameSaveData
    {
        /// <summary>Save format version, see <see cref="Fives.Domain.ProgressMigration"/>. Old saves read as 0.</summary>
        [SerializeField]
        public int Version;
        [SerializeField]
        public int Stars;
        [SerializeField]
        public EnergyData Energy;
        [SerializeField]
        public PlayerProgressData PlayerProgress;
        [SerializeField]
        public SoundSettingsData SoundSettings;
        /// <summary>Language code; empty in saves made before languages existed.</summary>
        [SerializeField]
        public string Language;
    }
}