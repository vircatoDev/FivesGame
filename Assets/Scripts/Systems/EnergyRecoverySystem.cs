using System;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Services;
using UnityEngine;

namespace Scripts.Systems
{
    public class EnergyRecoverySystem : IEcsRunSystem
    {
        private readonly EcsWorld _world;
        private readonly EnergyService _energyService;

        private int _oldBalance;
        private int _newBalance;

        private float _nextCheckTime = 0f;

        public EnergyRecoverySystem(EnergyService energyService)
        {
            _energyService = energyService;
        }

        public void Run()
        {
            float currentTime = Time.time;

            if (currentTime >= _nextCheckTime)
            {
                _oldBalance = _energyService.GetBalance();
                _energyService.GetBalance();

                _newBalance = _energyService.GetBalance();

                if (_oldBalance != _newBalance)
                {
                    var updateEvent = _world.NewEntity();
                    updateEvent.Get<UpdateControlPanelEnergyEvent>().EnergyAmount = _newBalance;

                    var saveDataEvent = _world.NewEntity();
                    saveDataEvent.Replace(new SaveDataEvent
                    {
                        StorableObject = _energyService
                    });
                }

                _nextCheckTime = currentTime + GetNextCheckDelay();
            }
        }

        private float GetNextCheckDelay()
        {
            DateTime now = DateTime.Now;
            TimeSpan elapsed = now - _energyService.GetLastRecoveryTime();
            TimeSpan timeLeft = _energyService.GetRecoveryInterval() - elapsed;

            // if energy full check more intensive
            if (timeLeft <= TimeSpan.Zero)
            {
                return 60f;
            }

            return (float)timeLeft.TotalSeconds;
        }
    }
}