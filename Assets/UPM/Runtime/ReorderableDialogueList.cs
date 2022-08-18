using Malee.List;

namespace Fog.Dialogue {
    [System.Serializable]
    public class ReorderableDialogueList : ReorderableArray<DialogueLine> {
        public ReorderableDialogueList() : base() { }
        public ReorderableDialogueList(int length) : base(length) { }
        public new object Clone() {
            ReorderableDialogueList clone = new ReorderableDialogueList(Length);
            foreach (DialogueLine line in this) {
                clone.Add((DialogueLine)line.Clone());
            }
            return clone;
        }
    }
}
