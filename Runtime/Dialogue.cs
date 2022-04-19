using Malee.List;
using UnityEngine;

namespace Fog.Dialogue {
    /// <summary>
    ///     Creates a scriptable object for an array of dialogue lines, so that it can be saved as a file.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "FoG/DialogueModule/Dialogue")]
    public class Dialogue : ScriptableObject {
#if UNITY_EDITOR
        protected static ReorderableDialogueList clipboard = null;

        [ContextMenu("Copy")]
        private void CopyLines() {
            // clipboard = lines.Clone() as ReorderableDialogueList;
            clipboard = new ReorderableDialogueList();
            foreach (DialogueLine line in lines) {
                clipboard.Add(line.Clone());
            }
        }

        [ContextMenu("Paste")]
        private void PasteLines() {
            if (clipboard != null) {
                // lines = clipboard.Clone() as ReorderableDialogueList;
                UnityEditor.Undo.RecordObject(this, "Pasted Dialogue Lines");
                lines = new ReorderableDialogueList();
                foreach (DialogueLine line in clipboard) {
                    lines.Add(line.Clone());
                }
            }
        }
#endif
        [Reorderable] public ReorderableDialogueList lines;

        public virtual void BeforeDialogue() {
            if (Agent.Instance) {
                Agent.Instance.BlockInteractions();
            }
            DialogueHandler.instance.OnDialogueStart -= BeforeDialogue;
        }

        public virtual void AfterDialogue() {
            if (Agent.Instance) {
                Agent.Instance.AllowInteractions();
            }
            DialogueHandler.instance.OnDialogueEnd -= AfterDialogue;
        }
    }

    [System.Serializable]
    public class ReorderableDialogueList : ReorderableArray<DialogueLine> { }
}