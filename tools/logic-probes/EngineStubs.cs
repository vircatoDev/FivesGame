// Minimal engine/UI substitutes for isolated logic checks. They are not a Unity runtime.
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace UnityEngine
{
    public class SerializeField : Attribute { }
    public class Header : Attribute { public Header(string value) { } }
    public class CreateAssetMenuAttribute : Attribute { public string menuName; }
    public class ScriptableObject { }
    public class GameObject { }
    public class Sprite { }
    public class AudioClip { }
    public class RectTransform { public Vector2 anchoredPosition; }
    public static class Debug { public static void Log(object value) { } public static void LogError(object value) { } }
    public static class Time { public static float time; public static float deltaTime = 1; }
    public static class Mathf
    {
        public static float Max(float a, float b) => Math.Max(a, b);
        public static bool Approximately(float a, float b) => Math.Abs(a - b) < 0.00001f;
    }

    public struct Vector2
    {
        public float x;
        public float y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => new Vector2(a.x + (b.x - a.x) * Math.Clamp(t, 0, 1), a.y + (b.y - a.y) * Math.Clamp(t, 0, 1));
        public override string ToString() => $"({x}, {y})";
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
        public Vector3(float x, float y, float z = 0) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public Vector3 normalized
        {
            get
            {
                var distance = Distance(this, new Vector3());
                return distance == 0 ? new Vector3() : new Vector3(x / distance, y / distance, z / distance);
            }
        }
        public static float Distance(Vector3 a, Vector3 b) => (float)Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z));
        public static implicit operator Vector2(Vector3 value) => new Vector2(value.x, value.y);
        public override string ToString() => $"({x}, {y}, {z})";
    }

    public static class Random
    {
        private static readonly System.Random Source = new System.Random(123);
        public static int Range(int min, int max) => Source.Next(min, max);
    }
}

namespace Cysharp.Threading.Tasks
{
    [AsyncMethodBuilder(typeof(UniTaskBuilder))]
    public struct UniTask
    {
        internal Task Inner;
        public TaskAwaiter GetAwaiter() => (Inner ?? Task.CompletedTask).GetAwaiter();
        public static UniTask Yield() => default;
        public void Forget() => Inner?.GetAwaiter().GetResult();
    }

    public struct UniTaskBuilder
    {
        private AsyncTaskMethodBuilder _builder;
        public static UniTaskBuilder Create() => new UniTaskBuilder { _builder = AsyncTaskMethodBuilder.Create() };
        public UniTask Task => new UniTask { Inner = _builder.Task };
        public void SetResult() => _builder.SetResult();
        public void SetException(Exception exception) => _builder.SetException(exception);
        public void SetStateMachine(IAsyncStateMachine stateMachine) => _builder.SetStateMachine(stateMachine);
        public void Start<T>(ref T stateMachine) where T : IAsyncStateMachine => _builder.Start(ref stateMachine);
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine => _builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine => _builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
    }
}

namespace Scripts.Services
{
    public class SoundSettingsData { public float MusicVolume = 1; public float SoundEffectsVolume = 1; }
}

namespace Scripts.UI.Views
{
    public class BaseView { }
    public class SelectMenuView : BaseView
    {
        public void UpdateViewContent(Scripts.Models.MenuItemData[] data, string title, Action<string> callback, bool animate) { }
        public void UnlockThemeItemByName(Scripts.Models.MenuItemData data, Action<string> callback) { }
        public Cysharp.Threading.Tasks.UniTask PlayHideAnimation() => default;
    }
    public class GameResultView : BaseView
    {
        public void UpdateViewContent(Scripts.Models.GameResult result, Scripts.Configs.ThemeConfig theme, Scripts.Models.PuzzleData puzzle) { }
        public Cysharp.Threading.Tasks.UniTask PlayShowAnimation() => default;
    }
}
