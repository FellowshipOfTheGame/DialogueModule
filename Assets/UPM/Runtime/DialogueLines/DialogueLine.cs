using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;

namespace Fog.Dialogue {
    [Serializable]
    public class DialogueLine {
        [Header("Dialogue Properties")]
        [SerializeField] protected DialogueEntity speaker;
        public DialogueEntity Speaker => speaker;

        [SerializeField] [TextArea(3, 5)] protected string text;
        public virtual string SerializedText => text;

        protected List<DialogueTextTag> tags = new();
        protected ReadOnlyCollection<DialogueTextTag> readonlyTags = null;
        public ReadOnlyCollection<DialogueTextTag> Tags => readonlyTags ??= tags.AsReadOnly();
        protected StringBuilder tagBuilder = new();
        protected StringBuilder visibleLineBuilder = new();
        protected StringBuilder invisibleLineBuilder = new();

        public string VisibleText { get; protected set; }
        public string InvisibleText { get; protected set; }

        public virtual object Clone() {
            return new DialogueLine(this);
        }

        public void CopyFrom(DialogueLine otherLine) {
            speaker = otherLine.speaker;
            text = $"{otherLine.text}";
        }

        public DialogueLine(DialogueLine otherLine) {
            CopyFrom(otherLine);
            Init();
            VisibleText = text;
            InvisibleText = text;
        }

        public DialogueLine(DialogueEntity speaker, string text) {
            this.speaker = speaker;
            this.text = $"{text}";
            Init();
            VisibleText = this.text;
            InvisibleText = this.text;
        }

        protected void Init() {
            tags ??= new List<DialogueTextTag>();
            tagBuilder ??= new StringBuilder();
            visibleLineBuilder ??= new StringBuilder();
            invisibleLineBuilder ??= new StringBuilder();
        }

        public virtual void ParseTags(ReadOnlyDictionary<string, DialogueTextTag.Constructor> tagFactory) {
            Init();
            tags.Clear();
            visibleLineBuilder.Clear();
            invisibleLineBuilder.Clear();
            for (int index = 0; index < text.Length; index++) {
                if (text[index] == DialogueTextTag.OpenTagChar) {
                    index = ParseTag(index, tagFactory);
                } else {
                    visibleLineBuilder.Append(text[index]);
                    invisibleLineBuilder.Append(text[index]);
                }
            }
            RemoveInvalidTags();
            VisibleText = visibleLineBuilder.ToString();
            InvisibleText = invisibleLineBuilder.ToString();
        }

        protected int ParseTag(int startIndex, ReadOnlyDictionary<string, DialogueTextTag.Constructor> tagFactory) {
            tagBuilder.Clear();
            int index = CheckForClosingTag(startIndex);
            bool isClosing = index != startIndex;
            string tagName = null;
            while (++index < text.Length) {
                if (text[index] == DialogueTextTag.OpenTagChar) {
                    CancelIncompleteTagParse();
                    return index - 1;
                }

                if (text[index] == DialogueTextTag.CloseTagChar)
                    return ParseEndOfTag(index, tagFactory, tagName, isClosing);

                tagName = ParseValidCharacter(index, tagName, isClosing);
            }
            CancelIncompleteTagParse();
            return text.Length - 1;
        }

        protected int CheckForClosingTag(int index) {
            if (index >= text.Length - 1 || text[index + 1] != DialogueTextTag.ClosingTagIndicator) return index;

            tagBuilder.Append(DialogueTextTag.ClosingTagIndicator);
            return index + 1;
        }

        protected void CancelIncompleteTagParse() {
            visibleLineBuilder.Append(DialogueTextTag.OpenTagChar);
            invisibleLineBuilder.Append(DialogueTextTag.OpenTagChar);
            visibleLineBuilder.Append(tagBuilder);
            invisibleLineBuilder.Append(tagBuilder);
            tagBuilder.Clear();
        }

        protected virtual int ParseEndOfTag(
            int index, ReadOnlyDictionary<string, DialogueTextTag.Constructor> tagFactory, string tagName,
            bool isClosing) {
            if (tagName == null && tagBuilder.Length < 1) {
                tagBuilder.Append(DialogueTextTag.CloseTagChar);
                CancelIncompleteTagParse();
                return index;
            }

            tagName ??= !isClosing ? tagBuilder.ToString() : tagBuilder.ToString().Remove(0, 1);

            if (!tagFactory.ContainsKey(tagName)) {
                tagBuilder.Append(DialogueTextTag.CloseTagChar);
                CancelIncompleteTagParse();
                return index;
            }

            return isClosing ? CloseValidOpenTag(index, tagName) : CreateNewTag(index, tagFactory, tagName);
        }

        protected virtual int CloseValidOpenTag(int index, string tagName) {
            for (int tagIndex = tags.Count - 1; tagIndex >= 0; tagIndex--) {
                if (tags[tagIndex].ClosingTagIndex >= 0 || string.IsNullOrEmpty(tags[tagIndex].ClosingTag)
                                                        || tags[tagIndex].tagName != tagName)
                    continue;

                tags[tagIndex].SetClosingTagIndex(visibleLineBuilder.Length);
                visibleLineBuilder.Append(tags[tagIndex].ClosingTag);
                invisibleLineBuilder.Append(tags[tagIndex].ClosingTag);
                return index;
            }
            tagBuilder.Append(DialogueTextTag.CloseTagChar);
            CancelIncompleteTagParse();
            return index;
        }

        protected virtual int CreateNewTag(
            int index, ReadOnlyDictionary<string, DialogueTextTag.Constructor> tagFactory, string tagName) {
            DialogueTextTag newTag =
                tagFactory[tagName].Invoke(visibleLineBuilder.Length, tagName, tagBuilder.ToString());
            newTag.SetInvisibleIndexes(invisibleLineBuilder.Length);
            tags.Add(newTag);
            visibleLineBuilder.Append(newTag.VisibleTag);
            invisibleLineBuilder.Append(newTag.InvisibleTag);
            return index;
        }

        protected virtual string ParseValidCharacter(int index, string tagName, bool isClosing) {
            if (tagName == null && DialogueTextTag.TagStopChars.Contains(text[index]))
                tagName = !isClosing ? tagBuilder.ToString() : tagBuilder.ToString().Remove(0, 1);
            tagBuilder.Append(text[index]);
            return tagName;
        }

        protected void RemoveInvalidTags() {
            for (int index = tags.Count - 1; index >= 0; index--) {
                if (tags[index].MustClose && tags[index].ClosingTagIndex < 0) tags.RemoveAt(index);
            }
        }
    }
}