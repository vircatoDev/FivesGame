using System;
using Fives.Domain;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services.Interfaces;
using UnityEngine;

namespace Scripts.Services
{
    public class EnergyService : IStorable
    {
        private readonly int _maxEnergy;
        private readonly TimeSpan _recoveryInterval;
        private readonly IClock _clock;
        private EnergyWallet _wallet;

        public EnergyService(GlobalConfig config, PlayerDataSaveHelper saveHelper, IClock clock)
        {
            _maxEnergy = config.MaxEnergy;
            _recoveryInterval = TimeSpan.FromHours(config.EnergyRecoveryIntervalHours);
            _clock = clock;

            var savedEnergy = saveHelper.GetPlayerData().Energy;
            var balance = savedEnergy?.CurrentEnergy ?? config.InitialEnergy;
            var lastRecovery = savedEnergy?.LastRecoveryTime ?? _clock.UtcNow;
            _wallet = new EnergyWallet(Math.Clamp(balance, 0, _maxEnergy), _maxEnergy, _recoveryInterval, lastRecovery);
        }


        public int GetBalance()
        {
            return _wallet.Balance;
        }

        public void Add(int amount)
        {
            if (!_wallet.TryCredit(amount, _clock.UtcNow))
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }
        }

        public bool Spend(int amount)
        {
            return _wallet.TrySpend(amount);
        }

        public int RecoverEnergy()
        {
            return _wallet.Recover(_clock.UtcNow);
        }

        public DateTime GetLastRecoveryTime()
        {
            return _wallet.LastRecoveryUtc;
        }

        public TimeSpan GetTimeUntilNextRecovery()
        {
            return _wallet.TimeUntilNextRecovery(_clock.UtcNow);
        }

        public void SetDataFromSave(EnergyData data)
        {
            var balance = data?.CurrentEnergy ?? 0;
            var lastRecovery = data?.LastRecoveryTime ?? _clock.UtcNow;
            _wallet = new EnergyWallet(Math.Clamp(balance, 0, _maxEnergy), _maxEnergy, _recoveryInterval, lastRecovery);
        }

        public void UpdatePlayerData(GameSaveData playerData)
        {
            playerData.Energy ??= new EnergyData();
            playerData.Energy.CurrentEnergy = _wallet.Balance;
            playerData.Energy.LastRecoveryTime = _wallet.LastRecoveryUtc;
        }
    
    }

    [System.Serializable]
    public class EnergyData
    {
        [SerializeField] public int CurrentEnergy;
        [SerializeField] public DateTime LastRecoveryTime;
    }
}
