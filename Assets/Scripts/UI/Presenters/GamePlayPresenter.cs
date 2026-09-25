using Cysharp.Threading.Tasks;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using Scripts.Services;
using Scripts.UI.Views;

namespace Scripts.UI.Presenters
{
    public class GamePlayPresenter : Presenter<IGamePlayView>
    {
        private readonly GameSession _gameSession;
        private readonly IHeaderPanelView _header;
        private readonly EcsWorld _world;
        private readonly ITexts _texts;

        public GamePlayPresenter(GameSession gameSession, IHeaderPanelView header, EcsWorld world, ITexts texts)
        {
            _texts = texts;
            _header = header;
            _world = world;
            _gameSession = gameSession;
        }

        public override void OnActivateView()
        {
            var puzzle = _gameSession.SelectedPuzzle;
            View.UpdateViewContent(_gameSession.PuzzleImage, _texts.Get(TextKeys.Name(puzzle)), _texts.Get(TextKeys.About(puzzle)));
            View.PlayShowAnimation().Forget();

            _header.ShowButton(HeaderBtnType.Back, BackToMainMenu);
        }

        public void RequestControl(BoardControl control) =>
            _world.Send(new BoardControlEvent { Control = control });

        public void ShowHud(in BoardHud hud)
        {
            View.UpdateControls(_texts.Get(TextKeys.Moves, hud.Moves), hud.HintPrice.ToString(), hud.CanUndo, hud.CanHint);
        }

        private void BackToMainMenu()
        {
            _world.PlaySound(AudioKeyCollection.MenuClick);
            _world.Send<GameEndEvent>();
            _world.ChangeState(GameStateType.MainMenu);
        }
    }
}
