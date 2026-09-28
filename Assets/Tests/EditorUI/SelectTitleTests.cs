using Fives.Configs;
using Fives.Editor.Localization;
using Fives.Services;
using Fives.UI;
using Fives.UI.Views;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Fives.UI.Tests
{
    /// <summary>Заголовок экрана выбора на табличке: первая строка на облаке, вторая на доске.</summary>
    public sealed class SelectTitleTests
    {
        private const string PrefabPath = "Assets/Content/Prefabs/UI/Screens/SelectMenuScreen.prefab";

        /// <summary>
        /// «Выбери тему» и «Выбери пазл» на каждом языке игры встают на табличку ровно двумя строками. Каждая строка
        /// помещается на облако или доску (поля текста в префабе): длинная сжимается по ширине, а размер шрифта не
        /// меняется, иначе строки сползли бы к стыку облака и доски.
        /// </summary>
        [Test]
        public void SelectTitles_TakeTwoLinesThatFitTheCloudAndPlank_InEveryLanguage()
        {
            var sheet = ExcelStringImporter.Read(ExcelStringImporter.SheetPath);
            var languages = AssetDatabase.LoadAssetAtPath<GlobalConfig>("Assets/Configs/GameConfig.asset").Languages;
            // A UI text lays itself out only under a canvas.
            var canvas = new GameObject("Select title test", typeof(Canvas));
            try
            {
                var screen = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), canvas.transform);
                var view = new SerializedObject(screen.GetComponentInChildren<SelectMenuView>(true));
                var title = ((RectTransform)view.FindProperty("textContainer").objectReferenceValue).GetComponentInChildren<TextMeshProUGUI>(true);
                var width = title.rectTransform.rect.width - title.margin.x - title.margin.z;

                foreach (var key in new[] { TextKeys.SelectThemes, TextKeys.SelectPuzzles })
                foreach (var language in languages)
                {
                    title.text = TwoLineTitle.Split(sheet[key][language]);
                    title.ForceMeshUpdate(true);

                    Assert.That(title.textInfo.lineCount, Is.EqualTo(2), $"{key} in {language}");
                    Assert.That(title.GetRenderedValues(true).x, Is.LessThanOrEqualTo(width), $"{key} in {language}");
                    Assert.That(title.fontSize, Is.EqualTo(title.fontSizeMax).Within(0.01f), $"{key} in {language}");
                }
            }
            finally
            {
                Object.DestroyImmediate(canvas);
            }
        }

        /// <summary>Фраза делится там, где две строки ближе всего по длине; если вариантов два, первая строка короче.</summary>
        [TestCase("SELECT THEME", "SELECT\nTHEME")]
        [TestCase("CHOISIS UN THÈME", "CHOISIS\nUN THÈME")] // 7 и 8 букв ровнее, чем 10 и 5
        [TestCase("WÄHLE EIN THEMA", "WÄHLE\nEIN THEMA")] // 5 и 9 или 9 и 5: артикль остаётся с существительным
        [TestCase("SELECT<br>THEME", "SELECT\nTHEME")] // перенос из текста не даёт третью строку
        [TestCase("PUZZLE", "PUZZLE")] // одно слово не делится
        public void Title_BreaksWhereTheLinesAreClosestInLength(string title, string lines) =>
            Assert.That(TwoLineTitle.Split(title), Is.EqualTo(lines));
    }
}
