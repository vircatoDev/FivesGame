namespace Scripts.Services.Interfaces
{
    public interface ISoundService
    {
        void PlayBackgroundMusic(string key);
        void PlaySoundEffect(string key, float volume);
        void SetMusicVolume(float volume);
        void SetSoundEffectVolume(float vloume);

    }
}