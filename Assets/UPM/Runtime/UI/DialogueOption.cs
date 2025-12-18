using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Fog.Dialogue {
    [RequireComponent(typeof(RectTransform))]
    public class DialogueOption : MonoBehaviour {
        [SerializeField] protected TextMeshProUGUI textField;
        [SerializeField] protected Image focusIndicator;
        public UnityAction OnExit;
        public UnityAction OnFocus;
        public UnityAction OnSelect = null;
        public RectTransform RectTransform { get; protected set; } = null;

        public IDialogueOption Option { get; protected set; }

        protected virtual void Awake() {
            RectTransform = GetComponent<RectTransform>();
            if (!focusIndicator) return;

            focusIndicator.enabled = false;
            OnFocus += ToggleFocus;
            OnExit += ToggleFocus;
        }

        public virtual void Configure(IDialogueOption option) {
            Option = option;
            textField.text = option.Text;
        }

        protected virtual void ToggleFocus() {
            if (focusIndicator) focusIndicator.enabled = !focusIndicator.enabled;
        }

        public virtual void ResetFocus() {
            if (focusIndicator) focusIndicator.enabled = false;
        }
    }
}