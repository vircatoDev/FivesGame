using System;
using System.Collections;
using NUnit.Framework;
using Scripts.UI.Views;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace Fives.UI.Tests
{
    public class GamePlayPrefabTests
    {
        private const string PrefabPath = "Assets/Content/Prefabs/UI/Screens/GamePlayScreen.prefab";
        // Entering play mode reloads the domain and recreates this fixture, so what is restored after the test is kept
        // in SessionState rather than in fields.
        private const string StartSceneKey = "Fives.Tests.PlayModeStartScene";
        private const string PlayModeKey = "Fives.Tests.AddressablesPlayMode";

        [UnityTest]
        public IEnumerator ScreenCanOpenRefreshAndReopenWithoutExceptions()
        {
            // The screen is tested on its own: play mode must not boot the game, and its texts come from the asset
            // database whatever Addressables play mode this machine uses, so nothing loads from bundles or the network.
            SessionState.SetString(StartSceneKey, AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
            EditorSceneManager.playModeStartScene = null;
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            SessionState.SetInt(PlayModeKey, settings.ActivePlayModeDataBuilderIndex);
            settings.ActivePlayModeDataBuilderIndex = settings.DataBuilders.FindIndex(builder => builder is BuildScriptFastMode);
            yield return new EnterPlayMode();
            var canvas = new GameObject("Gameplay UI smoke test", typeof(Canvas));
            try
            {
                for (var attempt = 0; attempt < 2; attempt++)
                {
                    var screen = UnityEngine.Object.Instantiate(
                        AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), canvas.transform);
                    yield return null; // Run Awake, OnEnable and the first frame on real Unity objects.
                    var view = screen.GetComponent<GamePlayView>();
                    var controls = screen.transform.Find("Panel/BoardControls");
                    Assert.That(controls, Is.Not.Null);
                    var undo = controls.Find("Undo").GetComponent<Button>();
                    var hint = controls.Find("Hint").GetComponent<Button>();
                    var moves = controls.Find("Moves/Label").GetComponent<TextMeshProUGUI>();
                    Assert.That(undo.interactable, Is.False);
                    Assert.That(hint.interactable, Is.False);

                    view.UpdateControls("Moves: 2", "5", true, false);
                    Assert.That(moves.text, Is.EqualTo("Moves: 2"));
                    Assert.That(undo.interactable, Is.True);
                    Assert.That(hint.interactable, Is.False);

                    view.UpdateControls("Moves: 1", "5", true, true);
                    Assert.That(moves.text, Is.EqualTo("Moves: 1"));
                    Assert.That(undo.interactable && hint.interactable, Is.True);
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
            var startScene = SessionState.GetString(StartSceneKey, "");
            if (startScene.Length > 0)
                EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(startScene);
            var playMode = SessionState.GetInt(PlayModeKey, -1);
            if (playMode >= 0)
                AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilderIndex = playMode;
            SessionState.EraseString(StartSceneKey);
            SessionState.EraseInt(PlayModeKey);
        }

        [TestCase("undoButton", typeof(Button))]
        [TestCase("hintButton", typeof(Button))]
        [TestCase("hintPriceLabel", typeof(TextMeshProUGUI))]
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
