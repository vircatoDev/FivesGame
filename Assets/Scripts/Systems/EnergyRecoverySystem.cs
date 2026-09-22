using System;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Services;
using UnityEngine;

namespace Scripts.Systems
{
    public class EnergyRecoverySystem : IEcsRunSystem, IEcsInitSystem
    {
        private readonly EcsWorld _world;
        private readonly EnergyService _energyService;

        private float _nextCheckTime = 0f;

        public EnergyRecoverySystem(EnergyService energyService)
        {
            _energyService = energyService;
        }

        public void Init()
        {
            RecoverAndScheduleNextCheck();
        }

        public void Run()
        {
            if (Time.time >= _nextCheckTime)
                RecoverAndScheduleNextCheck();
        }

        private void RecoverAndScheduleNextCheck()
        {
            var recoveredAmount = _energyService.RecoverEnergy();
            if (recoveredAmount > 0)
            {
                _world.NewEntity().Replace(new UpdateControlPanelEnergyEvent
                {
                    EnergyAmount = _energyService.GetBalance(),
                    EnergyChange = recoveredAmount
                });
                _world.NewEntity().Replace(new SaveDataEvent { StorableObject = _energyService });
            }

            _nextCheckTime = Time.time + GetNextCheckDelay();
        }

        private float GetNextCheckDelay()
        {
            var timeLeft = _energyService.GetTimeUntilNextRecovery();
            return Math.Max(1f, (float)timeLeft.TotalSeconds);
        }
    }
}
