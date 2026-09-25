using System;
using System.Collections.Generic;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services.Interfaces;
using UnityEngine;

namespace Scripts.Services
{
    /// <summary>
    /// The player's language: one of <see cref="GlobalConfig.Languages"/>, saved with the player data.
    /// Without a saved choice it follows the device language when supported, otherwise the first configured one.
    /// </summary>
    public class LanguageService : IStorable
    {
        private readonly string[] _supported;

        public string Current { get; private set; }
        public IReadOnlyList<string> Supported => _supported;
        public event Action<string> Changed;

        public LanguageService(GlobalConfig config, PlayerDataSaveHelper saveHelper)
        {
            _supported = config.Languages;
            var saved = saveHelper.GetPlayerData().Language;
            Current = IsSupported(saved) ? saved : IsSupported(DeviceLanguage) ? DeviceLanguage : _supported[0];
        }

        /// <summary>False for an unknown language or the current one.</summary>
        public bool TrySet(string code)
        {
            if (!IsSupported(code) || code == Current)
                return false;

            Current = code;
            Changed?.Invoke(code);
            return true;
        }

        public void UpdatePlayerData(GameSaveData playerData) => playerData.Language = Current;

        private bool IsSupported(string code) => Array.IndexOf(_supported, code) >= 0;

        private static string DeviceLanguage => Application.systemLanguage switch
        {
            SystemLanguage.Russian => "ru",
            SystemLanguage.French => "fr",
            SystemLanguage.Italian => "it",
            SystemLanguage.German => "de",
            SystemLanguage.Spanish => "es",
            _ => "en"
        };
    }
}
