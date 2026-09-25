using System;
using Scripts.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.UI.Views
{
    /// <summary>Inspector-configured controls; gameplay commands are handled by ECS.</summary>
    public sealed class BoardControlsView : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button undoButton;
        [FormerlySerializedAs("redoButton")]
        [SerializeField] private UnityEngine.UI.Button hintButton;
        [SerializeField] private TextMeshProUGUI hintPriceLabel;
        [SerializeField] private TextMeshProUGUI movesLabel;

        private Action<BoardControl> _onControl;

        public void Initialize(Action<BoardControl> onControl) => _onControl = onControl;

        private void Awake()
        {
            undoButton.onClick.AddListener(Undo);
            hintButton.onClick.AddListener(Hint);
            Refresh("", "", false, false);
        }

        private void OnDestroy()
        {
            undoButton.onClick.RemoveListener(Undo);
            hintButton.onClick.RemoveListener(Hint);
        }

        public void Refresh(string moves, string hintPrice, bool canUndo, bool canHint)
        {
            movesLabel.text = moves;
            hintPriceLabel.text = hintPrice;
            undoButton.interactable = canUndo;
            hintButton.interactable = canHint;
        }

        private void Undo() => _onControl?.Invoke(BoardControl.Undo);
        private void Hint() => _onControl?.Invoke(BoardControl.Hint);
    }
}
