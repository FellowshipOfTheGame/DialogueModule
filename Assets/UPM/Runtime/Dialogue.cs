using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Fog.Dialogue {
    /// <summary>
    ///     Creates a scriptable object for an array of dialogue lines, so that it can be saved as a file.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "FoG/DialogueModule/Dialogue")]
    public class Dialogue : ScriptableObject {
        public List<DialogueLine> lines;

        protected void CopyFrom(Dialogue otherDialogue) {
            lines.Clear();
            lines.AddRange(otherDialogue.lines);
        }

        public virtual object Clone() {
            Dialogue clone = CreateInstance<Dialogue>();
            clone.CopyFrom(this);
            return clone;
        }

        public virtual void BeforeDialogue() {
            if (Agent.Instance) Agent.Instance.BlockInteractions();
            DialogueHandler.instance.OnDialogueStart -= BeforeDialogue;
        }

        public virtual void AfterDialogue() {
            if (Agent.Instance) Agent.Instance.AllowInteractions();
            DialogueHandler.instance.OnDialogueEnd -= AfterDialogue;
        }
#if UNITY_EDITOR
        private static List<DialogueLine> clipboard;

        [ContextMenu("Copy")]
        private void CopyLines() {
            if (clipboard == null)
                clipboard = new List<DialogueLine>();
            else
                clipboard.Clear();
            clipboard.AddRange(lines);
        }

        [ContextMenu("Paste")]
        private void PasteLines() {
            if (clipboard == null || clipboard.Count < 1) return;

            Undo.RecordObject(this, $"Pasted Dialogue Lines ({name})");
            lines.Clear();
            lines.AddRange(clipboard);
            EditorUtility.SetDirty(this);
        }
#endif
    }
}