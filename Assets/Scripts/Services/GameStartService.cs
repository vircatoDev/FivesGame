using Scripts.Commands;
using Scripts.Configs;
using Scripts.Models;

namespace Scripts.Services
{
    public class GameStartService
    {
        private readonly GameSession _session;
        private readonly EnergyService _energy;
        private readonly ECSCommandService _commands;

        public GameStartService(GameSession session, EnergyService energy, ECSCommandService commands)
        {
            _session = session;
            _energy = energy;
            _commands = commands;
        }

        public bool TryStart(ThemeConfig theme, PuzzleData puzzle)
        {
            if (_session.IsRunning || theme == null || puzzle == null)
            {
                return false;
            }

            if (!_energy.Spend(1))
            {
                _commands.CreateCommand<PlaySoundEffectCommand>(AudioKeyCollection.WrongClick, 1f).Execute();
                _commands.CreateCommand<HeaderNoEnergyAnimationCommand>().Execute();
                return false;
            }

            _session.SetSelectedTheme(theme);
            _session.SetSelectedImage(puzzle);
            _session.BeginRun();
            _commands.CreateCommand<UpdateEnergyBalanceCommand>(_energy, -1).Execute();
            _commands.CreateCommand<SaveDataCommand>(_energy).Execute();
            return true;
        }
    }
}
