using Cysharp.Threading.Tasks;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public class GameResultPresenter : Presenter<IGameResultView>
    {
        private readonly GameSession _gameSession;
        private readonly StarService _starService;
        private readonly PlayerProgressService _playerProgressService;
        private readonly EcsWorld _world;

        public GameResultPresenter(
            GameSession gameSession,
            StarService starService,
            PlayerProgressService playerProgressService,
            EcsWorld world)
        {
            _gameSession = gameSession;
            _starService = starService;
            _playerProgressService = playerProgressService;
            _world = world;
        }

        public override void OnActivateView()
        {
            PlayOpenPopUpAudioEffects();
            UpdateProgress();
            SavePlayerProgress();
            var theme = _gameSession.SelectedTheme;
            View.UpdateViewContent(_gameSession.LastGameResult, theme.ThemeName, _playerProgressService.GetThemeProgress(theme));
            View.PlayShowAnimation().Forget();
        }

        public void GetReward(bool doubleReward)
        {
            if (!_gameSession.TryClaimReward(doubleReward, out var rewardAmount))
            {
                return;
            }

            GiveReward(rewardAmount);
            SaveReward();
            BackToMainMenu();
        }

        private void GiveReward(int rewardAmount)
        {
            _starService.Add(rewardAmount);
            _world.Send(CurrencyChangedEvent.Changed(Currency.Stars, _starService.GetBalance(), rewardAmount));
        }

  
        private void UpdateProgress() => _playerProgressService.MarkCompleted(_gameSession.SelectedPuzzle);
        private void SavePlayerProgress() => _world.Send<SaveDataEvent>();
        private void SaveReward() => _world.Send<SaveDataEvent>();
        private void PlayOpenPopUpAudioEffects() => _world.PlaySound(AudioKeyCollection.OpenPopUp);
        private void BackToMainMenu() => _world.ChangeState(GameStateType.MainMenu);
    }
}
