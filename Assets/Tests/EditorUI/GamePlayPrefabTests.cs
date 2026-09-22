using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
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
                    var view = screen.GetComponents<MonoBehaviour>()
                        .Single(component => component.GetType().FullName == "Scripts.UI.Views.GamePlayView");
                    var controls = screen.transform.Find("Panel/BoardControls");
                    Assert.That(controls, Is.Not.Null);
                    var undo = controls.Find("Undo").GetComponent<Button>();
                    var replay = controls.Find("Replay").GetComponent<Button>();
                    var status = controls.Find("Status/Label").GetComponent<TextMeshProUGUI>();
                    var replayLabel = controls.Find("Replay/Label").GetComponent<TextMeshProUGUI>();
                    Assert.That(undo.interactable, Is.False);
                    Assert.That(replay.interactable, Is.False);

                    var refresh = view.GetType().GetMethod("UpdateControls");
                    Assert.That(refresh, Is.Not.Null);
                    refresh.Invoke(view, new object[] { "Ходов: 2", true, true, false });
                    Assert.That(status.text, Is.EqualTo("Ходов: 2"));
                    Assert.That(undo.interactable && replay.interactable, Is.True);
                    Assert.That(replayLabel.text, Is.EqualTo("Повтор"));

                    refresh.Invoke(view, new object[] { "Повтор: 1/2", false, true, true });
                    Assert.That(status.text, Is.EqualTo("Повтор: 1/2"));
                    Assert.That(undo.interactable, Is.False);
                    Assert.That(replay.interactable, Is.True);
                    Assert.That(replayLabel.text, Is.EqualTo("Стоп"));
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
