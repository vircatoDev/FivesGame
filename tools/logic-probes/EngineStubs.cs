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
    public class Object
    {
        public bool Destroyed;
        public static void Destroy(Object value) { if (value != null) value.Destroyed = true; }
    }
    public class GameObject : Object
    {
        public GameObject(string name = "") { }
        public T AddComponent<T>() where T : new() => new T();
    }
    public class Transform : Object
    {
        public Vector3 position;
        public GameObject gameObject = new GameObject();
        public int childCount;
        public Transform GetChild(int index) => throw new InvalidOperationException("No visual hierarchy in logic probes.");
    }
    public class MonoBehaviour : Object
    {
        public GameObject gameObject = new GameObject();
        public Transform transform = new Transform();
    }
    public class Texture2D : Object { }
    public class CanvasGroup : Object { }
    public struct Color { public static Color white; public static Color clear; }
    public class AudioSource : Object
    {
        public static float LastVolume;
        public bool loop;
        public float volume;
        public AudioClip clip;
        public void Play() { }
        public void Stop() { }
        public static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume) { LastVolume = volume; }
    }
    public class Camera { public static Camera main = new Camera(); public Transform transform = new Transform(); }
    public static class PlayerPrefs
    {
        private static readonly System.Collections.Generic.Dictionary<string, string> Values = new();
        public static bool HasKey(string key) => Values.ContainsKey(key);
        public static string GetString(string key) => Values[key];
        public static void SetString(string key, string value) => Values[key] = value;
        public static void Save() { }
        public static void DeleteAll() => Values.Clear();
    }
    public class Sprite : Object { public Texture2D texture; }
    public class AudioClip { }
    public class RectTransform { public Vector2 anchoredPosition; public bool IsChildOf(Transform parent) => false; public T[] GetComponentsInChildren<T>() => Array.Empty<T>(); }
    public static class Debug { public static void Log(object value) { } public static void LogError(object value) { } public static void LogWarning(object value) { } }
    public static class Time { public static float time; public static float realtimeSinceStartup; public static float unscaledDeltaTime = 0.1f; public static float deltaTime = 1; }
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
        public static Vector3 one => new Vector3(1, 1, 1);
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
        public static UniTask Yield() => new UniTask { Inner = new TaskCompletionSource<bool>().Task };
        public void Forget() { }
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

namespace Scripts.UI.Views
{
    public class BaseView { }
    public class MainMenuView : BaseView
    {
        public readonly TaskCompletionSource<bool> HideCompletion = new TaskCompletionSource<bool>();
        public Cysharp.Threading.Tasks.UniTask PlayHideAnimation() => new Cysharp.Threading.Tasks.UniTask { Inner = HideCompletion.Task };
        public Cysharp.Threading.Tasks.UniTask PlayShowAnimation() => default;
        public void UpdateViewContent(string progress, string name, UnityEngine.Sprite sprite) { }
    }
    public class HeaderPanelView
    {
        public string EnergyText;
        public void UpdateViewContent(string stars, string energy) { EnergyText = energy; }
        public void UpdateCurrency(in Scripts.Components.CurrencyChangedEvent e)
        {
            if (e.Currency == Scripts.Models.Currency.Energy) EnergyText = e.Balance.ToString();
        }
        public void UpdateButtonLogic(Scripts.Components.UpdateControlPanelBtnLogicEvent e) { }
    }
    public class SelectMenuView : BaseView
    {
        public void UpdateViewContent(Scripts.Models.MenuItemData[] data, string title, Action<string> callback, bool animate) { }
        public void UnlockThemeItemByName(Scripts.Models.MenuItemData data, Action<string> callback) { }
        public readonly TaskCompletionSource<bool> HideCompletion = new TaskCompletionSource<bool>();
        public Cysharp.Threading.Tasks.UniTask PlayHideAnimation() => new Cysharp.Threading.Tasks.UniTask { Inner = HideCompletion.Task };
    }
    public class GameResultView : BaseView
    {
        public void UpdateViewContent(Scripts.Models.GameResult result, Scripts.Configs.ThemeConfig theme, Scripts.Models.PuzzleData puzzle) { }
        public Cysharp.Threading.Tasks.UniTask PlayShowAnimation() => default;
    }
}

namespace UnityEngine.UI
{
    public class Graphic { public bool raycastTarget; public UnityEngine.Color color; }
}
namespace UnityEngine.EventSystems
{
    public class PointerEventData { }
    public interface IPointerClickHandler { void OnPointerClick(PointerEventData eventData); }
}
namespace DG.Tweening
{
    public enum Ease { OutBack }
    public enum LinkBehaviour { KillOnDestroy }
    public class Tween
    {
        public Tween SetEase(Ease ease) => this;
        public Tween SetDelay(float delay) => this;
        public Tween SetLink(UnityEngine.GameObject target, LinkBehaviour behaviour) => this;
        public Tween OnComplete(System.Action action) => this;
    }
    public static class TweenExtensions
    {
        public static Tween DOFade(this UnityEngine.AudioSource target, float end, float duration) => new Tween();
        public static Tween DOFade(this UnityEngine.CanvasGroup target, float end, float duration) => new Tween();
        public static Tween DOScale(this UnityEngine.Transform target, UnityEngine.Vector3 end, float duration) => new Tween();
    }
}
