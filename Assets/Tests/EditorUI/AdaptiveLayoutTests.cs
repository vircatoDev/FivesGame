using System.Linq;
using NUnit.Framework;
using Scripts.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fives.UI.Tests
{
    /// <summary>Landscape screens from 4:3 to 21:9: nothing clipped, backgrounds cover, header inside the safe area.</summary>
    public sealed class AdaptiveLayoutTests
    {
        private const string ScenePath = "Assets/Scenes/MainGame.unity";

        [Test]
        public void SceneCanvas_ExpandsInsteadOfClipping_AndKeepsTheHeaderSafe()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var canvas = scene.GetRootGameObjects().Select(go => go.GetComponent<CanvasScaler>()).Single(scaler => scaler != null);
                Assert.That(canvas.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
                Assert.That(canvas.screenMatchMode, Is.EqualTo(CanvasScaler.ScreenMatchMode.Expand));
                AssertCovers(canvas.transform.Find("UIRoot/Back"));
                Assert.That(canvas.transform.Find("CommonUILyaer").GetComponent<SafeAreaFitter>(), Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void GameplayBackground_CoversTheScreen()
        {
            var screen = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/UI/Screens/GamePlayScreen.prefab");
            AssertCovers(screen.transform.Find("Background"));
        }

        [Test]
        public void Gameplay_SidePanelsHugTheSafeEdges_AndTheBoardIsCentred()
        {
            var panel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/UI/Screens/GamePlayScreen.prefab").transform.Find("Panel");
            Assert.That(panel.GetComponent<SafeAreaFitter>(), Is.Not.Null);
            Assert.That(((RectTransform)panel.Find("BoardControls")).anchorMin.x, Is.EqualTo(0f));
            Assert.That(((RectTransform)panel.Find("Preview")).anchorMin.x, Is.EqualTo(1f));
            Assert.That(((RectTransform)panel.Find("Information")).anchorMin.x, Is.EqualTo(1f));

            var board = (RectTransform)AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/GamePlayElements/Board.prefab").transform;
            Assert.That(board.anchoredPosition.x + board.sizeDelta.x / 2, Is.EqualTo(0f).Within(0.5f), "board centre (pivot is top-left)");
        }

        private static void AssertCovers(Transform background)
        {
            var fitter = background.GetComponent<AspectRatioFitter>();
            var sprite = background.GetComponent<Image>().sprite;
            Assert.That(fitter.aspectMode, Is.EqualTo(AspectRatioFitter.AspectMode.EnvelopeParent), background.name);
            Assert.That(fitter.aspectRatio, Is.EqualTo(sprite.rect.width / sprite.rect.height).Within(0.01f), background.name);
        }
    }
}
