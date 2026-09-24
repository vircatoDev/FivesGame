using Newtonsoft.Json;
using Scripts.Services.Interfaces;
using UnityEngine;
using VContainer;

namespace Scripts.Services
{
    /// <summary>JSON in PlayerPrefs. A key prefix keeps other writers, such as tests, away from the player's save.</summary>
    public class StorageService : IStorageService
    {
        private readonly string _keyPrefix;

        [Inject]
        public StorageService() : this(string.Empty)
        {
        }

        public StorageService(string keyPrefix)
        {
            _keyPrefix = keyPrefix;
        }

        public void Save<T>(string key, T data)
        {
            key = _keyPrefix + key;
            string json = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        public T Load<T>(string key, T defaultValue = default)
        {
            key = _keyPrefix + key;
            if (!PlayerPrefs.HasKey(key))
                return defaultValue;

            var json = PlayerPrefs.GetString(key);
            try
            {
                var data = JsonConvert.DeserializeObject<T>(json);
                if (data != null)
                    return data;
            }
            catch (JsonException)
            {
                Debug.LogWarning($"Cannot read save '{key}'. A copy is stored in '{key}.corrupt'.");
            }

            PlayerPrefs.SetString(key + ".corrupt", json);
            PlayerPrefs.Save();
            return defaultValue;
        }
    }
}
