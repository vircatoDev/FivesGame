using System;
using System.Linq;
using Fives.Configs;
using Fives.Models;
using NUnit.Framework;
using UnityEditor;

namespace Fives.UI.Tests
{
    /// <summary>Экраны и части поля подключены через конфиги: пропавшая ссылка ловится здесь, а не в игре.</summary>
    public class ConfigReferenceTests
    {
        private static GlobalConfig Config => AssetDatabase.LoadAssetAtPath<GlobalConfig>("Assets/Configs/GameConfig.asset");

        /// <summary>У каждого состояния игры есть конфиг с префабом экрана.</summary>
        [Test]
        public void EveryGameState_HasAScreen()
        {
            var states = Config.StateConfigs;

            Assert.That(states.Select(state => state.StateName), Is.EquivalentTo(Enum.GetValues(typeof(GameStateType))));
            Assert.That(states.Where(state => state.ScreenPrefab == null).Select(state => state.name), Is.Empty);
        }

        /// <summary>В конфиге заданы префабы поля и плитки.</summary>
        [Test]
        public void TheBoard_HasItsPrefabs()
        {
            Assert.That(Config.BoardPrefab, Is.Not.Null);
            Assert.That(Config.TilePrefab, Is.Not.Null);
        }
    }
}
