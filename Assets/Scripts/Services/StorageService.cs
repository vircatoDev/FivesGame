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
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                return JsonConvert.DeserializeObject<T>(json);
            }
            return defaultValue;
        }
    
    }
}