using System;
using System.Collections;
using NUnit.Framework;
using Scripts.UI.Views;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace Fives.UI.Tests
{
    public class GamePlayPrefabTests
    {
        private const string PrefabPath = "Assets/Resources/Prefabs/UI/Screens/GamePlayScreen.prefab";

        [UnityTest]
        public IEnumerator ScreenCanOpenRefreshAndReopenWithoutExceptions()
        {
            yield return new EnterPlayMode();
            var canvas = new GameObject("Gameplay UI smoke test", typeof(Canvas));
            try
            {
                for (var attempt = 0; attempt < 2; attempt++)
                {
                    var screen = UnityEngine.Object.Instantiate(
                        Resources.Load<GameObject>("Prefabs/UI/Screens/GamePlayScreen"), canvas.transform);
                    yield return null; // Run Awake, OnEnable and the first frame on real Unity objects.
                    var view = screen.GetComponent<GamePlayView>();
                    var controls = screen.transform.Find("Panel/BoardControls");
                    Assert.That(controls, Is.Not.Null);
                    var undo = controls.Find("Undo").GetComponent<Button>();
                    var redo = controls.Find("Redo").GetComponent<Button>();
                    var moves = controls.Find("Moves/Label").GetComponent<TextMeshProUGUI>();
                    Assert.That(undo.interactable, Is.False);
                    Assert.That(redo.interactable, Is.False);

                    view.UpdateControls("Moves: 2", true, false);
                    Assert.That(moves.text, Is.EqualTo("Moves: 2"));
                    Assert.That(undo.interactable, Is.True);
                    Assert.That(redo.interactable, Is.False);

                    view.UpdateControls("Moves: 1", true, true);
                    Assert.That(moves.text, Is.EqualTo("Moves: 1"));
                    Assert.That(undo.interactable && redo.interactable, Is.True);
                    screen.SetActive(false);
                    screen.SetActive(true);
                    UnityEngine.Object.Destroy(screen);
                    yield return null; // Include OnDestroy and listener cleanup.
                    LogAssert.NoUnexpectedReceived();
                }
            }
            finally
            {
                UnityEngine.Object.Destroy(canvas);
            }
        }

        [UnityTearDown]
        public IEnumerator LeavePlayMode()
        {
            if (Application.isPlaying)
                yield return new ExitPlayMode();
        }

        [TestCase("undoButton", typeof(Button))]
        [TestCase("redoButton", typeof(Button))]
        [TestCase("movesLabel", typeof(TextMeshProUGUI))]
        public void ImportedPrefabHasControlReference(string field, Type expectedType)
        {
            AssetDatabase.ImportAsset(PrefabPath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);

            var controls = prefab.GetComponentInChildren<BoardControlsView>(true);
            Assert.That(controls, Is.Not.Null);
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
