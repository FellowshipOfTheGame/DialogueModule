using System;
using UnityEngine;

namespace Fog.Dialogue {
    [Serializable]
    public struct DialogueOptionInfo : IDialogueOption {
        [SerializeField, TextArea] private string text;
        public string Text => text;
        [SerializeField] private Dialogue nextDialogue;

        public void Select() {
            if (nextDialogue)
                nextDialogue.StartDialogue();
            else
                DialogueHandler.Instance.InterruptDialogue();
        }
    }
}