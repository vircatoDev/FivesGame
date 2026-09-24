using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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

        public void UpdateViewContent(string starsAmount, string energyAmount)
        {
            starsAmountText.text = starsAmount;
            energyAmountText.text = energyAmount;
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
                commonButtonText.color = skin.TextColor;
            }
        }

        public void UpdateCurrency(in CurrencyChangedEvent evt)
        {
            var text = evt.Currency == Currency.Stars ? starsAmountText : energyAmountText;

            if (evt.Delta != 0)
            {
                text.text = evt.Balance.ToString();
                PlayChangeCurrencyAnimation(text, evt.Delta);
            }

            if (evt.Insufficient)
            {
                PlayNotEnoughCurrencyAnimation(text);
            }
        }

        private void PlayNotEnoughCurrencyAnimation(TextMeshProUGUI textMeshProUGUI)
        {
            textMeshProUGUI.transform.DOPunchScale(Vector3.one, 0.3f);
            textMeshProUGUI.DOColor(Color.red, 0.5f).OnComplete(() => { textMeshProUGUI.DOColor(Color.white, 0.5f); });
        }

        private void PlayChangeCurrencyAnimation(TextMeshProUGUI textMeshProUGUI, int delta)
        {
            textMeshProUGUI.transform.DOPunchScale(Vector3.one, 0.3f);
            var animationPrefab = Instantiate(currencyAnimationComponentPrefab, textMeshProUGUI.transform);
            animationPrefab.PlayAnimation(delta);
        }
    }


    [Serializable]
    public class HeaderCommonBtnSkins
    {
        public HeaderBtnType BtnType;
        public Sprite BtnSkin;
        public string BtnText;
        public Color TextColor = Color.white;
    }
}