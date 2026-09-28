using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace Fives.Editor.Localization
{
    /// <summary>
    /// Reads the first worksheet of an .xlsx file (a zip of XML parts) as rows of strings, without third-party libraries.
    /// Handles shared, inline and plain cell values; skipped cells come back empty.
    /// </summary>
    public static class XlsxReader
    {
        private static readonly XNamespace Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace Relations = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace PackageRelations = "http://schemas.openxmlformats.org/package/2006/relationships";

        public static List<string[]> ReadFirstSheet(Stream xlsx)
        {
            using var zip = new ZipArchive(xlsx, ZipArchiveMode.Read);
            var shared = zip.GetEntry("xl/sharedStrings.xml") is { } sharedPart
                ? Load(sharedPart).Root.Elements(Main + "si").Select(Text).ToList()
                : new List<string>();

            var rows = new List<string[]>();
            foreach (var row in Load(zip.GetEntry(FirstSheetPath(zip))).Descendants(Main + "row"))
            {
                var cells = new List<string>();
                foreach (var cell in row.Elements(Main + "c"))
                {
                    var column = Column((string)cell.Attribute("r"));
                    while (cells.Count < column) cells.Add("");
                    cells.Add(Value(cell, shared));
                }
                rows.Add(cells.ToArray());
            }
            return rows;
        }

        private static string FirstSheetPath(ZipArchive zip)
        {
            var id = (string)Load(zip.GetEntry("xl/workbook.xml")).Descendants(Main + "sheet").First().Attribute(Relations + "id");
            var target = (string)Load(zip.GetEntry("xl/_rels/workbook.xml.rels")).Root.Elements(PackageRelations + "Relationship")
                .First(relation => (string)relation.Attribute("Id") == id).Attribute("Target");
            return target.StartsWith("/") ? target.Substring(1) : "xl/" + target;
        }

        private static string Value(XElement cell, List<string> shared)
        {
            switch ((string)cell.Attribute("t"))
            {
                case "s": return shared[int.Parse(cell.Element(Main + "v").Value)];
                case "inlineStr": return Text(cell.Element(Main + "is"));
                default: return cell.Element(Main + "v")?.Value ?? "";
            }
        }

        // Rich text keeps its runs in <r><t>; phonetic hints (<rPh>) are not part of the value.
        private static string Text(XElement item) =>
            string.Concat(item.Descendants(Main + "t").Where(t => t.Parent.Name != Main + "rPh").Select(t => t.Value));

        // "C12" -> 2
        private static int Column(string reference)
        {
            var index = 0;
            foreach (var letter in reference.TakeWhile(char.IsLetter))
                index = index * 26 + (letter - 'A' + 1);
            return index - 1;
        }

        private static XDocument Load(ZipArchiveEntry entry)
        {
            using var stream = entry.Open();
            return XDocument.Load(stream);
        }
    }
}
