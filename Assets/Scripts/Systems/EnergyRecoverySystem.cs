using System;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using Scripts.Services;
using UnityEngine;
using Scripts.Services.Interfaces;

namespace Scripts.Systems
{
    public class EnergyRecoverySystem : IEcsRunSystem, IEcsInitSystem
    {
        private readonly EcsWorld _world;
        private readonly EnergyService _energyService;
        private readonly IFrameTime _time = null;

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
            if (_time.Time >= _nextCheckTime)
                RecoverAndScheduleNextCheck();
        }

        private void RecoverAndScheduleNextCheck()
        {
            var recoveredAmount = _energyService.RecoverEnergy();
            if (recoveredAmount > 0)
            {
                _world.Send(CurrencyChangedEvent.Changed(Currency.Energy, _energyService.GetBalance(), recoveredAmount));
                _world.Send<SaveDataEvent>();
            }

            _nextCheckTime = _time.Time + GetNextCheckDelay();
        }

        private float GetNextCheckDelay()
        {
            var timeLeft = _energyService.GetTimeUntilNextRecovery();
            return Math.Max(1f, (float)timeLeft.TotalSeconds);
        }
    }
}
