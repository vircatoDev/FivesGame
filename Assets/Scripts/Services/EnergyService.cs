using System;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services.Interfaces;
using UnityEngine;

namespace Scripts.Services
{
    public class EnergyService : ICurrencyService, IStorable
    {
        private int _currentEnergy;
        private readonly int _maxEnergy;
        private readonly TimeSpan _recoveryInterval;
        private DateTime _lastRecoveryTime;

        public EnergyService(GlobalConfig config, PlayerDataSaveHelper saveHelper)
        {
            _currentEnergy = config.InitialEnergy;
            _maxEnergy = config.MaxEnergy;
            _recoveryInterval = TimeSpan.FromHours(config.EnergyRecoveryIntervalHours);
            _lastRecoveryTime = DateTime.Now;

            SetDataFromSave(saveHelper.GetPlayerData().Energy);
        }


        public int GetBalance()
        {
            RecoverEnergy();
            return _currentEnergy;
        }

        public void Add(int amount)
        {
            _currentEnergy = Math.Min(_currentEnergy + amount, _maxEnergy);
        }

        public bool Spend(int amount)
        {
            if (_currentEnergy >= amount)
            {
                _currentEnergy -= amount;
                return true;
            }

            return false;
        }
    
        public DateTime GetLastRecoveryTime()
        {
            return _lastRecoveryTime;
        }

        public TimeSpan GetRecoveryInterval()
        {
            return _recoveryInterval;
        }

        private void RecoverEnergy()
        {
            var now = DateTime.Now;
            var elapsed = now - _lastRecoveryTime;

            if (elapsed >= _recoveryInterval)
            {
                int recovered = (int)(elapsed.TotalHours / _recoveryInterval.TotalHours);
                _currentEnergy = Math.Min(_currentEnergy + recovered, _maxEnergy);
                _lastRecoveryTime = now;
            }
        }


        public void SetDataFromSave(EnergyData data)
        {
            _currentEnergy = data.CurrentEnergy;
            _lastRecoveryTime = data.LastRecoveryTime;
        }

        public void UpdatePlayerData(GameSaveData playerData)
        {
            playerData.Energy.CurrentEnergy = _currentEnergy;
            playerData.Energy.LastRecoveryTime = _lastRecoveryTime;
        }
    
    }

    [System.Serializable]
    public class EnergyData
    {
        [SerializeField] public int CurrentEnergy;
        [SerializeField] public DateTime LastRecoveryTime;
    }
}