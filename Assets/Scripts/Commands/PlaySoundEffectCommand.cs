using Leopotam.Ecs;
using Scripts.Components;

namespace Scripts.Commands
{
    public class PlaySoundEffectCommand : ICommand
    {
        private readonly EcsWorld _world;
        private readonly string _key;
        private readonly float _volume;


        public PlaySoundEffectCommand(EcsWorld world ,string key,float volume)
        {
            _world = world;
            _volume = volume;
            _key = key;
        }

        public void Execute()
        {
            var updateControlPanelEvent = _world.NewEntity();
            updateControlPanelEvent.Replace(new PlaySoundEffectEvent()
            {
                Key = _key,
                Volume = _volume
            });
        }
    }
}