using System;
using UnityEngine;

namespace Fog.Dialogue {
    /// <summary>
    ///     This is the dialogue instance, which will be in a list in the inspector
    ///     There are getters but no setter - To prevent edit from outside scripts, overwriting dialogue made by the writers
    ///     The only way to edit dialogue is from the inspector, if you want to change this, just add a setter to the property
    /// </summary>
    [Serializable]
    public class DialogueLine {
        [Header("Dialogue Properties")]

        [SerializeField] private DialogueEntity speaker;

        [SerializeField] [TextArea(3, 5)] private string text;

        public DialogueLine(DialogueLine otherLine) {
            speaker = otherLine.speaker;
            text = $"{otherLine.text}";
        }

        public DialogueLine(DialogueEntity speaker, string text) {
            this.speaker = speaker;
            this.text = $"{text}";
        }

        public virtual string Title => speaker == null ? null : speaker.DialogueName;
        public virtual Color Color => speaker == null ? Color.white : speaker.DialogueColor;
        public virtual Sprite Portrait => speaker == null ? null : speaker.DialoguePortrait;
        public virtual string Text => text;

        public virtual object Clone() {
            return new DialogueLine(this);
        }
    }
}