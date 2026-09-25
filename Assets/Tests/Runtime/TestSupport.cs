using UnityEngine.AddressableAssets;
using Scripts.Services;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Models;
using Scripts.Services.Interfaces;
using Scripts.UI.Views;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Fives.Runtime.Tests
{
    internal sealed class MemoryStorage : IStorageService
    {
        public int Saves;
        public GameSaveData Data;
        public void Save<T>(string key, T data) { Data = (GameSaveData)(object)data; Saves++; }
        public T Load<T>(string key, T fallback = default) => Data == null ? fallback : (T)(object)Data;
    }

    internal sealed class FakeClock : IClock
    {
        public DateTime UtcNow { get; set; }
    }

    /// <summary>Frame time driven by the test: every Run advances by DeltaTime only when the test says so.</summary>
    internal sealed class FakeFrameTime : IFrameTime
    {
        public float Time { get; set; }
        public float DeltaTime { get; set; } = 0.1f;
        public float UnscaledDeltaTime { get; set; } = 0.1f;
        public float RealtimeSinceStartup { get; set; }
    }

    /// <summary>Owns the Unity objects a test creates and destroys them afterwards.</summary>
    internal sealed class TestObjects : IDisposable
    {
        private readonly List<Object> _objects = new List<Object>();

        public T Track<T>(T obj) where T : Object
        {
            _objects.Add(obj);
            return obj;
        }

        public T Asset<T>() where T : ScriptableObject => Track(ScriptableObject.CreateInstance<T>());

        public Sprite Sprite(string name)
        {
            var texture = Track(new Texture2D(2, 2) { name = name });
            var sprite = Track(UnityEngine.Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.zero));
            sprite.name = name;
            return sprite;
        }

        public RectTransform Rect(string name) => Track(new GameObject(name, typeof(RectTransform))).GetComponent<RectTransform>();

        public GlobalConfig Config(int stars = 200)
        {
            var config = Asset<GlobalConfig>();
            config.InitialStars = stars;
            config.InitialEnergy = 5;
            config.MaxEnergy = 10;
            config.EnergyRecoveryIntervalHours = 1;
            config.RewardStars = 10;
            config.DefaultUnlockedThemes = new[] { "dogs" };
            config.Themes = new List<ThemeConfig> { Theme("cities", "Cities", 60), Theme("dogs", "Dogs", 0) };
            return config;
        }

        public ThemeConfig Theme(string id, string name, int cost, params PuzzleData[] puzzles)
        {
            var theme = Asset<ThemeConfig>();
            theme.Id = id;
            theme.ThemeName = name;
            theme.UnlockCost = cost;
            theme.Preview = new AssetReferenceSprite(id + ".preview");
            theme.Puzzles = puzzles;
            return theme;
        }

        public PuzzleData[] Puzzles(string themeId, params string[] names)
        {
            var puzzles = new PuzzleData[names.Length];
            for (var i = 0; i < names.Length; i++)
                puzzles[i] = new PuzzleData { Id = $"{themeId}.{names[i].ToLowerInvariant()}", Name = names[i], Image = new AssetReferenceSprite(names[i]) };
            return puzzles;
        }

        public GameSettings Mode(int columns, int rows)
        {
            var mode = Asset<GameSettings>();
            mode.Columns = columns;
            mode.Rows = rows;
            mode.TileSize = 1;
            mode.TileSpacing = 0;
            return mode;
        }

        public void Dispose()
        {
            foreach (var obj in _objects)
            {
                if (obj == null) continue;
                var go = obj is Component component ? component.gameObject : obj;
                Object.DestroyImmediate(go);
            }
            _objects.Clear();
        }
    }

    internal static class EcsTestExtensions
    {
        public static int Count<T>(this EcsWorld world) where T : struct =>
            world.GetFilter(typeof(EcsFilter<T>)).GetEntitiesCount();

        public static T[] All<T>(this EcsWorld world) where T : struct
        {
            var filter = (EcsFilter<T>)world.GetFilter(typeof(EcsFilter<T>));
            var items = new T[filter.GetEntitiesCount()];
            var n = 0;
            foreach (var i in filter)
                items[n++] = filter.Get1(i);
            return items;
        }

        public static void Tick(this EcsSystems systems, int frames = 1)
        {
            for (var i = 0; i < frames; i++)
                systems.Run();
        }
    }

    internal class FakeView : IView
    {
        public UniTaskCompletionSource Hide = new UniTaskCompletionSource();
        public UniTask PlayShowAnimation() => UniTask.CompletedTask;
        public UniTask PlayHideAnimation() => Hide.Task;
    }

    internal sealed class FakeMainMenuView : FakeView, IMainMenuView
    {
        public ThemeCard Previous, Current, Next;
        public int Direction;
        public bool CanBrowse;
        public bool IsSliding => false;

        public void ShowThemes(in ThemeCard previous, in ThemeCard current, in ThemeCard next, int direction, bool canBrowse)
        {
            Previous = previous; Current = current; Next = next; Direction = direction; CanBrowse = canBrowse;
        }
    }

    internal sealed class FakeSelectMenuView : FakeView, ISelectMenuView
    {
        public MenuItemData[] Items;
        public Action<string> OnClick;
        public MenuItemData Unlocked;

        public void UpdateViewContent(MenuItemData[] newContent, string titleText, Action<string> onClick, bool playAnimation, int centeredItem = 0)
        {
            Items = newContent;
            OnClick = onClick;
        }

        public void UnlockThemeItemByName(MenuItemData itemData, Action<string> onTileClick) => Unlocked = itemData;
    }

    internal sealed class FakeGamePlayView : FakeView, IGamePlayView
    {
        public int Updates;
        public string Moves = "";
        public bool CanUndo, CanHint;
        public string HintPrice = "";
        public void UpdateViewContent(Sprite image, string title, string about) { }
        public void UpdateControls(string moves, string hintPrice, bool canUndo, bool canHint) { Updates++; Moves = moves; HintPrice = hintPrice; CanUndo = canUndo; CanHint = canHint; }
    }

    internal sealed class FakeGameResultView : FakeView, IGameResultView
    {
        public string Progress;
        public void UpdateViewContent(string stars, string stats, string themeName, ThemeProgress progress) => Progress = progress.ToString();
    }

    /// <summary>Returns the key, with arguments after a colon, so tests do not depend on a language.</summary>
    internal sealed class FakeTexts : ITexts
    {
        public UniTask Ready() => UniTask.CompletedTask;
        public string Get(string key, params object[] args) => args.Length == 0 ? key : key + ":" + string.Join(",", args);
    }

    /// <summary>Hands back one sprite per reference at once and remembers what each owner still holds.</summary>
    internal sealed class FakeSpriteLoader : ISpriteLoader
    {
        private readonly TestObjects _objects;
        private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();
        public readonly Dictionary<object, int> Held = new Dictionary<object, int>();

        public FakeSpriteLoader(TestObjects objects) => _objects = objects;

        public Sprite Of(AssetReferenceSprite reference)
        {
            if (!_sprites.TryGetValue(reference.AssetGUID, out var sprite))
                _sprites[reference.AssetGUID] = sprite = _objects.Sprite(reference.AssetGUID);
            return sprite;
        }

        public UniTask<Sprite> Load(AssetReferenceSprite sprite, object owner)
        {
            Held[owner] = Held.TryGetValue(owner, out var count) ? count + 1 : 1;
            return UniTask.FromResult(Of(sprite));
        }

        public void Release(object owner) => Held.Remove(owner);

        /// <summary>Previews as the loading screen leaves them: loaded for every theme.</summary>
        public ThemePreviews Previews(GlobalConfig config)
        {
            var previews = new ThemePreviews(config, this);
            previews.Load().GetAwaiter().GetResult();
            return previews;
        }
    }

    internal sealed class FakeHeaderPanelView : IHeaderPanelView
    {
        public string Energy;
        public HeaderBtnType Button;
        public Action OnButton;
        public void UpdateViewContent(string starsAmount, string energyAmount) => Energy = energyAmount;

        public void UpdateCurrency(in CurrencyChangedEvent evt)
        {
            if (evt.Currency == Currency.Energy)
                Energy = evt.Balance.ToString();
        }

        public void ShowButton(HeaderBtnType type, Action onClick)
        {
            Button = type;
            OnButton = onClick;
        }
    }
}
