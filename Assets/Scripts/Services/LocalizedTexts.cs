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

        /// <summary>Applies the saved language and waits until its tables are loaded.</summary>
        public async UniTask Initialize()
        {
            await Ready();
            Select(_language.Current);
            await Ready();
            _language.Changed += Select;
        }

        // Changing the locale restarts the initialization operation, which loads that locale's tables.
        public UniTask Ready() => LocalizationSettings.InitializationOperation.Task.AsUniTask();

        public string Get(string key, params object[] args) =>
            LocalizationSettings.StringDatabase.GetLocalizedString(Table, key, args);

        private static void Select(string code) =>
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(code);
    }
}
