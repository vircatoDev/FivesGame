using Scripts.Services.Interfaces;

namespace Scripts.Services
{
    public sealed class UnityFrameTime : IFrameTime
    {
        public float Time => UnityEngine.Time.time;
        public float DeltaTime => UnityEngine.Time.deltaTime;
        public float UnscaledDeltaTime => UnityEngine.Time.unscaledDeltaTime;
        public float RealtimeSinceStartup => UnityEngine.Time.realtimeSinceStartup;
    }
}
