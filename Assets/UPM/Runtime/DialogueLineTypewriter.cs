using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Fog.Dialogue {
    public class DialogueLineTypewriter {
        private const string invisibilityTag = "<color=#00000000>";
        private readonly StringBuilder outputBuilder = new();
        private int visibleIndex = -1;
        private int invisibleIndex = -1;
        public bool ReachedTheEnd =>
            currentLine != null &&
            (visibleIndex >= currentLine.VisibleString.Length - 1
             || invisibleIndex >= currentLine.InvisibleString.Length - 1);
        private int tagIndex = -1;
        private string prefix = string.Empty;
        private DialogueLine currentLine = null;
        private readonly List<DialogueTextTag> pendingTags = new();
        private readonly List<DialogueTextTag> sortedPendingTags = new();

        public void Reset(DialogueLine line, string linePrefix = null) {
            prefix = linePrefix ?? string.Empty;
            currentLine = line;
            pendingTags.Clear();
            sortedPendingTags.Clear();
            visibleIndex = -1;
            invisibleIndex = -1;
            tagIndex = currentLine.Tags.Count > 0 ? 0 : -1;
        }

        public string GetOutputString() {
            outputBuilder.Clear();
            outputBuilder.Append(prefix);
            visibleIndex = Math.Min(visibleIndex, currentLine.VisibleString.Length - 1);
            invisibleIndex = Math.Min(invisibleIndex, currentLine.InvisibleString.Length - 1);
            outputBuilder.Append(currentLine.VisibleString, 0, visibleIndex + 1);
            for (int index = pendingTags.Count - 1; index >= 0; index--) {
                outputBuilder.Append(pendingTags[index].ClosingTag);
            }
            outputBuilder.Append(invisibilityTag);
            foreach (DialogueTextTag tag in pendingTags) {
                outputBuilder.Append(tag.InvisibleTag);
            }
            outputBuilder.Append(currentLine.InvisibleString, invisibleIndex + 1,
                                 currentLine.InvisibleString.Length - invisibleIndex - 1);
            return outputBuilder.ToString();
        }

        public int AdvanceTypingIndex() {
            if (ReachedTheEnd) {
                SkipToTheEnd();
                return 0;
            }
            visibleIndex += 1;
            invisibleIndex += 1;
            if (tagIndex < 0 || !NextCharacterIsTagStart()) return 1;

            int typedCount = ParseAndUpdateTags();
            if (typedCount > 0) return typedCount;

            return ReachedTheEnd ? 0 : 1;
        }

        private int ParseAndUpdateTags() {
            bool shouldCheck = true;

            while (shouldCheck) {
                if (tagIndex < currentLine.Tags.Count && visibleIndex == currentLine.Tags[tagIndex].StartIndex) {
                    int typedLength = ParseNewTag();
                    if (typedLength > 0) {
                        if (visibleIndex < currentLine.VisibleString.Length - 1) visibleIndex--;
                        if (invisibleIndex < currentLine.InvisibleString.Length - 1) invisibleIndex--;
                        return typedLength;
                    }
                } else {
                    ClosePendingTag();
                }
                shouldCheck = visibleIndex < currentLine.VisibleString.Length && NextCharacterIsTagStart();
            }
            return 0;
        }

        private bool NextCharacterIsTagStart() {
            return (tagIndex < currentLine.Tags.Count && visibleIndex == currentLine.Tags[tagIndex].StartIndex)
                   || (sortedPendingTags.Count > 0 && visibleIndex == sortedPendingTags[0].ClosingTagIndex);
        }

        private int ParseNewTag() {
            DialogueTextTag newTag = currentLine.Tags[tagIndex++];
            visibleIndex = Mathf.Min(newTag.EndIndex + 1, currentLine.VisibleString.Length - 1);
            invisibleIndex = Mathf.Min(newTag.InvisibleEndIndex + 1, currentLine.InvisibleString.Length - 1);
            if (newTag.ClosingTagIndex < 0) return newTag.TypedLength;

            pendingTags.Add(newTag);
            sortedPendingTags.Add(newTag);
            sortedPendingTags.Sort(CompareTags);
            return newTag.TypedLength;
        }

        private static int CompareTags(DialogueTextTag x, DialogueTextTag y) {
            return x.ClosingTagIndex.CompareTo(y.ClosingTagIndex);
        }

        private void ClosePendingTag() {
            DialogueTextTag closedTag = sortedPendingTags[0];
            sortedPendingTags.RemoveAt(0);
            pendingTags.Remove(closedTag);
            visibleIndex = Mathf.Min(visibleIndex + closedTag.ClosingTag.Length,
                                     currentLine.VisibleString.Length - 1);
            invisibleIndex = Mathf.Min(invisibleIndex + closedTag.ClosingTag.Length,
                                       currentLine.InvisibleString.Length - 1);
        }

        public void SkipToTheEnd() {
            visibleIndex = currentLine.VisibleString.Length - 1;
            invisibleIndex = currentLine.InvisibleString.Length - 1;
            if (tagIndex >= 0) tagIndex = currentLine.Tags.Count;
            pendingTags.Clear();
            sortedPendingTags.Clear();
        }
    }
}