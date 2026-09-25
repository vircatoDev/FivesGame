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
            theme.Puzzles = puzzles;
            return theme;
        }

        public PuzzleData[] Puzzles(string themeId, params string[] names)
        {
            var puzzles = new PuzzleData[names.Length];
            for (var i = 0; i < names.Length; i++)
                puzzles[i] = new PuzzleData { Id = $"{themeId}.{names[i].ToLowerInvariant()}", Name = names[i], Image = Sprite(names[i]) };
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
        public bool CanUndo, CanRedo;
        public void UpdateViewContent(PuzzleData selectedPuzzle) { }
        public void UpdateControls(string moves, bool canUndo, bool canRedo) { Updates++; Moves = moves; CanUndo = canUndo; CanRedo = canRedo; }
    }

    internal sealed class FakeGameResultView : FakeView, IGameResultView
    {
        public string Progress;
        public void UpdateViewContent(GameResult result, string themeName, ThemeProgress progress) => Progress = progress.ToString();
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
