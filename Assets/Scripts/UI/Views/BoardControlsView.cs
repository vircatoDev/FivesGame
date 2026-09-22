using System;
using Scripts.Components;
using TMPro;
using UnityEngine;

namespace Scripts.UI.Views
{
    /// <summary>Inspector-configured controls; gameplay commands are handled by ECS.</summary>
    public sealed class BoardControlsView : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button undoButton;
        [SerializeField] private UnityEngine.UI.Button replayButton;
        [SerializeField] private TextMeshProUGUI replayLabel;
        [SerializeField] private TextMeshProUGUI statusLabel;

        private Action<BoardControl> _onControl;
        private bool _replaying;

        public void Initialize(Action<BoardControl> onControl) => _onControl = onControl;

        private void Awake()
        {
            undoButton.onClick.AddListener(Undo);
            replayButton.onClick.AddListener(Replay);
            Refresh("", false, false, false);
        }

        private void OnDestroy()
        {
            undoButton.onClick.RemoveListener(Undo);
            replayButton.onClick.RemoveListener(Replay);
        }

        public void Refresh(string status, bool canUndo, bool canReplay, bool replaying)
        {
            statusLabel.text = status;
            undoButton.interactable = canUndo;
            replayButton.interactable = canReplay;
            _replaying = replaying;
            replayLabel.text = replaying ? "Стоп" : "Повтор";
        }

        private void Undo() => _onControl?.Invoke(BoardControl.Undo);
        private void Replay() => _onControl?.Invoke(_replaying ? BoardControl.StopReplay : BoardControl.Replay);
    }
}
