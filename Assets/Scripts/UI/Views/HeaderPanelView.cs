using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Views
{
    public class HeaderPanelView : MonoBehaviour
    {
        [SerializeField] private Button commonButton;
        [SerializeField] private Image commonButtonImg;
        [SerializeField] private TextMeshProUGUI commonButtonText;
        [SerializeField] private List<HeaderCommonBtnSkins> commonBtnSkins;

        [SerializeField] private TextMeshProUGUI starsAmountText;
        [SerializeField] private TextMeshProUGUI energyAmountText;
        [SerializeField] private CurrencyAnimationComponent currencyAnimationComponentPrefab;

        private EcsWorld _world;
    

        public void Initialize(EcsWorld world)
        {
            _world = world;
        }

        public void UpdateViewContent(string starsAmount, string energyAmount)
        {
            starsAmountText.text = starsAmount;
            energyAmountText.text = energyAmount;
        }

        private void OnHomeClick()
        {
            var stateChangeEvent = _world.NewEntity();

            stateChangeEvent.Replace(new ChangeStateEvent
            {
                NewStateName = GameStateType.MainMenu
            });
        }

        private void OnSettingsClick()
        {
            var stateChangeEvent = _world.NewEntity();

            stateChangeEvent.Replace(new ChangeStateEvent
            {
                NewStateName = GameStateType.Settings
            });
        }

        public void UpdateButtonLogic(UpdateControlPanelBtnLogicEvent btnLogicEvent)
        {
            commonButton.onClick.RemoveAllListeners();
            commonButton.onClick.AddListener(btnLogicEvent.CommonBtnCallback.Invoke);

            var skin = commonBtnSkins.FirstOrDefault(x => x.BtnType == btnLogicEvent.BtnType);

            if (skin != null)
            {
                commonButtonImg.sprite = skin.BtnSkin;
                commonButtonText.text = skin.BtnText;
            }
        }

        public void UpdateEnergy(UpdateControlPanelEnergyEvent updateControlPanelEventEvent)
        {
            if (updateControlPanelEventEvent.EnergyChange != 0)
            {
                energyAmountText.text = updateControlPanelEventEvent.EnergyAmount.ToString();
                PlayChangeCurrencyAnimation(energyAmountText, updateControlPanelEventEvent.EnergyChange);
            }

            if (updateControlPanelEventEvent.EnergyNotEnough)
            {
                PlayNotEnoughCurrencyAnimation(energyAmountText);
            }
        }

        public void UpdateStars(UpdateControlPanelStarsEvent updateControlPanelEventEvent)
        {
            if (updateControlPanelEventEvent.StarsChange != 0)
            {
                starsAmountText.text = updateControlPanelEventEvent.StarsAmount.ToString();
                PlayChangeCurrencyAnimation(starsAmountText, updateControlPanelEventEvent.StarsChange);
            }

            if (updateControlPanelEventEvent.StarsNotEnough)
            {
                PlayNotEnoughCurrencyAnimation(starsAmountText);
            }
        }

        private void PlayNotEnoughCurrencyAnimation(TextMeshProUGUI textMeshProUGUI)
        {
            textMeshProUGUI.transform.DOPunchScale(Vector3.one, 0.3f);
            textMeshProUGUI.DOColor(Color.red, 0.5f).OnComplete(() => { textMeshProUGUI.DOColor(Color.white, 0.5f); });
        }

        private void PlayChangeCurrencyAnimation(TextMeshProUGUI textMeshProUGUI, int starsChange)
        {
            textMeshProUGUI.transform.DOPunchScale(Vector3.one, 0.3f);
            var animationPrefab = Instantiate(currencyAnimationComponentPrefab, textMeshProUGUI.transform);
            animationPrefab.PlayAnimation(starsChange);
        }
    }


    [Serializable]
    public class HeaderCommonBtnSkins
    {
        public HeaderBtnType BtnType;
        public Sprite BtnSkin;
        public string BtnText;
    }
}