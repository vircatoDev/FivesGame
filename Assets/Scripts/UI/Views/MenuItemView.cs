using System;
using Scripts.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Views
{
    public class MenuItemView : MonoBehaviour
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private GameObject lockImage;
        [SerializeField] private TextMeshProUGUI itemText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI unlockOfferText;
        [SerializeField] private Button itemButton;
        [SerializeField] private Button unlockButton;

        private string _tileId;

        public void Initialize(MenuItemData itemData, Action<string> onTileClick, Action<string> onBuyClick)
        {
            _tileId = itemData.Id;
            itemImage.sprite = itemData.Image;
            itemText.text = itemData.TitleText;

            if (itemData.MenuItemType == MenuItemType.Theme)
            {
                if (itemData.Offer)
                {
                    lockImage.SetActive(true);
                    progressText.gameObject.SetActive(false);

                    unlockOfferText.text = itemData.BottomText;
                    unlockButton.gameObject.SetActive(true);
                    unlockButton.onClick.RemoveAllListeners();
                    unlockButton.onClick.AddListener(() => onBuyClick?.Invoke(_tileId));
                    return;
                }
                else
                {
                    lockImage.SetActive(false);
                    unlockButton.gameObject.SetActive(false);

                    progressText.text = itemData.BottomText;
                    progressText.gameObject.SetActive(true);
                }
            }
            else
            {
                progressText.gameObject.SetActive(false);
            }

            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(() => onTileClick?.Invoke(_tileId));
        }

        public string GetId()
        {
            return _tileId;
        }
    }
}