namespace Fives.Services
{
    /// <summary>Where the save is kept, as JSON by key: PlayerPrefs in the game, memory in tests.</summary>
    public interface IStorageService
    {
        void Save<T>(string key, T data);
        T Load<T>(string key, T defaultValue = default);
    }
}
