using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Fog.Dialogue {
    [RequireComponent(typeof(RectTransform))]
    public class DialogueOption : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI textField;
        [SerializeField] private Image focusIndicator;
        public UnityAction OnExit;
        public UnityAction OnFocus;

        public UnityAction OnSelect = null;
        public Dialogue NextDialogue { get; private set; }

        private void Awake() {
            if (!focusIndicator) return;

            focusIndicator.enabled = false;
            OnFocus += ToggleFocus;
            OnExit += ToggleFocus;
        }

        public virtual void Configure(DialogueOptionInfo info) {
            textField.text = info.text;
            NextDialogue = info.nextDialogue;
        }

        protected virtual void ToggleFocus() {
            if (focusIndicator) focusIndicator.enabled = !focusIndicator.enabled;
        }
    }
}