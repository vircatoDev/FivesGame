namespace Scripts.Services.Interfaces
{
    /// <summary>Frame timing for ECS systems; tests drive it by hand instead of UnityEngine.Time.</summary>
    public interface IFrameTime
    {
        float Time { get; }
        float DeltaTime { get; }
        float UnscaledDeltaTime { get; }
        float RealtimeSinceStartup { get; }
    }
}
