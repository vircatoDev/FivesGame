using System.Collections;
using Fives.Editor.Localization;
using Fives.Models;
using Fives.UI.Views;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.TestTools;

namespace Fives.UI.Tests
{
    /// <summary>Префаб главного меню: карточки карусели сами переводят название темы.</summary>
    public sealed class MainMenuPrefabTests
    {
        private const string PrefabPath = "Assets/Content/Prefabs/UI/Screens/MainMenuScreen.prefab";
        // Вход в Play Mode перезагружает домен и пересоздаёт этот класс, поэтому то, что нужно вернуть после теста,
        // хранится в SessionState, а не в полях.
        private const string StartSceneKey = "Fives.Tests.MainMenu.PlayModeStartScene";
        private const string PlayModeKey = "Fives.Tests.MainMenu.AddressablesPlayMode";

        /// <summary>
        /// Карточка показывает название темы на выбранном языке и переводит его, когда язык меняется при открытом
        /// меню: главное меню под попапом настроек сразу говорит на новом языке, презентер для этого не нужен.
        /// </summary>
        [UnityTest]
        public IEnumerator CardTitle_FollowsALanguageChange_WhileTheMenuIsOpen()
        {
            // Как в GamePlayPrefabTests: Play Mode без запуска игры, тексты из базы ассетов.
            SessionState.SetString(StartSceneKey, AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            EditorSceneManager.playModeStartScene = null;
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            SessionState.SetInt(PlayModeKey, settings.ActivePlayModeDataBuilderIndex);
            settings.ActivePlayModeDataBuilderIndex = settings.DataBuilders.FindIndex(builder => builder is BuildScriptFastMode);
            yield return new EnterPlayMode();

            var names = ExcelStringImporter.Read(ExcelStringImporter.SheetPath)["theme.dogs"];
            var canvas = new GameObject("Main menu card test", typeof(Canvas));
            try
            {
                while (!LocalizationSettings.InitializationOperation.IsDone)
                    yield return null;
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("en");
                while (!LocalizationSettings.InitializationOperation.IsDone)
                    yield return null;

                var screen = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), canvas.transform);
                yield return null;
                var card = screen.GetComponentInChildren<PuzzleCardView>();
                var title = card.transform.Find("ThemeName").GetComponent<TextMeshProUGUI>();
                var picture = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.one / 2);
                card.Show(new ThemeCard(picture, "theme.dogs", "0/3"));
                Assert.That(title.text, Is.EqualTo(names["en"]));

                // The new locale's tables load asynchronously; batch mode runs thousands of frames a second, so wait by time.
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("ru");
                var since = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - since < 10 && title.text != names["ru"])
                    yield return null;
                Assert.That(title.text, Is.EqualTo(names["ru"]));
            }
            finally
            {
                Object.Destroy(canvas);
            }
        }

        [UnityTearDown]
        public IEnumerator LeavePlayMode()
        {
            if (Application.isPlaying)
                yield return new ExitPlayMode();
            var startScene = SessionState.GetString(StartSceneKey, "");
            if (startScene.Length > 0)
                EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(startScene);
            var playMode = SessionState.GetInt(PlayModeKey, -1);
            if (playMode >= 0)
                AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilderIndex = playMode;
            SessionState.EraseString(StartSceneKey);
            SessionState.EraseInt(PlayModeKey);
        }
    }
}
