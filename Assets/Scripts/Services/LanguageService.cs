using System;
using System.Collections.Generic;
using Fives.Configs;
using Fives.Models;
using UnityEngine;

namespace Fives.Services
{
    /// <summary>
    /// The player's language: one of <see cref="GlobalConfig.Languages"/>, saved with the player data.
    /// Without a saved choice it follows the device language when supported, otherwise the first configured one.
    /// </summary>
    public class LanguageService : IStorable
    {
        private readonly string[] _supported;
        private readonly PlayerSave _save;
        private string _current;

        /// <summary>
        /// Read from the save on first use: the boot creates this service for the texts before it loads the save.
        /// </summary>
        public string Current => _current ??= Initial(_save.GetPlayerData().Language);
        public IReadOnlyList<string> Supported => _supported;
        public event Action<string> Changed;

        public LanguageService(GlobalConfig config, PlayerSave save)
        {
            _supported = config.Languages;
            _save = save;
        }

        /// <summary>False for an unknown language or the current one.</summary>
        public bool TrySet(string code)
        {
            if (!IsSupported(code) || code == Current)
                return false;

            _current = code;
            Changed?.Invoke(code);
            return true;
        }

        public void UpdatePlayerData(GameSaveData playerData) => playerData.Language = Current;

        private string Initial(string saved) =>
            IsSupported(saved) ? saved : IsSupported(DeviceLanguage) ? DeviceLanguage : _supported[0];

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
