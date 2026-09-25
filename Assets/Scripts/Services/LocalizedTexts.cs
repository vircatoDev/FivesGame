using Cysharp.Threading.Tasks;
using UnityEngine.Localization.Settings;

namespace Scripts.Services
{
    /// <summary>Texts from the "UI" string table in the language chosen in <see cref="LanguageService"/>.</summary>
    public sealed class LocalizedTexts : ITexts
    {
        public const string Table = "UI";

        private readonly LanguageService _language;

        public LocalizedTexts(LanguageService language)
        {
            _language = language;
        }

        /// <summary>Waits for the tables (all locales are preloaded) and applies the saved language.</summary>
        public async UniTask Initialize()
        {
            await LocalizationSettings.InitializationOperation.Task;
            Select(_language.Current);
            _language.Changed += Select;
        }

        public string Get(string key, params object[] args) =>
            LocalizationSettings.StringDatabase.GetLocalizedString(Table, key, args);

        private static void Select(string code) =>
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(code);
    }
}
