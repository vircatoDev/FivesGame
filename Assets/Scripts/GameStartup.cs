using Cysharp.Threading.Tasks;
using Fives.Components;
using Fives.Configs;
using Fives.Models;
using Fives.Services;
using Fives.Systems;
using Fives.UI;
using Fives.UI.Navigation;
using Fives.UI.Views;
using Leopotam.Ecs;
using UnityEngine;
using VContainer;

namespace Fives
{
    public class GameStartup : MonoBehaviour
    {
        [SerializeField] private Transform gameLayer;

        private EcsWorld _world;
        private EcsSystems _mainSystems;

        private GlobalConfig _config;
        private GameBalance _balance;
        private SoundService _soundService;
        private LanguageService _languageService;
        private LoadingScreen _loadingScreen;
        private EnergyService _energyService;
        private StarService _starService;
        private PlayerSave _playerSave;
        private GameStateMachine _stateMachine;
        private GameSession _gameSession;
        private IBoardHud _boardHud;
        private PlayerProgressService _progressService;
        private IHeaderPanelView _header;

        [Inject]
        public void InjectDependencies(EcsWorld world, 
            GlobalConfig config,
            GameBalance balance,
            SoundService soundService,
            LanguageService languageService,
            LoadingScreen loadingScreen,
            EnergyService energyService,
            StarService starService,
            GameStateMachine stateMachine, GameSession gameSession,
            PlayerSave playerSave,
            IBoardHud boardHud,
            PlayerProgressService progressService,
            IHeaderPanelView header)
        {
            _world = world;
            _config = config;
            _balance = balance;
            _soundService = soundService;
            _languageService = languageService;
            _loadingScreen = loadingScreen;
            _energyService = energyService;
            _starService = starService;
            _stateMachine = stateMachine;
            _gameSession = gameSession;
            _playerSave = playerSave;
            _boardHud = boardHud;
            _progressService = progressService;
            _header = header;
        }

        private void Start()
        {
            _mainSystems = new EcsSystems(_world);

            AddSystems();
            AddOneFrames();
            AddShareData();  // Share data that is used by more than 1 system, the rest is through the constructor

            SetDefaultGameSettings();
            SetDefaultState();

            _mainSystems.Init();
            _loadingScreen.Hide().Forget(); // shown by the Boot scene until the game is running
        }

        private void AddShareData()
        {
            _mainSystems.Inject(_gameSession)
                .Inject(_config)
                .Inject(_balance)
                .Inject(_playerSave)
                .Inject(new UnityFrameTime());
        }

        // Every event lives one frame and is removed here, after all systems. Events sent outside Run
        // (UI callbacks, async continuations) survive until the next Run, so each consumer must run
        // after the systems that send its events within a frame.
        private void AddOneFrames()
        {
            _mainSystems.OneFrame<TileClickEvent>()
                .OneFrame<TileSwipeEvent>()
                .OneFrame<BoardControlEvent>()
                .OneFrame<BoardChangedEvent>()
                .OneFrame<CurrencyChangedEvent>()
                .OneFrame<ChangeStateEvent>()
                .OneFrame<PlaySoundEffectEvent>()
                .OneFrame<SaveDataEvent>()
                .OneFrame<BoardInitializedEvent>()
                .OneFrame<BoardSolvedEvent>()
                .OneFrame<GameEndEvent>();
        }

        private void AddSystems()
        {
            _mainSystems
                .Add(new GamePlayManagementSystem(_mainSystems))
                .Add(AddGamePlaySystems())
                .Add(new BoardRevealSystem())
                .Add(new BoardDestroySystem())
                .Add(new GameStateSystem(_stateMachine))
                .Add(new EnergyRecoverySystem(_energyService))
                .Add(new SoundSystem(_soundService))
                .Add(new CurrencySyncSystem(_starService, _energyService)) // after every spender, before the header and the save
                .Add(new CommonUIHeaderPanelSystem(_header, _energyService, _starService))
                .Add(new StorageSystem(Storables));
        }

        private EcsSystems AddGamePlaySystems()
        {
            var gamePlaySystems = new EcsSystems(_world, "gamePlay")
                .Add(new BoardSetupSystem())
                .Add(new BoardInitSystem(gameLayer))
                .Add(new BoardInputSystem())
                .Add(new BoardHintSystem(_starService))
                .Add(new BoardHintViewSystem())
                .Add(new BoardProjectionSystem())
                .Add(new TileHighlightSystem())
                .Add(new TileMoveSystem())
                .Add(new WinCheckSystem())
                .Add(new PuzzleCompletionSystem(_progressService))
                .Add(new BoardHudSystem(_boardHud));
            return gamePlaySystems;
        }

        private void Update()
        {
            _mainSystems.Run();
        }

        // Mobile OSes may kill a paused app without further callbacks, so unsaved settings and progress are written here.
        private void OnApplicationPause(bool paused)
        {
            if (paused)
                SaveAll();
        }

        private void OnApplicationQuit() => SaveAll();

        // Leaving while the scene is still loading ends before Start, even before injection: nothing ran, nothing to save.
        private bool Started => _mainSystems != null;

        private void SaveAll()
        {
            if (Started)
                _playerSave.SaveAll(Storables);
        }

        private IStorable[] Storables => new IStorable[] { _soundService, _languageService, _energyService, _starService, _progressService };

        private void OnDestroy()
        {
            if (!Started)
                return;

            _mainSystems.Destroy();
            _world.Destroy();
        }

        private void SetDefaultState()
        {
            var stateEntity = _world.NewEntity();
            stateEntity.Replace(new GameStateComponent
            {
                CurrentState = GameStateType.MainMenu
            });
        }

        private void SetDefaultGameSettings()
        {
            _gameSession.SetGameMode(_config.GameModes[0]);
        }
    }
}
