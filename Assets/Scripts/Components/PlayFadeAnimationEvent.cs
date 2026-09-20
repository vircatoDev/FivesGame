using System;
using Scripts.Models;

namespace Scripts.Components
{
    public struct PlayFadeAnimationEvent
    {
        public float Duration; 
        public FadeMode FadeMode;
        public Action OnComplete;
    }
}