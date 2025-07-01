using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using UnityEngine;

namespace Fog.Dialogue {
    /// <summary>
    ///     This is the dialogue instance, which will be in a list in the inspector
    ///     There are getters but no setter - To prevent edit from outside scripts, overwriting dialogue made by the writers
    ///     The only way to edit dialogue is from the inspector, if you want to change this, just add a setter to the property
    /// </summary>
    [Serializable]
    public class DialogueLine {
        [Header("Dialogue Properties")]
        [SerializeField] protected DialogueEntity speaker;

        [SerializeField] [TextArea(3, 5)] protected string text;

        protected List<DialogueTextTag> tags = new();
        public ReadOnlyCollection<DialogueTextTag> Tags => tags.AsReadOnly();
        protected StringBuilder tagBuilder = new();
        protected StringBuilder visibleLineBuilder = new();
        protected StringBuilder invisibleLineBuilder = new();

        public DialogueLine(DialogueLine otherLine) {
            speaker = otherLine.speaker;
            text = $"{otherLine.text}";
            Init();
            VisibleString = text;
            InvisibleString = text;
        }

        public DialogueLine(DialogueEntity speaker, string text) {
            this.speaker = speaker;
            this.text = $"{text}";
            Init();
            VisibleString = text;
            InvisibleString = text;
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
                if (text[index] == DialogueTextTag.OpenTagChar)
                    index = ParseTag(index, tagFactory);
                else {
                    visibleLineBuilder.Append(text[index]);
                    invisibleLineBuilder.Append(text[index]);
                }
            }
            RemoveInvalidTags();
            VisibleString = visibleLineBuilder.ToString();
            InvisibleString = invisibleLineBuilder.ToString();
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

                if (text[index] != DialogueTextTag.CloseTagChar) {
                    if (tagName == null && DialogueTextTag.TagStopChars.Contains(text[index]))
                        tagName = !isClosing ? tagBuilder.ToString() : tagBuilder.ToString().Remove(0, 1);
                    tagBuilder.Append(text[index]);
                    continue;
                }

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

                if (isClosing) {
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

                DialogueTextTag newTag =
                    tagFactory[tagName].Invoke(visibleLineBuilder.Length, tagName, tagBuilder.ToString());
                newTag.SetInvisibleIndexes(invisibleLineBuilder.Length);
                tags.Add(newTag);
                visibleLineBuilder.Append(newTag.VisibleTag);
                invisibleLineBuilder.Append(newTag.InvisibleTag);
                return index;
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

        protected void RemoveInvalidTags() {
            for (int index = tags.Count - 1; index >= 0; index--) {
                if (tags[index].MustClose && tags[index].ClosingTagIndex < 0) tags.RemoveAt(index);
            }
        }

        public void CopyFrom(DialogueLine otherLine) {
            speaker = otherLine.speaker;
            text = $"{otherLine.text}";
        }

        public virtual string Title => speaker == null ? null : speaker.DialogueName;
        public virtual Color Color => speaker == null ? Color.white : speaker.DialogueColor;
        public virtual Sprite Portrait => speaker == null ? null : speaker.DialoguePortrait;
        public virtual string Text => text;
        public string VisibleString { get; protected set; }
        public string InvisibleString { get; protected set; }

        public virtual object Clone() {
            return new DialogueLine(this);
        }
    }
}