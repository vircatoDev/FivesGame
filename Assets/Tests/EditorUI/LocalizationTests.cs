using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Fives.Editor.Localization;
using NUnit.Framework;
using Scripts.Configs;
using Scripts.Services;
using TMPro;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace Fives.UI.Tests
{
    public sealed class LocalizationTests
    {
        private const string FontPath = "Assets/Content/Fonts/Ru/Lilita One SDF.asset";
        private static Dictionary<string, Dictionary<string, string>> Sheet => ExcelStringImporter.Read(ExcelStringImporter.SheetPath);
        private static GlobalConfig Config => AssetDatabase.LoadAssetAtPath<GlobalConfig>("Assets/Configs/GameConfig.asset");

        [Test]
        public void EveryKey_HasTextInEveryLanguage()
        {
            foreach (var (key, texts) in Sheet)
                foreach (var language in Config.Languages)
                    Assert.That(texts.TryGetValue(language, out var text) && text.Length > 0, $"{key} has no {language} text");
        }

        [Test]
        public void Sheet_CoversKeysUsedByCodeAndContent()
        {
            var keys = TextKeys.Fixed
                .Concat(Config.Themes.Select(TextKeys.Name))
                .Concat(Config.Themes.SelectMany(theme => theme.Puzzles).SelectMany(puzzle => new[] { TextKeys.Name(puzzle), TextKeys.About(puzzle) }));

            Assert.That(keys.Where(key => !Sheet.ContainsKey(key)), Is.Empty);
        }

        [Test]
        public void PrefabTexts_ReferenceExistingKeys()
        {
            var used = new List<string>();
            foreach (var path in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/Prefabs" }).Select(AssetDatabase.GUIDToAssetPath))
            foreach (var text in AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(path).GetComponentsInChildren<LocalizeStringEvent>(true))
                used.Add(text.StringReference.TableEntryReference.Key);

            Assert.That(used, Is.Not.Empty);
            Assert.That(used.Where(key => !Sheet.ContainsKey(key)), Is.Empty);
        }

        [Test]
        public void HeaderButtonSkins_UseExistingKeys()
        {
            var header = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Resources/Prefabs/UI/Common/Controls.prefab")
                .GetComponentInChildren<Scripts.UI.Views.HeaderPanelView>(true);
            var skins = new SerializedObject(header).FindProperty("commonBtnSkins");
            var keys = Enumerable.Range(0, skins.arraySize).Select(i => skins.GetArrayElementAtIndex(i).FindPropertyRelative("TextKey").stringValue).ToList();

            Assert.That(keys, Is.Not.Empty);
            Assert.That(keys.Where(key => !Sheet.ContainsKey(key)), Is.Empty);
        }

        [Test]
        public void Texts_UseOnlyCharactersOfTheGameFont()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            var characters = string.Concat(Sheet.Values.SelectMany(texts => texts.Values)).Where(c => !char.IsControl(c)).Distinct();

            Assert.That(characters.Where(c => !font.HasCharacter(c)).Select(c => c.ToString()), Is.Empty);
        }

        [Test]
        public void ImportedTable_MatchesTheSheet()
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection(ExcelStringImporter.TableName);
            Assert.That(collection.SharedData.Entries.Select(entry => entry.Key), Is.EquivalentTo(Sheet.Keys), "run FivesGame > Localization > Import from Excel");

            foreach (var table in collection.StringTables)
                foreach (var (key, texts) in Sheet)
                    Assert.That(table.GetEntry(key)?.Value, Is.EqualTo(texts[table.LocaleIdentifier.Code]), $"{key} / {table.LocaleIdentifier.Code}");
        }

        [Test]
        public void EveryTable_IsPreloaded()
        {
            foreach (var table in LocalizationEditorSettings.GetStringTableCollection(ExcelStringImporter.TableName).StringTables)
                Assert.That(LocalizationEditorSettings.GetPreloadTableFlag(table), Is.True, table.LocaleIdentifier.Code);
        }

        [Test]
        public void SelectedLocale_SwitchesTheText()
        {
            var previous = LocalizationSettings.SelectedLocale;
            try
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("ru");
                Assert.That(new LocalizedTexts(null).Get(TextKeys.Play), Is.EqualTo("Играть"));
                Assert.That(new LocalizedTexts(null).Get(TextKeys.Moves, 7), Is.EqualTo("Ходы: 7"));

                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("de");
                Assert.That(new LocalizedTexts(null).Get(TextKeys.Play), Is.EqualTo("Spielen"));
            }
            finally
            {
                LocalizationSettings.SelectedLocale = previous;
            }
        }

        [Test]
        public void Reader_HandlesSharedStringsInlineStringsAndSkippedCells()
        {
            using var file = new MemoryStream();
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create, true))
            {
                Add(zip, "xl/workbook.xml", "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"S\" sheetId=\"1\" r:id=\"rId7\"/></sheets></workbook>");
                Add(zip, "xl/_rels/workbook.xml.rels", "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId7\" Target=\"worksheets/data.xml\"/></Relationships>");
                Add(zip, "xl/sharedStrings.xml", "<sst xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><si><t>Key</t></si><si><r><t>Bon</t></r><r><t>jour</t></r></si></sst>");
                Add(zip, "xl/worksheets/data.xml", "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>" +
                    "<row r=\"1\"><c r=\"A1\" t=\"s\"><v>0</v></c><c r=\"C1\" t=\"inlineStr\"><is><t>fr</t></is></c></row>" +
                    "<row r=\"2\"><c r=\"A2\" t=\"inlineStr\"><is><t>hello</t></is></c><c r=\"C2\" t=\"s\"><v>1</v></c></row>" +
                    "<row r=\"3\"><c r=\"A3\"><v>42</v></c></row></sheetData></worksheet>");
            }

            file.Position = 0;
            var rows = XlsxReader.ReadFirstSheet(file);

            Assert.That(rows[0], Is.EqualTo(new[] { "Key", "", "fr" }));
            Assert.That(rows[1], Is.EqualTo(new[] { "hello", "", "Bonjour" }));
            Assert.That(rows[2], Is.EqualTo(new[] { "42" }));
        }

        private static void Add(ZipArchive zip, string name, string xml)
        {
            using var writer = new StreamWriter(zip.CreateEntry(name).Open(), Encoding.UTF8);
            writer.Write(xml);
        }
    }
}
