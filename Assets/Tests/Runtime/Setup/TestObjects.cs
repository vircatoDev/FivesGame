using System;
using System.Collections.Generic;
using Fives.Configs;
using Fives.Models;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Fives.Runtime.Tests
{
    /// <summary>The content and balance every test starts from; see <see cref="TestObjects.Config"/>.</summary>
    internal static class TestConfig
    {
        public const int Stars = 200;
        public const int Energy = 5;
        public const int MaxEnergy = 10;
        public const int RewardStars = 10;
        public const int HintPrice = 5;
        /// <summary>The price of the locked "cities" theme; the "dogs" theme is free and unlocked.</summary>
        public const int CitiesPrice = 60;
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

        /// <summary>Two themes without puzzles: "cities", locked, and "dogs", free and unlocked.</summary>
        public GlobalConfig Config(int stars = TestConfig.Stars)
        {
            var config = Asset<GlobalConfig>();
            config.InitialStars = stars;
            config.InitialEnergy = TestConfig.Energy;
            config.MaxEnergy = TestConfig.MaxEnergy;
            config.EnergyRecoveryIntervalHours = 1;
            config.RewardStars = TestConfig.RewardStars;
            config.HintPrice = TestConfig.HintPrice;
            config.DefaultUnlockedThemes = new[] { "dogs" };
            config.Themes = new List<ThemeConfig> { Theme("cities", "Cities", TestConfig.CitiesPrice), Theme("dogs", "Dogs", 0) };
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

        /// <summary>The game's board: 4 columns by 3 rows.</summary>
        public GameSettings Board()
        {
            var mode = Asset<GameSettings>();
            mode.Columns = 4;
            mode.Rows = 3;
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
}
