using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

namespace Fog.Dialogue {
    /// <summary>
    ///     Creates a scriptable object for an array of dialogue lines, so that it can be saved as a file.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "FoG/DialogueModule/Dialogue")]
    public class Dialogue : ScriptableObject {
        public List<DialogueLine> lines = new();
        protected static readonly ReadOnlyDictionary<string, DialogueTextTag.Constructor> TMProTagFactory =
            new ReadOnlyDictionary<string, DialogueTextTag.Constructor>(BuildTMProTagFactory());

        public static Dictionary<string, DialogueTextTag.Constructor> BuildTMProTagFactory() {
            Dictionary<string, DialogueTextTag.Constructor> dict = new() {
                { "align", SimpleTextTag.CreateSimpleTag }, { "allcaps", SimpleTextTag.CreateSimpleTag },
                { "alpha", SimpleColoredTag.CreateColoredTag }, { "b", SimpleTextTag.CreateSimpleTag },
                { "br", SimpleTextTag.CreateSimpleTag }, { "color", SimpleColoredTag.CreateColoredTag },
                { "cspace", SimpleTextTag.CreateSimpleTag }, { "font", SimpleTextTag.CreateSimpleTag },
                { "font-weight", SimpleTextTag.CreateSimpleTag }, { "gradient", SimpleTextTag.CreateSimpleTag },
                { "i", SimpleTextTag.CreateSimpleTag }, { "indent", SimpleTextTag.CreateSimpleTag },
                { "line-height", SimpleTextTag.CreateSimpleTag }, { "line-indent", SimpleTextTag.CreateSimpleTag },
                { "lowercase", SimpleTextTag.CreateSimpleTag }, { "margin", SimpleTextTag.CreateSimpleTag },
                { "mspace", SimpleTextTag.CreateSimpleTag }, { "nobr", SimpleTextTag.CreateSimpleTag },
                { "page", SimpleTextTag.CreateSimpleTag }, { "rotate", SimpleTextTag.CreateSimpleTag },
                { "s", SimpleTextTag.CreateSimpleTag }, { "size", SimpleTextTag.CreateSimpleTag },
                { "smallcaps", SimpleTextTag.CreateSimpleTag }, { "space", SimpleTextTag.CreateSimpleTag },
                { "sprite", SimpleColoredTag.CreateColoredTag }, { "strikethrough", SimpleTextTag.CreateSimpleTag },
                { "style", SimpleTextTag.CreateSimpleTag }, { "sub", SimpleTextTag.CreateSimpleTag },
                { "sup", SimpleTextTag.CreateSimpleTag }, { "u", SimpleTextTag.CreateSimpleTag },
                { "uppercase", SimpleTextTag.CreateSimpleTag }, { "voffset", SimpleTextTag.CreateSimpleTag },
                { "width", SimpleTextTag.CreateSimpleTag },
            };
            return dict;
        }

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
        protected void ParseLineTags() {
            if (lines.Count < 1) return;

            foreach (DialogueLine dialogueLine in lines) {
                dialogueLine.ParseTags(TMProTagFactory);
            }
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