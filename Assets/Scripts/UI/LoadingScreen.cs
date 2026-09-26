using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    /// <summary>
    /// First screen of the game: a progress bar while the loading steps run, then a fade into the main menu.
    /// It comes back over the menus while a theme downloads, with a retry when the download fails.
    /// </summary>
    public sealed class LoadingScreen : MonoBehaviour, IDownloadScreen
    {
        private const float ProgressDuration = 0.25f;
        private const float FadeDuration = 0.35f;

        [SerializeField] private CanvasGroup group;
        [SerializeField] private Slider progress;
        [Header("Failed download")]
        [SerializeField] private GameObject error;
        [SerializeField] private Button retry;
        [SerializeField] private Button back;

        // A Hide that is overtaken by Show while it fades must not switch the screen off.
        private int _shows;

        public void Show()
        {
            _shows++;
            group.DOKill();
            group.alpha = 1;
            group.blocksRaycasts = true;
            progress.DOKill();
            progress.value = 0;
            progress.gameObject.SetActive(true);
            error.SetActive(false);
            gameObject.SetActive(true);
        }

        public void SetProgress(float value) => progress.DOValue(value, ProgressDuration).SetLink(gameObject);

        public async UniTask<bool> AskRetry(CancellationToken cancellation)
        {
            progress.gameObject.SetActive(false);
            error.SetActive(true);
            using var answered = CancellationTokenSource.CreateLinkedTokenSource(cancellation, destroyCancellationToken);
            try
            {
                return await UniTask.WhenAny(retry.OnClickAsync(answered.Token), back.OnClickAsync(answered.Token)) == 0;
            }
            finally
            {
                answered.Cancel(); // stops waiting for the button that was not pressed
                error.SetActive(false);
                progress.DOKill();
                progress.value = 0;
                progress.gameObject.SetActive(true);
            }
        }

        public async UniTask Hide()
        {
            var shown = _shows;
            group.blocksRaycasts = false;
            await group.DOFade(0, FadeDuration).SetLink(gameObject).ToUniTask();
            if (shown == _shows)
                gameObject.SetActive(false);
        }
    }
}
