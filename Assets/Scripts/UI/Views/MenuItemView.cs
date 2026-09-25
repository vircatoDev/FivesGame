using System;
using Scripts.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.Views
{
    public class MenuItemView : MonoBehaviour
    {
        [SerializeField] private Image frame;
        [SerializeField] private Sprite centeredFrame;
        [SerializeField] private Sprite sideFrame;
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
            itemImage.SetCover(itemData.Image);
            itemText.text = itemData.TitleText;

            // Themes and puzzles share one card: an offer shows the lock and the buy button, otherwise the status text.
            lockImage.SetActive(itemData.Offer);
            unlockButton.gameObject.SetActive(itemData.Offer);
            progressText.gameObject.SetActive(!itemData.Offer);
            if (itemData.Offer)
            {
                unlockOfferText.text = itemData.BottomText;
                unlockButton.onClick.RemoveAllListeners();
                unlockButton.onClick.AddListener(() => onBuyClick?.Invoke(_tileId));
                return;
            }

            progressText.text = itemData.BottomText;

            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(() => onTileClick?.Invoke(_tileId));
        }

        public void SetCentered(bool centered) => frame.sprite = centered ? centeredFrame : sideFrame;

        public string GetId()
        {
            return _tileId;
        }
    }
}