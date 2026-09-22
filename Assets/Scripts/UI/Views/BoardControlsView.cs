using System;
using Scripts.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.UI.Views
{
    /// <summary>Small touch toolbar below the existing preview panel.</summary>
    public sealed class BoardControlsView
    {
        private readonly UnityEngine.UI.Button _undo;
        private readonly UnityEngine.UI.Button _replay;
        private readonly TextMeshProUGUI _replayLabel;
        private readonly TextMeshProUGUI _status;
        private bool _replaying;

        public BoardControlsView(Transform parent, TMP_FontAsset font, Action<BoardControl> onControl)
        {
            var root = new GameObject("BoardControls", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(parent, false);
            root.gameObject.layer = parent.gameObject.layer;
            root.anchorMin = root.anchorMax = new Vector2(0, 0.5f);
            root.pivot = new Vector2(0, 0.5f);
            root.anchoredPosition = new Vector2(25, -350);
            root.sizeDelta = new Vector2(450, 80);
            var layout = root.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.spacing = 16;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = true;

            _undo = CreateButton(root, font, "Отмена", () => onControl(BoardControl.Undo));
            _replay = CreateButton(root, font, "Повтор", () =>
                onControl(_replaying ? BoardControl.StopReplay : BoardControl.Replay));
            _replayLabel = _replay.GetComponentInChildren<TextMeshProUGUI>();
            _status = CreateLabel(root, font, 23);
            _status.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().ignoreLayout = true;
            _status.rectTransform.anchorMin = new Vector2(0, 1);
            _status.rectTransform.anchorMax = new Vector2(1, 1);
            _status.rectTransform.pivot = new Vector2(0.5f, 0);
            _status.rectTransform.anchoredPosition = new Vector2(0, 12);
            _status.rectTransform.sizeDelta = new Vector2(0, 50);
            _status.color = new Color(0.08f, 0.26f, 0.51f);
            Refresh("", false, false, false);
        }

        public void Refresh(string status, bool canUndo, bool canReplay, bool replaying)
        {
            _status.text = status;
            _undo.interactable = canUndo;
            _replay.interactable = canReplay;
            _replaying = replaying;
            _replayLabel.text = replaying ? "Стоп" : "Повтор";
        }

        private static UnityEngine.UI.Button CreateButton(Transform parent, TMP_FontAsset font,
            string text, UnityAction onClick)
        {
            var buttonObject = new GameObject(text, typeof(RectTransform), typeof(UnityEngine.UI.Image),
                typeof(UnityEngine.UI.Button));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.layer = parent.gameObject.layer;
            var background = buttonObject.GetComponent<UnityEngine.UI.Image>();
            background.color = new Color(0.08f, 0.26f, 0.51f);
            var button = buttonObject.GetComponent<UnityEngine.UI.Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(onClick);
            var label = CreateLabel(buttonObject.transform, font, 32);
            label.text = text;
            return button;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, TMP_FontAsset font, float fontSize)
        {
            var label = new GameObject("Label", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            label.transform.SetParent(parent, false);
            label.gameObject.layer = parent.gameObject.layer;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.sizeDelta = Vector2.zero;
            label.font = font;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }
    }
}
