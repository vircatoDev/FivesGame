using Scripts.Models;
using UnityEngine;

namespace Scripts.Configs
{
    [CreateAssetMenu(menuName = "Game/State Config")]
    public class StateConfig : ScriptableObject
    {
        public GameStateType StateName;
        public string ScreenPrefab;
        public bool  IsPopup;
        public StateConfig[] AllowedTransitions;
    }
}