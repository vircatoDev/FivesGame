using System;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Fives.UI.Tests
{
    public class GamePlayPrefabTests
    {
        private const string PrefabPath = "Assets/Resources/Prefabs/UI/Screens/GamePlayScreen.prefab";

        [TestCase("undoButton", typeof(Button))]
        [TestCase("replayButton", typeof(Button))]
        [TestCase("replayLabel", typeof(TextMeshProUGUI))]
        [TestCase("statusLabel", typeof(TextMeshProUGUI))]
        public void ImportedPrefabHasControlReference(string field, Type expectedType)
        {
            AssetDatabase.ImportAsset(PrefabPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);

            // Runtime lives in Assembly-CSharp, which an asmdef cannot reference.
            var controls = prefab.GetComponentsInChildren<MonoBehaviour>(true)
                .Single(component => component != null
                    && component.GetType().FullName == "Scripts.UI.Views.BoardControlsView");
            using var serialized = new SerializedObject(controls);
            var property = serialized.FindProperty(field);
            Assert.That(property, Is.Not.Null, $"Missing serialized field: {field}");
            var reference = property.objectReferenceValue;
            Assert.That(reference, Is.Not.Null, $"Unity failed to deserialize BoardControlsView.{field}");
            Assert.That(reference, Is.InstanceOf(expectedType));
            Assert.That(((Component)reference).transform.IsChildOf(controls.transform), Is.True);
        }
    }
}
