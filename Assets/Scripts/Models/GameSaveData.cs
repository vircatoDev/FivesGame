using Fives.Services;

namespace Fives.Models
{
    [System.Serializable]
    public class GameSaveData
    {
        /// <summary>Save format version, see <see cref="Fives.Domain.ProgressMigration"/>. Old saves read as 0.</summary>
        public int Version;
        public int Stars;
        public EnergyData Energy;
        public PlayerProgressData PlayerProgress;
        public SoundSettingsData SoundSettings;
        /// <summary>Language code; empty in saves made before languages existed.</summary>
        public string Language;
    }
}