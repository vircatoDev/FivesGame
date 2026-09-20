namespace Scripts.Services.Interfaces
{
    public interface IStorageService {
        void Save<T>(string key, T data);
        T Load<T>(string key, T defaultValue = default);
    }
}