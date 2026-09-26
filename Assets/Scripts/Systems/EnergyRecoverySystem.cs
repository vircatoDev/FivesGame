using System;
using Leopotam.Ecs;
using Scripts.Services;
using Scripts.Services.Interfaces;

namespace Scripts.Systems
{
    /// <summary>Recovers energy when the next unit is due; CurrencySyncSystem reports the change.</summary>
    public class EnergyRecoverySystem : IEcsRunSystem, IEcsInitSystem
    {
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
            _energyService.RecoverEnergy();
            _nextCheckTime = _time.Time + GetNextCheckDelay();
        }

        private float GetNextCheckDelay()
        {
            var timeLeft = _energyService.GetTimeUntilNextRecovery();
            return Math.Max(1f, (float)timeLeft.TotalSeconds);
        }
    }
}
