using System;
using System.Linq;
using NUnit.Framework;
using Scripts.Configs;
using Scripts.Models;
using UnityEditor;

namespace Fives.UI.Tests
{
    /// <summary>Screens and board parts are referenced from the configs; a missing reference fails here, not in the game.</summary>
    public class ConfigReferenceTests
    {
        private static GlobalConfig Config => AssetDatabase.LoadAssetAtPath<GlobalConfig>("Assets/Configs/GameConfig.asset");

        [Test]
        public void EveryGameState_HasAScreen()
        {
            var states = Config.StateConfigs;

            Assert.That(states.Select(state => state.StateName), Is.EquivalentTo(Enum.GetValues(typeof(GameStateType))));
            Assert.That(states.Where(state => state.ScreenPrefab == null).Select(state => state.name), Is.Empty);
        }

        [Test]
        public void TheBoard_HasItsPrefabs()
        {
            Assert.That(Config.BoardPrefab, Is.Not.Null);
            Assert.That(Config.TilePrefab, Is.Not.Null);
        }
    }
}
