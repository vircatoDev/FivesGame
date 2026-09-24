using System;
using System.Linq;
using Fives.Domain;
using Leopotam.Ecs;
using NUnit.Framework;
using Scripts.Components;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services;
using Scripts.Systems;
using Scripts.UI.Presenters;
using UnityEngine;

namespace Fives.Runtime.Tests
{
    public sealed class SaveTests
    {
        // Tests write under their own prefix so the player's editor save is never touched.
        private const string Prefix = "Fives.Tests.";
        private const string SaveKey = Prefix + "GameSaveData";
        private TestObjects _objects;
        private StorageService _storage;

        [SetUp]
        public void SetUp()
        {
            _objects = new TestObjects();
            _storage = new StorageService(Prefix);
            DeleteTestKeys();
        }

        [TearDown]
        public void TearDown()
        {
            DeleteTestKeys();
            _objects.Dispose();
        }

        private static void DeleteTestKeys()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.DeleteKey(SaveKey + ".corrupt");
        }

        private Scripts.Configs.GlobalConfig ConfigWithCorgi()
        {
            var config = _objects.Config();
            config.Themes[1].Puzzles = new[] { new PuzzleData { Id = "dogs.corgi", Name = "Corgi" } };
            return config;
        }

        [Test]
        public void LateSaveRequest_SurvivesUntilTheNextFrame()
        {
            var store = new MemoryStorage();
            var save = new PlayerDataSaveHelper(store, _objects.Config());
            var world = new EcsWorld();
            var systems = new EcsSystems(world)
                .Add(new StorageSystem()).OneFrame<SaveDataEvent>()
                .Add(new SaveEveryFrame { Target = new StarService(save) })
                .Inject(save);
            systems.Init();
            systems.Tick(2);

            Assert.That(store.Saves, Is.EqualTo(1), "a request made after StorageSystem is written on the next frame");
            systems.Destroy();
            world.Destroy();
        }

        [Test]
        public void Version0Save_MigratesNamesToIds_Once()
        {
            var config = ConfigWithCorgi();
            PlayerPrefs.SetString(SaveKey,
                "{\"Stars\":7,\"PlayerProgress\":{\"UnlockedThemes\":[\"Dogs\",\"Flowers\"],\"CompletedPuzzles\":[\"Corgi\"]}}");

            var migrated = new PlayerDataSaveHelper(_storage, config).GetPlayerData();
            Assert.That(migrated.Version, Is.EqualTo(ProgressMigration.CurrentVersion));
            Assert.That(migrated.Stars, Is.EqualTo(7));
            Assert.That(migrated.PlayerProgress.UnlockedThemes, Is.EqualTo(new[] { "dogs", "Flowers" }));
            Assert.That(migrated.PlayerProgress.CompletedPuzzles, Is.EqualTo(new[] { "dogs.corgi" }));

            _storage.Save("GameSaveData", migrated);
            var reloaded = new PlayerDataSaveHelper(_storage, config).GetPlayerData().PlayerProgress;
            Assert.That(reloaded.CompletedPuzzles, Is.EqualTo(new[] { "dogs.corgi" }));
            Assert.That(reloaded.UnlockedThemes, Is.EqualTo(new[] { "dogs", "Flowers" }));
        }

        [Test]
        public void RenamingContent_KeepsProgress()
        {
            var config = ConfigWithCorgi();
            PlayerPrefs.SetString(SaveKey, "{\"PlayerProgress\":{\"UnlockedThemes\":[\"Dogs\"],\"CompletedPuzzles\":[\"Corgi\"]}}");
            var progress = new PlayerProgressService(new PlayerDataSaveHelper(_storage, config));
            var theme = config.Themes[1];
            theme.ThemeName = "Puppies";
            theme.Puzzles[0].Name = "Welsh Corgi";

            Assert.That(progress.IsUnlocked(theme), Is.True);
            Assert.That(progress.IsCompleted(theme.Puzzles[0]), Is.True);
        }

        [TestCase("{")]
        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("{\"Stars\":\"invalid\"}")]
        public void InvalidSave_FallsBackToDefaults_AndKeepsTheRawData(string invalid)
        {
            PlayerPrefs.SetString(SaveKey, invalid);

            var save = new PlayerDataSaveHelper(_storage, _objects.Config());

            Assert.That(save.GetPlayerData().Stars, Is.EqualTo(200));
            Assert.That(PlayerPrefs.GetString(SaveKey + ".corrupt"), Is.EqualTo(invalid));
        }

        [Test]
        public void PartialSave_KeepsProgress_RestoresMissingData_AndRoundtrips()
        {
            PlayerPrefs.SetString(SaveKey + ".corrupt", "{");
            PlayerPrefs.SetString(SaveKey, "{\"Stars\":41,\"PlayerProgress\":{\"CompletedPuzzles\":[\"One\"]}}");

            var partial = new PlayerDataSaveHelper(_storage, _objects.Config()).GetPlayerData();
            Assert.That(partial.Stars, Is.EqualTo(41));
            Assert.That(partial.PlayerProgress.CompletedPuzzles, Does.Contain("One"));
            Assert.That(partial.PlayerProgress.UnlockedThemes, Does.Contain("dogs"));
            Assert.That(partial.Energy.CurrentEnergy, Is.EqualTo(5));
            Assert.That(partial.SoundSettings, Is.Not.Null);

            _storage.Save("GameSaveData", partial);
            var restored = _storage.Load<GameSaveData>("GameSaveData");
            Assert.That(restored.Stars, Is.EqualTo(41));
            Assert.That(restored.PlayerProgress.CompletedPuzzles, Does.Contain("One"));
            Assert.That(PlayerPrefs.HasKey(SaveKey + ".corrupt"), Is.True, "a successful save keeps the corrupt backup");
        }

        [Test]
        public void PartialSaveWithARemovedTheme_StillOpensTheMainMenu()
        {
            PlayerPrefs.SetString(SaveKey, "{\"Energy\":{},\"PlayerProgress\":{\"UnlockedThemes\":[\"RemovedTheme\",null]}}");
            var config = ConfigWithCorgi();
            var save = new PlayerDataSaveHelper(_storage, config);
            var world = new EcsWorld();
            var session = new GameSession(config);
            var energy = new EnergyService(config, save, new FakeClock { UtcNow = DateTime.UtcNow });
            var view = new FakeMainMenuView();

            new MainMenuPresenter(config, new PlayerProgressService(save), new GameStartService(session, energy, world), session, world)
                .Initialize(view);

            Assert.That(save.GetPlayerData().Energy.LastRecoveryTime, Is.Not.EqualTo(default(DateTime)));
            Assert.That(save.GetPlayerData().PlayerProgress.CompletedPuzzles, Is.Not.Null);
            Assert.That(view.Current.Title, Is.EqualTo("Dogs"), "an unknown theme id falls back to a known theme");
            world.Destroy();
        }
    }
}
