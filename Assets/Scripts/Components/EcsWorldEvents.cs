using Leopotam.Ecs;
using Scripts.Models;

namespace Scripts.Components
{
    /// <summary>The single way to raise a one-frame ECS event: a new entity carrying one event component.</summary>
    public static class EcsWorldEvents
    {
        public static void Send<TEvent>(this EcsWorld world, in TEvent evt) where TEvent : struct =>
            world.NewEntity().Replace(evt);

        public static void Send<TEvent>(this EcsWorld world) where TEvent : struct =>
            world.NewEntity().Get<TEvent>();

        public static void PlaySound(this EcsWorld world, string key, float volume = 1f) =>
            world.Send(new PlaySoundEffectEvent { Key = key, Volume = volume });

        public static void ChangeState(this EcsWorld world, GameStateType state) =>
            world.Send(new ChangeStateEvent { NewStateName = state });
    }
}
