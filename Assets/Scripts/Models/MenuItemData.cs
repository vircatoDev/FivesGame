using UnityEngine;

namespace Scripts.Models
{
    public struct MenuItemData
    {
        public string Id;
        public Sprite Image;
        public string TitleText;
        public string BottomText;
        public bool Offer;
        public MenuItemType MenuItemType;
    }
}