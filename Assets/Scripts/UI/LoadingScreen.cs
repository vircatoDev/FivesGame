using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    /// <summary>First screen of the game: a progress bar while the loading steps run, then a fade into the main menu.</summary>
    public sealed class LoadingScreen : MonoBehaviour
    {
        private const float ProgressDuration = 0.25f;
        private const float FadeDuration = 0.35f;

        [SerializeField] private CanvasGroup group;
        [SerializeField] private Slider progress;

        public void SetProgress(float value) => progress.DOValue(value, ProgressDuration).SetLink(gameObject);

        public async UniTask Hide()
        {
            group.blocksRaycasts = false;
            await group.DOFade(0, FadeDuration).SetLink(gameObject).AsyncWaitForCompletion();
            gameObject.SetActive(false);
        }
    }
}
