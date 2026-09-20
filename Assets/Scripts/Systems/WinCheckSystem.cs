using System;
using Cysharp.Threading.Tasks;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using UnityEngine;

namespace Scripts.Systems
{
    class WinCheckSystem : IEcsRunSystem, IEcsInitSystem
    {
        private readonly EcsFilter<TileComponent> _tileFilter = null;
        private readonly EcsFilter<GameStateComponent> _stateFilter = null;
        private readonly EcsWorld _world;

        private bool _isWin;


        public void Init()
        {
   
        }

        public void Run()
        {
            if (_stateFilter.Get1(0).CurrentState != GameStateType.Playing)
            {
                _isWin = false;
                return;
            }

            if (_isWin)
                return;

            _isWin = true;

            foreach (var i in _tileFilter)
            {
                ref var tile = ref _tileFilter.Get1(i);

                int expectedId = (int)(tile.Position.y + (tile.Position.y * 2) + tile.Position.x);

                if (tile.Id != expectedId)
                {
                    _isWin = false;
                    break;
                }
            }

            if (_isWin)
            {
                Debug.Log("Game completed!");
            
                var soundEntity = _world.NewEntity();
                soundEntity.Replace(new PlaySoundEffectEvent()
                {
                    Key = AudioKeyCollection.Win,
                    Volume = 1f
                });
            
                SendWinGameEvents();
            }
        }

        private async void SendWinGameEvents()
        {
            await SendWinEvents();
        }

        private async UniTask SendWinEvents()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f));

            var gameEndEvent = _world.NewEntity();
            gameEndEvent.Replace(new GameEndEvent()
            {
                Delay = 3f
            });

            await UniTask.Delay(TimeSpan.FromSeconds(1f));

            var stateChangeEvent = _world.NewEntity();
            stateChangeEvent.Replace(new ChangeStateEvent
            {
                NewStateName = GameStateType.Finished
            });

            await UniTask.Delay(TimeSpan.FromSeconds(1f));
        }

        private bool AllTilesInOrder()
        {
            return true;
        }
    }
}