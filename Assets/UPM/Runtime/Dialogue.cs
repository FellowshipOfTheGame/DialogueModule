using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

namespace Fog.Dialogue {
    /// <summary>
    ///     Creates a scriptable object for an array of dialogue lines, so that it can be saved as a file.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "FoG/DialogueModule/Dialogue")]
    public class Dialogue : ScriptableObject, IDialogue {
        [SerializeField] private List<DialogueLine> lines = new();
        public List<DialogueLine> Lines => lines;

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
        }

        public virtual void AfterDialogue() {
            if (Agent.Instance) Agent.Instance.AllowInteractions();
        }

        public virtual void StartDialogue() {
            DialogueHandler.instance.StartDialogue(this);
        }

        [ContextMenu("Parse Tags (TMPro default)")]
        protected virtual void ParseLineTags() {
            if (lines.Count < 1) return;

            foreach (DialogueLine dialogueLine in lines) {
                dialogueLine.ParseTags(IDialogue.TMProTagFactory);
            }
        }

        protected void OnEnable() {
            ParseLineTags();
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