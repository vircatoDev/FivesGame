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
        private readonly IRewardedAds _ads;

        public GameResultPresenter(
            GameSession gameSession,
            StarService starService,
            PlayerProgressService playerProgressService,
            EcsWorld world,
            ITexts texts,
            IRewardedAds ads)
        {
            _gameSession = gameSession;
            _starService = starService;
            _playerProgressService = playerProgressService;
            _world = world;
            _texts = texts;
            _ads = ads;
        }

        // The puzzle is already recorded as completed by PuzzleCompletionSystem: the screen only shows the result.
        public override void OnActivateView()
        {
            PlayOpenPopUpAudioEffects();
            var theme = _gameSession.SelectedTheme;
            var result = _gameSession.LastGameResult;
            View.UpdateViewContent(_texts.Get(TextKeys.RewardStars, result.StarCount),
                _texts.Get(TextKeys.ResultStats, result.TurnCount, result.GameTime.ToString(@"mm\:ss")),
                _texts.Get(TextKeys.Name(theme)), _playerProgressService.GetThemeProgress(theme));
            ShowDoubleReward();
            _ads.ReadyChanged += ShowDoubleReward;
            View.PlayShowAnimation().Forget();
        }

        protected override void OnDeactivateView() => _ads.ReadyChanged -= ShowDoubleReward;

        public void GetReward() => Claim(false);

        public void GetDoubleReward() => GetDoubleRewardAsync().Forget();

        // The stars double only after the ad was watched to its reward. A skipped or failed ad grants nothing, and the
        // plain reward stays available.
        private async UniTaskVoid GetDoubleRewardAsync()
        {
            if (await _ads.Show(View.Lifetime))
                Claim(true);
        }

        private void Claim(bool doubleReward)
        {
            if (!_gameSession.TryClaimReward(doubleReward, out var rewardAmount))
                return;

            _starService.Add(rewardAmount);
            BackToMainMenu();
        }

        private void ShowDoubleReward() => View.ShowDoubleReward(_ads.IsSupported, _ads.IsReady);

        private void PlayOpenPopUpAudioEffects() => _world.PlaySound(AudioKeyCollection.OpenPopUp);
        private void BackToMainMenu() => _world.ChangeState(GameStateType.MainMenu);
    }
}
