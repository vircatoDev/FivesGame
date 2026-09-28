using Cysharp.Threading.Tasks;
using Fives.Components;
using Fives.Models;
using Fives.Services;
using Fives.UI.Views;
using Leopotam.Ecs;

namespace Fives.UI.Presenters
{
    public class GamePlayPresenter : Presenter<IGamePlayView>, IBoardHud
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

        public void Show(in BoardHud hud)
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
