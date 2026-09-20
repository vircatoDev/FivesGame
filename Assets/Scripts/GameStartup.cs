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
        private PlayerDataSaveHelper _playerDataSaveHelper;
        private GameStateMachine _stateMachine;
        private GameSession _gameSession;


        [Inject]
        public void InjectDependencies(EcsWorld world, 
            GlobalConfig config,
            SoundService soundService,
            EnergyService energyService,
            GameStateMachine stateMachine, GameSession gameSession,
            PlayerDataSaveHelper playerDataSaveHelper)
        {
            _world = world;
            _config = config;
            _soundService = soundService;
            _energyService = energyService;
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
                .OneFrame<PlayFadeAnimationEvent>()
                .OneFrame<UpdateControlPanelEnergyEvent>()
                .OneFrame<UpdateControlPanelStarsEvent>()
                .OneFrame<UpdateControlPanelBtnLogicEvent>()
                .OneFrame<ChangeStateEvent>()
                .OneFrame<SaveDataEvent>()
                .OneFrame<OpenScreenEvent>()
                .OneFrame<CloseScreenEvent>()
                .OneFrame<PlaySoundEffectEvent>()
                .OneFrame<GameStartEvent>()
                .OneFrame<GameEndEvent>();
        }

        private void AddSystems()
        {
            var gamePlaySystems = AddGamePlaySystems();
            _mainSystems.Add(gamePlaySystems);

            var idx = _mainSystems.GetNamedRunSystem("gamePlay");
            _mainSystems.SetRunSystemState(idx, false);

            _mainSystems
                .Add(new GamePlayManagementSystem(_mainSystems))
                .Add(new GameStateSystem(_stateMachine))
                .Add(new SoundSystem(_soundService))
                .Add(new StorageSystem())
                .Add(new EnergyRecoverySystem(_energyService))
                .Add(new UISystem(_rootLayer, _popUpLayer))
                .Add(new CommonUIHeaderPanelSystem(_headerPanelView))
                .Add(new FadeSystem(_fadeScreen));
        }

        private EcsSystems AddGamePlaySystems()
        {
            var gamePlaySystems = new EcsSystems(_world, "gamePlay")
                .Add(new BoardInitSystem(_gameLayer))
                .Add(new BoardDestroySystem(_gameLayer))
                .Add(new TileClickSystem())
                .Add(new TileMoveSystem())
                .Add(new ShuffleSystem())
                .Add(new WinCheckSystem());
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
            _gameSession.SetGameMode(_config.GameModes[0], false);
        }
    }
}