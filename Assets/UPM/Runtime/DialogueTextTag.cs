using System.Collections.Generic;

namespace Fog.Dialogue {
    public abstract class DialogueTextTag {
        public const char OpenTagChar = '<';
        public const char CloseTagChar = '>';
        public const char ClosingTagIndicator = '/';
        public static readonly HashSet<char> TagStopChars = new() { ' ', '\n', '\t', '=' };
        public delegate DialogueTextTag Constructor(int startIndex, string tagName, string fullTag);
        protected readonly string originalTag;
        public int StartIndex { get; protected set; }
        public int EndIndex { get; protected set; }
        public int InvisibleStartIndex { get; protected set; }
        public int InvisibleEndIndex { get; protected set; }
        public int ClosingTagIndex { get; protected set; }
        public string VisibleTag { get; protected set; }
        public string InvisibleTag { get; protected set; }
        public abstract string ClosingTag { get; }
        public abstract bool WaitForType { get; }
        public abstract bool MustClose { get; }
        public readonly string tagName;

        protected DialogueTextTag(int startIndex, string tagName, string fullTag) {
            StartIndex = startIndex;
            EndIndex = startIndex + fullTag.Length + 1;
            this.tagName = tagName;
            ClosingTagIndex = -1;
            InvisibleStartIndex = -1;
            InvisibleEndIndex = -1;
            originalTag = fullTag;
        }

        public void SetInvisibleIndexes(int invisibleStart) {
            InvisibleStartIndex = invisibleStart;
            InvisibleEndIndex = InvisibleStartIndex + InvisibleTag.Length;
        }

        public void SetClosingTagIndex(int closingIndex) {
            if (closingIndex < 0 || closingIndex <= EndIndex) return;

            ClosingTagIndex = closingIndex;
        }
    }
}