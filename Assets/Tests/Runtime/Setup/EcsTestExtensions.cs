using Leopotam.Ecs;

namespace Fives.Runtime.Tests
{
    internal static class EcsTestExtensions
    {
        /// <summary>How many entities have the component, events included.</summary>
        public static int Count<T>(this EcsWorld world) where T : struct =>
            world.GetFilter(typeof(EcsFilter<T>)).GetEntitiesCount();

        public static T[] All<T>(this EcsWorld world) where T : struct
        {
            var filter = (EcsFilter<T>)world.GetFilter(typeof(EcsFilter<T>));
            var items = new T[filter.GetEntitiesCount()];
            var n = 0;
            foreach (var i in filter)
                items[n++] = filter.Get1(i);
            return items;
        }

        /// <summary>Runs the given number of frames.</summary>
        public static void Tick(this EcsSystems systems, int frames = 1)
        {
            for (var i = 0; i < frames; i++)
                systems.Run();
        }
    }
}
