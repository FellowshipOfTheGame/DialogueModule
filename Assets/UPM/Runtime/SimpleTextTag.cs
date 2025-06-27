namespace Fog.Dialogue {
    public class SimpleTextTag : DialogueTextTag {
        public static SimpleTextTag CreateSimpleTag(int startIndex, string tagName, string fullTag) {
            SimpleTextTag newTag = new(startIndex, tagName, fullTag);
            return newTag;
        }

        private SimpleTextTag(int startIndex, string tagName, string fullTag) : base(startIndex, tagName, fullTag) {
            VisibleTag = $"{OpenTagChar}{fullTag}{CloseTagChar}";
            InvisibleTag = $"{OpenTagChar}{fullTag}{CloseTagChar}";
        }

        public override string ClosingTag => $"{OpenTagChar}{ClosingTagIndicator}{tagName}{CloseTagChar}";
        public override bool WaitForType => false;
        public override bool MustClose => false;
    }
}