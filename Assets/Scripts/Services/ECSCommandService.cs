using System;
using System.Linq;
using Leopotam.Ecs;
using Scripts.Commands;

namespace Scripts.Services
{
    public class ECSCommandService
    {
        private readonly EcsWorld _world;
    
        public ECSCommandService(EcsWorld world)
        {
            _world = world;
        }

        public ICommand CreateCommand<T>(params object[] args) where T : ICommand
        {
            return (ICommand)Activator.CreateInstance(typeof(T), new object[] { _world }.Concat(args).ToArray());
        }

        public ICommand CreateCommand<T>(Action callback, params object[] args) where T : ICommand
        {
            return (ICommand)Activator.CreateInstance(typeof(T), new object[] { _world, callback }.Concat(args).ToArray());
        }

        public ICommand CreateSimpleEventCommand<TEvent>() where TEvent : struct
        {
            return new RaiseEventCommand<TEvent>(_world);
        }
    }
}