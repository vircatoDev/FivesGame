using System;
using Fives.Components;
using Fives.Configs;
using Fives.Domain;
using Fives.Models;
using Fives.Services;
using Fives.Systems;
using Leopotam.Ecs;
using NUnit.Framework;
using UnityEngine;

namespace Fives.Runtime.Tests.Saving
{
    /// <summary>Просит сохранение в каждом кадре, уже после StorageSystem.</summary>
    internal sealed class SaveEveryFrame : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        public void Run() => _world.Send<SaveDataEvent>();
    }

    /// <summary>
    /// Сохранение игрока в PlayerPrefs: одна запись за кадр, старые и битые сохранения, переименованный
    /// и удалённый контент.
    /// </summary>
    public sealed class SaveTests
    {
        // Тесты пишут под своим префиксом, чтобы не задеть сохранение игрока в редакторе.
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

        private GlobalConfig ConfigWithCorgi()
        {
            var config = _objects.Config();
            config.Themes[1].Puzzles = new[] { new PuzzleData { Id = "dogs.corgi", Name = "Corgi" } };
            return config;
        }

        private PlayerSave Load(GlobalConfig config)
        {
            var save = new PlayerSave(_storage, config, new GameBalance(config));
            save.Load();
            return save;
        }

        /// <summary>
        /// Запрос сохранения, пришедший после StorageSystem, не теряется, а пишется в следующем кадре.
        /// </summary>
        [Test]
        public void LateSaveRequest_SurvivesUntilTheNextFrame()
        {
            var storage = new MemoryStorage();
            using var game = new GameStand(storage: storage);
            var systems = new EcsSystems(game.World)
                .Add(new StorageSystem(game.Stars)).OneFrame<SaveDataEvent>()
                .Add(new SaveEveryFrame())
                .Inject(game.Save);
            systems.Init();
            systems.Tick(2);

            Assert.That(storage.Saves, Is.EqualTo(1), "a request made after StorageSystem is written on the next frame");
            systems.Destroy();
        }

        /// <summary>Два запроса за кадр дают одну запись, и в ней текущие данные всех сервисов.</summary>
        [Test]
        public void SeveralSaveRequestsInOneFrame_WriteOnce()
        {
            var storage = new MemoryStorage();
            using var game = new GameStand(storage: storage);
            var systems = new EcsSystems(game.World).Add(new StorageSystem(game.Stars)).OneFrame<SaveDataEvent>().Inject(game.Save);
            systems.Init();
            game.Stars.Add(5);
            game.World.Send<SaveDataEvent>();
            game.World.Send<SaveDataEvent>();
            systems.Run();

            Assert.That(storage.Saves, Is.EqualTo(1));
            Assert.That(storage.Data.Stars, Is.EqualTo(TestConfig.Stars + 5), "the save holds every service's current data");
            systems.Destroy();
        }

        /// <summary>
        /// Сохранение версии 0 с именами переводится на id;
        /// после записи и повторной загрузки id остаются.
        /// </summary>
        [Test]
        public void Version0Save_MigratesNamesToIds_Once()
        {
            var config = ConfigWithCorgi();
            PlayerPrefs.SetString(SaveKey,
                "{\"Stars\":7,\"PlayerProgress\":{\"UnlockedThemes\":[\"Dogs\",\"Flowers\"],\"CompletedPuzzles\":[\"Corgi\"]}}");

            var migrated = Load(config).GetPlayerData();
            Assert.That(migrated.Version, Is.EqualTo(ProgressMigration.CurrentVersion));
            Assert.That(migrated.Stars, Is.EqualTo(7));
            Assert.That(migrated.PlayerProgress.UnlockedThemes, Is.EqualTo(new[] { "dogs", "Flowers" }));
            Assert.That(migrated.PlayerProgress.CompletedPuzzles, Is.EqualTo(new[] { "dogs.corgi" }));

            _storage.Save("GameSaveData", migrated);
            var reloaded = Load(config).GetPlayerData().PlayerProgress;
            Assert.That(reloaded.CompletedPuzzles, Is.EqualTo(new[] { "dogs.corgi" }));
            Assert.That(reloaded.UnlockedThemes, Is.EqualTo(new[] { "dogs", "Flowers" }));
        }

        /// <summary>Переименование темы и пазла не теряет прогресс: он хранится по id.</summary>
        [Test]
        public void RenamingContent_KeepsProgress()
        {
            var config = ConfigWithCorgi();
            PlayerPrefs.SetString(SaveKey, "{\"PlayerProgress\":{\"UnlockedThemes\":[\"Dogs\"],\"CompletedPuzzles\":[\"Corgi\"]}}");
            var progress = new PlayerProgressService(Load(config));
            var theme = config.Themes[1];
            theme.ThemeName = "Puppies";
            theme.Puzzles[0].Name = "Welsh Corgi";

            Assert.That(progress.IsUnlocked(theme), Is.True);
            Assert.That(progress.IsCompleted(theme.Puzzles[0]), Is.True);
        }

        /// <summary>
        /// Нечитаемое сохранение заменяется значениями по умолчанию, а исходный текст копируется в
        /// GameSaveData.corrupt.
        /// </summary>
        [TestCase("{")]                        // JSON оборван
        [TestCase("null")]                     // null вместо объекта
        [TestCase("[]")]                       // массив вместо объекта
        [TestCase("{\"Stars\":\"invalid\"}")]  // строка вместо числа
        public void InvalidSave_FallsBackToDefaults_AndKeepsTheRawData(string invalid)
        {
            PlayerPrefs.SetString(SaveKey, invalid);

            var save = Load(_objects.Config());

            Assert.That(save.GetPlayerData().Stars, Is.EqualTo(TestConfig.Stars));
            Assert.That(PlayerPrefs.GetString(SaveKey + ".corrupt"), Is.EqualTo(invalid));
        }

        /// <summary>
        /// Неполное сохранение:
        /// имеющиеся данные сохраняются, недостающие заполняются по умолчанию;
        /// после записи и чтения данные те же, а старая копия .corrupt не стирается.
        /// </summary>
        [Test]
        public void PartialSave_KeepsProgress_RestoresMissingData_AndRoundtrips()
        {
            PlayerPrefs.SetString(SaveKey + ".corrupt", "{");
            PlayerPrefs.SetString(SaveKey, "{\"Stars\":41,\"PlayerProgress\":{\"CompletedPuzzles\":[\"One\"]}}");

            var partial = Load(_objects.Config()).GetPlayerData();
            Assert.That(partial.Stars, Is.EqualTo(41));
            Assert.That(partial.PlayerProgress.CompletedPuzzles, Does.Contain("One"));
            Assert.That(partial.PlayerProgress.UnlockedThemes, Does.Contain("dogs"));
            Assert.That(partial.Energy.CurrentEnergy, Is.EqualTo(TestConfig.Energy));
            Assert.That(partial.SoundSettings, Is.Not.Null);

            _storage.Save("GameSaveData", partial);
            var restored = _storage.Load<GameSaveData>("GameSaveData");
            Assert.That(restored.Stars, Is.EqualTo(41));
            Assert.That(restored.PlayerProgress.CompletedPuzzles, Does.Contain("One"));
            Assert.That(PlayerPrefs.HasKey(SaveKey + ".corrupt"), Is.True, "a successful save keeps the corrupt backup");
        }

        /// <summary>
        /// В сохранении открыта удалённая тема, а часть полей пуста:
        /// главное меню всё равно открывается на известной теме.
        /// </summary>
        [Test]
        public void PartialSaveWithARemovedTheme_StillOpensTheMainMenu()
        {
            PlayerPrefs.SetString(SaveKey, "{\"Energy\":{},\"PlayerProgress\":{\"UnlockedThemes\":[\"RemovedTheme\",null]}}");
            using var game = new GameStand(storage: _storage);
            game.Theme("dogs").Puzzles = new[] { new PuzzleData { Id = "dogs.corgi", Name = "Corgi" } };
            var view = new FakeMainMenuView();

            game.MainMenu(view);

            Assert.That(game.Save.GetPlayerData().Energy.LastRecoveryTime, Is.Not.EqualTo(default(DateTime)));
            Assert.That(game.Save.GetPlayerData().PlayerProgress.CompletedPuzzles, Is.Not.Null);
            Assert.That(view.Current.TitleKey, Is.EqualTo("theme.dogs"), "an unknown theme id falls back to a known theme");
        }

        /// <summary>
        /// Сохранение нельзя прочитать до загрузки: то, что читает его раньше времени, сразу падает, а не получает
        /// новое сохранение без значений Remote Config.
        /// </summary>
        [Test]
        public void ReadingTheSave_BeforeItIsLoaded_Throws()
        {
            var config = _objects.Config();
            var save = new PlayerSave(_storage, config, new GameBalance(config));

            Assert.Throws<InvalidOperationException>(() => save.GetPlayerData());
        }
    }
}
