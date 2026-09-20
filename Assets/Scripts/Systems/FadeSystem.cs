using DG.Tweening;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;

namespace Scripts.Systems
{
    public class FadeSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly EcsFilter<PlayFadeAnimationEvent> _fadeEventFilter;
        private readonly CanvasGroup _fadeOverlay;
        private bool _isFading;

        public FadeSystem(CanvasGroup fadeOverlayCanvas)
        {
            _fadeOverlay = fadeOverlayCanvas;
        }

        public void Init()
        {
            if (_fadeOverlay != null)
            {
                _fadeOverlay.alpha = 0;
                _fadeOverlay.blocksRaycasts = false;
            }
        }

        public void Run()
        {
            foreach (var i in _fadeEventFilter)
            {
                if (_isFading || _fadeOverlay == null) continue;

                ref var fadeEvent = ref _fadeEventFilter.Get1(i);

                _isFading = true;
                _fadeOverlay.blocksRaycasts = true;

                switch (fadeEvent.FadeMode)
                {
                    case FadeMode.FadeIn:
                        PlayFadeIn(fadeEvent.Duration, fadeEvent.OnComplete);
                        break;

                    case FadeMode.FadeOut:
                        PlayFadeOut(fadeEvent.Duration, fadeEvent.OnComplete);
                        break;

                    case FadeMode.FadeInOut:
                        PlayFadeInOut(fadeEvent.Duration, fadeEvent.OnComplete);
                        break;
                }

                _fadeEventFilter.GetEntity(i).Destroy();
            }
        }

        private void PlayFadeIn(float duration, System.Action onComplete)
        {
            _fadeOverlay.alpha = 0;
            _fadeOverlay.DOFade(1, duration).OnComplete(() =>
            {
                _fadeOverlay.blocksRaycasts = false;
                _isFading = false;
                onComplete?.Invoke();
            });
        }

        private void PlayFadeOut(float duration, System.Action onComplete)
        {
            _fadeOverlay.alpha = 1;
            _fadeOverlay.DOFade(0, duration).OnComplete(() =>
            {
                _fadeOverlay.blocksRaycasts = false;
                _isFading = false;
                onComplete?.Invoke();
            });
        }

        private void PlayFadeInOut(float duration, System.Action onComplete)
        {
            _fadeOverlay.alpha = 0;
            _fadeOverlay.DOFade(1, duration / 2).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                _fadeOverlay.DOFade(0, duration / 2).SetEase(Ease.InExpo).OnComplete(() =>
                {
                    _fadeOverlay.blocksRaycasts = false;
                    _isFading = false;
                    onComplete?.Invoke();
                });
            });
        }
    }
}