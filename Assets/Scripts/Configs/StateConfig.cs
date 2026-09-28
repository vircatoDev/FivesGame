using Fives.Models;
using Fives.UI.Views;
using UnityEngine;

namespace Fives.Configs
{
    [CreateAssetMenu(menuName = "Game/State Config")]
    public class StateConfig : ScriptableObject
    {
        public GameStateType StateName;
        public BaseView ScreenPrefab;
        public bool IsPopup;
    }
}