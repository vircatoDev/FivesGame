using Leopotam.Ecs;
using Leopotam.Ecs.Ui.Systems;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Helpers.StateMachine;
using Scripts.Models;
using Scripts.Services;
using Scripts.Systems;
using Scripts.UI.Views;
using UnityEngine;
using VContainer;

namespace Scripts
{
    public class GameStartup : MonoBehaviour
    {
        [SerializeField] EcsUiEmitter _uiEmitter;
        [SerializeField] HeaderPanelView _headerPanelView;
        [SerializeField] CanvasGroup _fadeScreen;

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


        [Inject]
        public void InjectDependencies(EcsWorld world, 
            GlobalConfig config,
            SoundService soundService,
            EnergyService energyService,
            StarService starService,
            GameStateMachine stateMachine, GameSession gameSession,
            PlayerDataSaveHelper playerDataSaveHelper)
        {
            _world = world;
            _config = config;
            _soundService = soundService;
            _energyService = energyService;
            _starService = starService;
            _stateMachine = stateMachine;
            _gameSession = gameSession;
            _playerDataSaveHelper = playerDataSaveHelper;
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
                .InjectUi(_uiEmitter);
        }

        private void AddOneFrames()
        {
            _mainSystems.OneFrame<TileClickEvent>()
                .OneFrame<BoardControlEvent>()
                .OneFrame<BoardChangedEvent>()
                .OneFrame<PlayFadeAnimationEvent>()
                .OneFrame<CurrencyChangedEvent>()
                .OneFrame<UpdateControlPanelBtnLogicEvent>()
                .OneFrame<ChangeStateEvent>()
                .OneFrame<OpenScreenEvent>()
                .OneFrame<CloseScreenEvent>()
                .OneFrame<PlaySoundEffectEvent>()
                .OneFrame<GameStartEvent>()
                .OneFrame<GameEndEvent>();
        }

        private void AddSystems()
        {
            _mainSystems
                .Add(new GamePlayManagementSystem(_mainSystems))
                .Add(AddGamePlaySystems())
                .Add(new WinCheckSystem())
                .Add(new BoardDestroySystem(_gameLayer))
                .Add(new GameStateSystem(_stateMachine))
                .Add(new SoundSystem(_soundService))
                .Add(new EnergyRecoverySystem(_energyService))
                .Add(new UISystem(_rootLayer, _popUpLayer))
                .Add(new CommonUIHeaderPanelSystem(_headerPanelView, _energyService, _starService))
                .Add(new FadeSystem(_fadeScreen))
                .Add(new StorageSystem());
        }

        private EcsSystems AddGamePlaySystems()
        {
            var gamePlaySystems = new EcsSystems(_world, "gamePlay")
                .Add(new BoardSetupSystem())
                .Add(new BoardInitSystem(_gameLayer))
                .Add(new BoardInputSystem())
                .Add(new BoardReplaySystem())
                .Add(new BoardProjectionSystem())
                .Add(new TileMoveSystem());
            return gamePlaySystems;
        }

        private void Update()
        {
            _mainSystems.Run();
        }

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
