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

                var duration = fadeEvent.Duration;
                var tween = fadeEvent.FadeMode switch
                {
                    FadeMode.FadeIn => Fade(0, 1, duration),
                    FadeMode.FadeOut => Fade(1, 0, duration),
                    _ => DOTween.Sequence()
                        .Append(Fade(0, 1, duration / 2).SetEase(Ease.OutExpo))
                        .Append(_fadeOverlay.DOFade(0, duration / 2).SetEase(Ease.InExpo))
                };
                var onComplete = fadeEvent.OnComplete;
                tween.OnComplete(() =>
                {
                    _fadeOverlay.blocksRaycasts = false;
                    _isFading = false;
                    onComplete?.Invoke();
                });

                _fadeEventFilter.GetEntity(i).Destroy();
            }
        }

        private Tween Fade(float from, float to, float duration)
        {
            _fadeOverlay.alpha = from;
            return _fadeOverlay.DOFade(to, duration);
        }
    }
}