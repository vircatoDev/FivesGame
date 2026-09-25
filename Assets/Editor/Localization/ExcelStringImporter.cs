using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace Fives.Editor.Localization
{
    /// <summary>
    /// Localization/Strings.xlsx is the source of the "UI" string table: columns Key, Comment and one per locale code.
    /// Import replaces every value and removes keys that are no longer in the sheet.
    /// </summary>
    public static class ExcelStringImporter
    {
        public const string SheetPath = "Localization/Strings.xlsx";
        public const string TableName = "UI";

        [MenuItem("FivesGame/Localization/Import from Excel")]
        public static void ImportFromMenu()
        {
            var count = Import(SheetPath);
            Debug.Log($"Imported {count} keys from {SheetPath} into the {TableName} table.");
        }

        /// <summary>Rows keyed by the Key column: locale code to text.</summary>
        public static Dictionary<string, Dictionary<string, string>> Read(string path)
        {
            List<string[]> rows;
            using (var file = File.OpenRead(path))
                rows = XlsxReader.ReadFirstSheet(file);

            var header = rows[0];
            var keyColumn = Array.IndexOf(header, "Key");
            if (keyColumn < 0)
                throw new InvalidDataException($"{path}: the first row needs a Key column.");

            var entries = new Dictionary<string, Dictionary<string, string>>();
            foreach (var row in rows.Skip(1))
            {
                var key = Cell(row, keyColumn).Trim();
                if (key.Length == 0)
                    continue;
                if (entries.ContainsKey(key))
                    throw new InvalidDataException($"{path}: duplicate key {key}.");

                entries[key] = Enumerable.Range(0, header.Length)
                    .Where(column => column != keyColumn && header[column] != "Comment" && header[column].Length > 0)
                    .ToDictionary(column => header[column], column => Cell(row, column));
            }
            return entries;
        }

        public static int Import(string path)
        {
            var entries = Read(path);
            var collection = LocalizationEditorSettings.GetStringTableCollection(TableName);
            foreach (var table in collection.StringTables)
            {
                var code = table.LocaleIdentifier.Code;
                foreach (var (key, values) in entries)
                {
                    if (!values.TryGetValue(code, out var text))
                        throw new InvalidDataException($"{path}: no column for locale {code}.");
                    table.AddEntry(key, text);
                }
                EditorUtility.SetDirty(table);
            }

            foreach (var stale in collection.SharedData.Entries.Select(entry => entry.Key).Where(key => !entries.ContainsKey(key)).ToList())
                collection.RemoveEntry(stale);

            EditorUtility.SetDirty(collection.SharedData);
            AssetDatabase.SaveAssets();
            return entries.Count;
        }

        private static string Cell(string[] row, int column) => column < row.Length ? row[column] : "";
    }
}
