using System.Collections.Generic;
using System.Linq;
using Leopotam.Ecs;
using Scripts.Commands;
using Scripts.Configs;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;
using UnityEngine;

namespace Scripts.UI.Presenters
{
    public class MainMenuPresenter : BasePresenter
    {
        private readonly EcsWorld _world;
        private readonly GlobalConfig _themeConfig;
        private readonly PlayerProgressService _playerProgressService;
        private readonly EnergyService _energyService;
        private readonly GameSession _gameSession;
        private readonly ECSCommandService _ecsCommandService;
        private MainMenuView _view;

        public MainMenuPresenter(EcsWorld world, GlobalConfig themeConfig, PlayerProgressService playerProgressService,
            EnergyService energyService, GameSession gameSession, ECSCommandService ecsCommandService)
        {
            _world = world;
            _themeConfig = themeConfig;
            _playerProgressService = playerProgressService;
            _energyService = energyService;
            _gameSession = gameSession;
            _ecsCommandService = ecsCommandService;
        }

        public override void Initialize(BaseView initData)
        {
            _view = initData as MainMenuView;
            OnActivateView();
        }

        public override void OnActivateView()
        {
            var theme = GetLastActiveTheme();

            _view.PlayShowAnimation();
            _view.UpdateViewContent(GetThemeProgress(theme), theme.ThemeName, theme.ThemeLogo);

            _ecsCommandService.CreateCommand<UpdateHeaderBtnLogicCommand>(OnSettings, HeaderBtnType.Settings).Execute();
        }

        public async void OnStartGame()
        {
            _ecsCommandService.CreateCommand<PlaySoundEffectCommand>(AudioKeyCollection.MenuClick, 1f).Execute();

            if (!TrySpendEnergy())
                return;

            var theme = GetLastActiveTheme();
            var nextPuzzle = FindNextUncompletedPuzzle(theme);
            _gameSession.SetSelectedTheme(theme);
            _gameSession.SetSelectedImage(nextPuzzle);
            
            await _view.PlayHideAnimation();
            
            _ecsCommandService.CreateCommand<ChangeGameStateCommand>(GameStateType.Playing).Execute();
        }

        public void OnSettings()
        {
            _ecsCommandService.CreateCommand<PlaySoundEffectCommand>(AudioKeyCollection.MenuClick, 1f).Execute();
            _ecsCommandService.CreateCommand<ChangeGameStateCommand>(GameStateType.Settings).Execute();
        }

        public async void OnSelectMenu()
        {
            Debug.Log("MainMenuPresenter: Start Game clicked");
            _ecsCommandService.CreateCommand<PlaySoundEffectCommand>(AudioKeyCollection.MenuClick, 1f).Execute();
            await _view.PlayHideAnimation();
            _ecsCommandService.CreateCommand<ChangeGameStateCommand>(GameStateType.SelectMenu).Execute();
        }

        private bool TrySpendEnergy()
        {
            if (_energyService.GetBalance() <= 0)
            {
                _ecsCommandService.CreateCommand<PlaySoundEffectCommand>(AudioKeyCollection.WrongClick, 1f).Execute();
                _ecsCommandService.CreateCommand<HeaderNoEnergyAnimationCommand>().Execute();

                return false;
            }
            else
            {
                _energyService.Spend(1);
                _ecsCommandService.CreateCommand<UpdateEnergyBalanceCommand>(_energyService, -1).Execute();
                _ecsCommandService.CreateCommand<SaveDataCommand>(_energyService).Execute();
                return true;
            }
        }

        private List<string> GetCompletedPuzzleForThemeIntersect(ThemeConfig theme)
        {
            var completedPuzzles = _playerProgressService.GetProgressData().CompletedPuzzles;
            return completedPuzzles.Intersect(theme.Puzzles.Select(p => p.Name)).ToList();
        }

        private ThemeConfig GetLastActiveTheme()
        {
            var lastActiveTheme = _playerProgressService.GetProgressData().UnlockedThemes.Last();
            var theme = _themeConfig.Themes.FirstOrDefault(x => x.ThemeName == lastActiveTheme);
            return theme;
        }

        private PuzzleData FindNextUncompletedPuzzle(ThemeConfig lastUnlockedTheme)
        {
            var completedPuzzles = _playerProgressService.GetProgressData().CompletedPuzzles;

            foreach (var puzzle in lastUnlockedTheme.Puzzles)
            {
                if (!completedPuzzles.Contains(puzzle.Name))
                {
                    return puzzle;
                }
            }

            return lastUnlockedTheme.Puzzles.Last();
        }

        private string GetThemeProgress(ThemeConfig theme) =>
            GetCompletedPuzzleForThemeIntersect(theme).Count() + "/" + theme.Puzzles.Length;
    }
}