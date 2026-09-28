using System;
using System.Collections;
using Fives.UI.Views;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Fives.UI.Tests
{
    /// <summary>Префаб экрана партии: ссылки на элементы управления и жизненный цикл экрана.</summary>
    public class GamePlayPrefabTests
    {
        private const string PrefabPath = "Assets/Content/Prefabs/UI/Screens/GamePlayScreen.prefab";
        // Вход в Play Mode перезагружает домен и пересоздаёт этот класс, поэтому то, что нужно вернуть после теста,
        // хранится в SessionState, а не в полях.
        private const string StartSceneKey = "Fives.Tests.PlayModeStartScene";
        private const string PlayModeKey = "Fives.Tests.AddressablesPlayMode";

        /// <summary>
        /// Экран партии дважды создаётся в Play Mode, обновляет счётчик и кнопки, прячется, показывается и
        /// уничтожается без ошибок в логе.
        /// </summary>
        [UnityTest]
        public IEnumerator ScreenCanOpenRefreshAndReopenWithoutExceptions()
        {
            // Экран проверяется отдельно: Play Mode не должен запускать игру, а тексты берутся из базы ассетов
            // при любом режиме Addressables на этой машине, так что ничего не грузится из бандлов и сети.
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
                    yield return null; // Awake, OnEnable и первый кадр на настоящих объектах Unity.
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
                    yield return null; // ещё кадр: OnDestroy и отписка слушателей
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

        /// <summary>
        /// После импорта префаба ссылки BoardControlsView на кнопки и подписи на месте и ведут внутрь панели
        /// управления.
        /// </summary>
        [TestCase("undoButton", typeof(Button))]               // кнопка Undo
        [TestCase("hintButton", typeof(Button))]               // кнопка подсказки
        [TestCase("hintPriceLabel", typeof(TextMeshProUGUI))]  // цена подсказки
        [TestCase("movesLabel", typeof(TextMeshProUGUI))]      // счётчик ходов
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
