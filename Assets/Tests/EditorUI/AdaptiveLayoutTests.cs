using System.Linq;
using Fives.UI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fives.UI.Tests
{
    /// <summary>
    /// Горизонтальные экраны от 4:3 до 21:9: канвас растягивается, а не обрезается, фон перекрывает экран,
    /// шапка остаётся в безопасной зоне.
    /// </summary>
    public sealed class AdaptiveLayoutTests
    {
        private const string ScenePath = "Assets/Scenes/MainGame.unity";

        /// <summary>
        /// Канвас сцены MainGame масштабируется в режиме Expand, фон UIRoot/Back перекрывает экран, у шапки есть
        /// SafeAreaFitter.
        /// </summary>
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

        private static void AssertCovers(Transform background)
        {
            var fitter = background.GetComponent<AspectRatioFitter>();
            var sprite = background.GetComponent<Image>().sprite;
            Assert.That(fitter.aspectMode, Is.EqualTo(AspectRatioFitter.AspectMode.EnvelopeParent), background.name);
            Assert.That(fitter.aspectRatio, Is.EqualTo(sprite.rect.width / sprite.rect.height).Within(0.01f), background.name);
        }
    }
}
