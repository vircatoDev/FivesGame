using Scripts.Models;
using Scripts.UI.Views;
using UnityEngine;

namespace Scripts.Configs
{
    [CreateAssetMenu(menuName = "Game/State Config")]
    public class StateConfig : ScriptableObject
    {
        public GameStateType StateName;
        public BaseView ScreenPrefab;
        public bool IsPopup;
    }
}