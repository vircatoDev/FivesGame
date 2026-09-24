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
        [FormerlySerializedAs("replayButton")]
        [SerializeField] private UnityEngine.UI.Button redoButton;
        [SerializeField] private TextMeshProUGUI movesLabel;

        private Action<BoardControl> _onControl;

        public void Initialize(Action<BoardControl> onControl) => _onControl = onControl;

        private void Awake()
        {
            undoButton.onClick.AddListener(Undo);
            redoButton.onClick.AddListener(Redo);
            Refresh("", false, false);
        }

        private void OnDestroy()
        {
            undoButton.onClick.RemoveListener(Undo);
            redoButton.onClick.RemoveListener(Redo);
        }

        public void Refresh(string moves, bool canUndo, bool canRedo)
        {
            movesLabel.text = moves;
            undoButton.interactable = canUndo;
            redoButton.interactable = canRedo;
        }

        private void Undo() => _onControl?.Invoke(BoardControl.Undo);
        private void Redo() => _onControl?.Invoke(BoardControl.Redo);
    }
}
