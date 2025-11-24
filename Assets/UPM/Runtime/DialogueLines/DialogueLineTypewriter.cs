using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Fog.Dialogue {
    public class DialogueLineTypewriter {
        protected const string invisibilityTag = "<color=#00000000>";
        protected readonly StringBuilder outputBuilder = new();
        protected int visibleIndex = -1;
        protected int invisibleIndex = -1;
        public bool ReachedTheEnd {
            get {
                if (currentLine == null) return true;

                return (visibleIndex >= currentLine.VisibleText.Length - 1
                        || invisibleIndex >= currentLine.InvisibleText.Length - 1);
            }
        }
        protected int tagIndex = -1;
        protected string prefix = string.Empty;
        protected DialogueLine currentLine = null;
        protected readonly List<DialogueTextTag> pendingTags = new();
        protected readonly List<DialogueTextTag> sortedPendingTags = new();
        protected readonly UnityEvent<DialogueTextTag> onTagProcessed;

        public DialogueLineTypewriter(UnityEvent<DialogueTextTag> tagProcessedEvent) {
            onTagProcessed = tagProcessedEvent;
            currentLine = null;
            pendingTags.Clear();
            sortedPendingTags.Clear();
        }

        public virtual void Reset(DialogueLine line, string linePrefix = null) {
            prefix = linePrefix ?? string.Empty;
            currentLine = line;
            pendingTags.Clear();
            sortedPendingTags.Clear();
            visibleIndex = -1;
            invisibleIndex = -1;
            tagIndex = currentLine.Tags.Count > 0 ? 0 : -1;
        }

        public virtual void ResetCurrentLine() {
            if (currentLine == null) return;

            Reset(currentLine, prefix);
        }

        public virtual string GetOutputString() {
            if (currentLine == null) return string.Empty;

            outputBuilder.Clear();
            outputBuilder.Append(prefix);
            visibleIndex = Math.Min(visibleIndex, currentLine.VisibleText.Length - 1);
            invisibleIndex = Math.Min(invisibleIndex, currentLine.InvisibleText.Length - 1);
            outputBuilder.Append(currentLine.VisibleText, 0, visibleIndex + 1);
            for (int index = pendingTags.Count - 1; index >= 0; index--) {
                outputBuilder.Append(pendingTags[index].ClosingTag);
            }
            outputBuilder.Append(invisibilityTag);
            foreach (DialogueTextTag tag in pendingTags) {
                outputBuilder.Append(tag.InvisibleTag);
            }
            outputBuilder.Append(currentLine.InvisibleText, invisibleIndex + 1,
                                 currentLine.InvisibleText.Length - invisibleIndex - 1);
            return outputBuilder.ToString();
        }

        public virtual int AdvanceTypingIndex() {
            if (ReachedTheEnd) {
                SkipToTheEnd();
                return 0;
            }
            visibleIndex += 1;
            invisibleIndex += 1;
            if (tagIndex < 0 || !NextCharacterIsTagStart()) return 1;

            int typedCount = ParseAndUpdateTags();
            return typedCount > 0 ? typedCount :
                ReachedTheEnd ? 0 : 1;
        }

        protected virtual bool NextCharacterIsTagStart() {
            return (tagIndex < currentLine.Tags.Count && visibleIndex == currentLine.Tags[tagIndex].StartIndex)
                   || (sortedPendingTags.Count > 0 && visibleIndex == sortedPendingTags[0].ClosingTagIndex);
        }

        protected virtual int ParseAndUpdateTags() {
            bool shouldCheck = true;

            while (shouldCheck) {
                if (tagIndex < currentLine.Tags.Count && visibleIndex == currentLine.Tags[tagIndex].StartIndex) {
                    int typedLength = ParseNewTag();
                    if (typedLength > 0) {
                        if (visibleIndex < currentLine.VisibleText.Length - 1) visibleIndex--;
                        if (invisibleIndex < currentLine.InvisibleText.Length - 1) invisibleIndex--;
                        return typedLength;
                    }
                } else {
                    ClosePendingTag();
                }
                shouldCheck = visibleIndex < currentLine.VisibleText.Length && NextCharacterIsTagStart();
            }
            return 0;
        }

        protected virtual int ParseNewTag() {
            DialogueTextTag newTag = currentLine.Tags[tagIndex++];
            visibleIndex = Mathf.Min(newTag.EndIndex + 1, currentLine.VisibleText.Length - 1);
            invisibleIndex = Mathf.Min(newTag.InvisibleEndIndex + 1, currentLine.InvisibleText.Length - 1);
            onTagProcessed?.Invoke(newTag);
            if (newTag.ClosingTagIndex < 0) return newTag.TypedLength;

            pendingTags.Add(newTag);
            sortedPendingTags.Add(newTag);
            sortedPendingTags.Sort(CompareTags);
            return newTag.TypedLength;
        }

        protected static int CompareTags(DialogueTextTag x, DialogueTextTag y) {
            return x.ClosingTagIndex.CompareTo(y.ClosingTagIndex);
        }

        protected virtual void ClosePendingTag() {
            DialogueTextTag closedTag = sortedPendingTags[0];
            sortedPendingTags.RemoveAt(0);
            pendingTags.Remove(closedTag);
            visibleIndex = Mathf.Min(visibleIndex + closedTag.ClosingTag.Length,
                                     currentLine.VisibleText.Length - 1);
            invisibleIndex = Mathf.Min(invisibleIndex + closedTag.ClosingTag.Length,
                                       currentLine.InvisibleText.Length - 1);
        }

        public void SkipToTheEnd() {
            visibleIndex = currentLine.VisibleText.Length - 1;
            invisibleIndex = currentLine.InvisibleText.Length - 1;
            while (tagIndex >= 0 && tagIndex < currentLine.Tags.Count) {
                onTagProcessed?.Invoke(currentLine.Tags[tagIndex++]);
            }
            pendingTags.Clear();
            sortedPendingTags.Clear();
        }
    }
}