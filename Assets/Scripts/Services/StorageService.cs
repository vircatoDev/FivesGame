using Newtonsoft.Json;
using Scripts.Services.Interfaces;
using UnityEngine;

namespace Scripts.Services
{
    public class StorageService : IStorageService
    {
        public void Save<T>(string key, T data)
        {
            string json = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        public T Load<T>(string key, T defaultValue = default)
        {
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
