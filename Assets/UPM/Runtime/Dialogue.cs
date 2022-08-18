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
            clipboard = (ReorderableDialogueList)lines.Clone();
        }

        [ContextMenu("Paste")]
        private void PasteLines() {
            if (clipboard == null)
                return;
            UnityEditor.Undo.RecordObject(this, $"Pasted Dialogue Lines ({name})");
            lines = (ReorderableDialogueList)clipboard.Clone();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        [Reorderable] public ReorderableDialogueList lines;

        protected void CopyFrom(Dialogue otherDialogue) {
            otherDialogue.lines = (ReorderableDialogueList)otherDialogue.lines.Clone();
        }

        public virtual object Clone() {
            Dialogue clone = CreateInstance<Dialogue>();
            clone.CopyFrom(this);
            return clone;
        }

        public virtual void BeforeDialogue() {
            if (Agent.Instance)
                Agent.Instance.BlockInteractions();
            DialogueHandler.instance.OnDialogueStart -= BeforeDialogue;
        }

        public virtual void AfterDialogue() {
            if (Agent.Instance)
                Agent.Instance.AllowInteractions();
            DialogueHandler.instance.OnDialogueEnd -= AfterDialogue;
        }
    }
}