using System.IO;
using System.Linq;
using System.Xml;
using Fives.Editor;
using NUnit.Framework;

namespace Fives.UI.Tests
{
    /// <summary>Манифест Android для детского приложения: без рекламного идентификатора AD_ID.</summary>
    public class ChildDirectedManifestTests
    {
        private const string Android = "http://schemas.android.com/apk/res/android";
        private const string Tools = "http://schemas.android.com/tools";
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _path = Path.GetTempFileName();
            File.WriteAllText(_path,
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<manifest xmlns:android=\"" + Android + "\" package=\"com.fives.game\">\n" +
                "  <uses-permission android:name=\"android.permission.INTERNET\" />\n" +
                "  <uses-permission android:name=\"" + ChildDirectedManifest.AdIdPermission + "\" />\n" +
                "  <application />\n" +
                "</manifest>\n");
        }

        [TearDown]
        public void TearDown() => File.Delete(_path);

        private XmlElement[] Permissions(string name)
        {
            var document = new XmlDocument();
            document.Load(_path);
            return document.DocumentElement.SelectNodes("uses-permission").Cast<XmlElement>()
                .Where(permission => permission.GetAttribute("name", Android) == name).ToArray();
        }

        /// <summary>
        /// Разрешение AD_ID превращается в пометку «удалить», и повторная обработка её не дублирует.
        /// </summary>
        [Test]
        public void TheAdIdPermission_BecomesARemoveMarker_Once()
        {
            ChildDirectedManifest.RemoveAdId(_path);
            ChildDirectedManifest.RemoveAdId(_path);

            var adId = Permissions(ChildDirectedManifest.AdIdPermission);
            Assert.That(adId, Has.Length.EqualTo(1));
            Assert.That(adId[0].GetAttribute("node", Tools), Is.EqualTo("remove"));
        }

        /// <summary>Остальные разрешения, например INTERNET, остаются.</summary>
        [Test]
        public void OtherPermissions_Stay()
        {
            ChildDirectedManifest.RemoveAdId(_path);

            Assert.That(Permissions("android.permission.INTERNET"), Has.Length.EqualTo(1));
        }
    }
}
