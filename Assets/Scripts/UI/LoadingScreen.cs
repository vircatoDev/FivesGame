using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Fives.UI
{
    /// <summary>
    /// First screen of the game: a progress bar while the loading steps run, then a fade into the main menu.
    /// When the themes do not download, it asks the player to retry.
    /// </summary>
    public sealed class LoadingScreen : MonoBehaviour, ILoadingScreen
    {
        private const float ProgressDuration = 0.25f;
        private const float FadeDuration = 0.35f;

        [SerializeField] private CanvasGroup group;
        [SerializeField] private Slider progress;
        [Header("Failed download")]
        [SerializeField] private GameObject error;
        [SerializeField] private Button retry;

        public void SetProgress(float value) => progress.DOValue(value, ProgressDuration).SetLink(gameObject);

        public async UniTask WaitForRetry(CancellationToken cancellation)
        {
            progress.gameObject.SetActive(false);
            error.SetActive(true);
            try
            {
                using var closing = CancellationTokenSource.CreateLinkedTokenSource(cancellation, destroyCancellationToken);
                await retry.OnClickAsync(closing.Token);
            }
            finally
            {
                error.SetActive(false);
                progress.gameObject.SetActive(true);
            }
        }

        public async UniTask Hide()
        {
            group.blocksRaycasts = false;
            await group.DOFade(0, FadeDuration).SetLink(gameObject).ToUniTask();
            gameObject.SetActive(false);
        }
    }
}
