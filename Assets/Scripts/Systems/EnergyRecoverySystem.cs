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
                var recoveredAmount = _energyService.RecoverEnergy();

                if (recoveredAmount > 0)
                {
                    var updateEvent = _world.NewEntity();
                    updateEvent.Replace(new UpdateControlPanelEnergyEvent
                    {
                        EnergyAmount = _energyService.GetBalance(),
                        EnergyChange = recoveredAmount
                    });

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
            var timeLeft = _energyService.GetTimeUntilNextRecovery();
            return Math.Max(1f, (float)timeLeft.TotalSeconds);
        }
    }
}
