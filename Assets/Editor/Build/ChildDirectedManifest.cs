using System.IO;
using System.Linq;
using System.Xml;
using UnityEditor.Android;

namespace Fives.Editor
{
    /// <summary>
    /// Every player is treated as a child (Google Play Families policy), so the app must not read the advertising ID.
    /// Ads SDKs and Play services declare the AD_ID permission in their own manifests; a remove marker in the app's
    /// manifest keeps it out of the merged one, whichever library brings it.
    /// </summary>
    public sealed class ChildDirectedManifest : IPostGenerateGradleAndroidProject
    {
        public const string AdIdPermission = "com.google.android.gms.permission.AD_ID";
        private const string AndroidNamespace = "http://schemas.android.com/apk/res/android";
        private const string ToolsNamespace = "http://schemas.android.com/tools";

        // After the LevelPlay processor, which may add the permission itself.
        public int callbackOrder => 1000;

        /// <param name="path">The unityLibrary module; the app's own manifest is in the launcher module next to it.</param>
        public void OnPostGenerateGradleAndroidProject(string path) =>
            RemoveAdId(Path.Combine(path, "..", "launcher", "src", "main", "AndroidManifest.xml"));

        public static void RemoveAdId(string manifestPath)
        {
            var document = new XmlDocument();
            document.Load(manifestPath);
            var manifest = document.DocumentElement;

            foreach (var permission in manifest.SelectNodes("uses-permission").Cast<XmlElement>().ToList())
            {
                if (permission.GetAttribute("name", AndroidNamespace) == AdIdPermission)
                    manifest.RemoveChild(permission);
            }

            if (!manifest.HasAttribute("xmlns:tools"))
                manifest.SetAttribute("xmlns:tools", ToolsNamespace);

            var remove = document.CreateElement("uses-permission");
            remove.SetAttributeNode(Attribute(document, "android", "name", AndroidNamespace, AdIdPermission));
            remove.SetAttributeNode(Attribute(document, "tools", "node", ToolsNamespace, "remove"));
            manifest.AppendChild(remove);
            document.Save(manifestPath);
        }

        private static XmlAttribute Attribute(XmlDocument document, string prefix, string name, string ns, string value)
        {
            var attribute = document.CreateAttribute(prefix, name, ns);
            attribute.Value = value;
            return attribute;
        }
    }
}
