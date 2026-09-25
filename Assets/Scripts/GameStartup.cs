using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Helpers.Factory;
using Scripts.Helpers.StateMachine;
using Scripts.Models;
using Scripts.Services;
using Scripts.Services.Interfaces;
using Scripts.Systems;
using Scripts.UI.Presenters;
using Scripts.UI.Views;
using UnityEngine;
using VContainer;

namespace Scripts
{
    public class GameStartup : MonoBehaviour
    {

        [SerializeField] Transform _rootLayer;
        [SerializeField] Transform _popUpLayer;
        [SerializeField] Transform _gameLayer;

        private EcsWorld _world;
        private EcsSystems _mainSystems;

        private GlobalConfig _config;
        private SoundService _soundService;
        private EnergyService _energyService;
        private StarService _starService;
        private PlayerDataSaveHelper _playerDataSaveHelper;
        private GameStateMachine _stateMachine;
        private GameSession _gameSession;
        private GamePlayPresenter _gamePlayPresenter;
        private PlayerProgressService _progressService;
        private IHeaderPanelView _header;
        private ScreenCatalog _screens;


        [Inject]
        public void InjectDependencies(EcsWorld world, 
            GlobalConfig config,
            SoundService soundService,
            EnergyService energyService,
            StarService starService,
            GameStateMachine stateMachine, GameSession gameSession,
            PlayerDataSaveHelper playerDataSaveHelper,
            GamePlayPresenter gamePlayPresenter,
            PlayerProgressService progressService,
            IHeaderPanelView header,
            ScreenCatalog screens)
        {
            _world = world;
            _config = config;
            _soundService = soundService;
            _energyService = energyService;
            _starService = starService;
            _stateMachine = stateMachine;
            _gameSession = gameSession;
            _playerDataSaveHelper = playerDataSaveHelper;
            _gamePlayPresenter = gamePlayPresenter;
            _progressService = progressService;
            _header = header;
            _screens = screens;
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
        }

        private void AddShareData()
        {
            _mainSystems.Inject(_gameSession)
                .Inject(_config)
                .Inject(_playerDataSaveHelper)
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
                .OneFrame<OpenScreenEvent>()
                .OneFrame<CloseScreenEvent>()
                .OneFrame<CloseAllScreensEvent>()
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
                .Add(new WinCheckSystem())
                .Add(new BoardRevealSystem())
                .Add(new BoardDestroySystem(_gameLayer))
                .Add(new GameStateSystem(_stateMachine))
                .Add(new EnergyRecoverySystem(_energyService))
                .Add(new UISystem(_rootLayer, _popUpLayer, _screens))
                .Add(new SoundSystem(_soundService))
                .Add(new CommonUIHeaderPanelSystem(_header, _energyService, _starService))
                .Add(new StorageSystem(Storables));
        }

        private EcsSystems AddGamePlaySystems()
        {
            var gamePlaySystems = new EcsSystems(_world, "gamePlay")
                .Add(new BoardSetupSystem())
                .Add(new BoardInitSystem(_gameLayer))
                .Add(new BoardInputSystem())
                .Add(new BoardProjectionSystem())
                .Add(new TileHighlightSystem())
                .Add(new TileMoveSystem())
                .Add(new BoardHudSystem(_gamePlayPresenter));
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

        private void SaveAll() => _playerDataSaveHelper.SaveAll(Storables);

        private IStorable[] Storables => new IStorable[] { _soundService, _energyService, _starService, _progressService };

        private void OnDestroy()
        {
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
