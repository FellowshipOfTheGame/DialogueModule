using UnityEngine;

namespace Fog.Dialogue {
    /// <summary>
    ///     This is the dialogue instance, which will be in a list in the inspector
    ///     There are getters but no setter - To prevent edit from outside scripts, overwriting dialogue made by the writers
    ///     The only way to edit dialogue is from the inspector, if you want to change this, just add a setter to the property
    /// </summary>
    [System.Serializable]
    public class DialogueLine {
        [Header("Dialogue Properties")]
        [SerializeField] private DialogueEntity speaker = null;
        [SerializeField] [TextArea(3, 5)] private string text = null;

        public DialogueLine(DialogueEntity speaker, string text) {
            this.speaker = speaker;
            this.text = $"{text}";
        }

        public DialogueLine Clone() {
            return new DialogueLine(speaker, text);
        }

        public string Title => (speaker == null) ? null : speaker.DialogueName;
        public Color Color => (speaker == null) ? Color.white : speaker.DialogueColor;
        public Sprite Portrait => (speaker == null) ? null : speaker.DialoguePortrait;
        public string Text => text;
    }
}
