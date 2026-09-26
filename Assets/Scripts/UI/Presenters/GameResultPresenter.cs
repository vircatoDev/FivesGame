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
        private readonly ITexts _texts;

        public GameResultPresenter(
            GameSession gameSession,
            StarService starService,
            PlayerProgressService playerProgressService,
            EcsWorld world,
            ITexts texts)
        {
            _gameSession = gameSession;
            _starService = starService;
            _playerProgressService = playerProgressService;
            _world = world;
            _texts = texts;
        }

        public override void OnActivateView()
        {
            PlayOpenPopUpAudioEffects();
            UpdateProgress();
            SavePlayerProgress();
            var theme = _gameSession.SelectedTheme;
            var result = _gameSession.LastGameResult;
            View.UpdateViewContent(_texts.Get(TextKeys.RewardStars, result.StarCount),
                _texts.Get(TextKeys.ResultStats, result.TurnCount, result.GameTime.ToString(@"mm\:ss")),
                _texts.Get(TextKeys.Name(theme)), _playerProgressService.GetThemeProgress(theme));
            View.PlayShowAnimation().Forget();
        }

        public void GetReward(bool doubleReward)
        {
            if (!_gameSession.TryClaimReward(doubleReward, out var rewardAmount))
            {
                return;
            }

            _starService.Add(rewardAmount);
            BackToMainMenu();
        }

  
        private void UpdateProgress() => _playerProgressService.MarkCompleted(_gameSession.SelectedPuzzle);
        private void SavePlayerProgress() => _world.Send<SaveDataEvent>();
        private void PlayOpenPopUpAudioEffects() => _world.PlaySound(AudioKeyCollection.OpenPopUp);
        private void BackToMainMenu() => _world.ChangeState(GameStateType.MainMenu);
    }
}
