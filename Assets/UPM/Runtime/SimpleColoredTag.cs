using System;
using System.Text;

namespace Fog.Dialogue {
    public class SimpleColoredTag : DialogueTextTag {
        private const string colorIndicator = "color=";
        private const string alphaIndicator = "alpha=";
        private const char colorStringIndicator = '\"';
        private const char colorCodeIndicator = '#';

        private readonly int length = 0;

        public static SimpleColoredTag CreateColoredTag(int startIndex, string tagName, string fullTag) {
            SimpleColoredTag newTag = new(startIndex, tagName, fullTag);
            return newTag;
        }

        public static SimpleColoredTag CreateSpriteTag(int startIndex, string tagName, string fullTag) {
            SimpleColoredTag newTag = new(startIndex, tagName, fullTag, 1);
            return newTag;
        }

        private SimpleColoredTag(int startIndex, string tagName, string fullTag, int typedLength = 0) :
            base(startIndex, tagName, fullTag) {
            length = typedLength;
            VisibleTag = $"{OpenTagChar}{fullTag}{CloseTagChar}";
            int colorStart = fullTag.IndexOf(colorIndicator, StringComparison.Ordinal);
            int alphaStart = fullTag.IndexOf(alphaIndicator, StringComparison.Ordinal);
            if (colorStart < 0 && alphaStart < 0) {
                InvisibleTag = $"{OpenTagChar}{fullTag} {colorIndicator}#00000000{CloseTagChar}";
                return;
            }

            if (colorStart >= 0) ProcessColorParameter(colorStart);
            if (alphaStart < 0) return;

            ProcessAlphaParameter(alphaStart, colorStart);
        }

        private void ProcessColorParameter(int colorStart) {
            StringBuilder invisibleBuilder = new(originalTag);
            int colorEnd = GetColorEnd(colorStart);
            if (colorEnd < 0) colorEnd = colorStart + colorIndicator.Length - 1;
            invisibleBuilder.Remove(colorStart, colorEnd - colorStart + 1);
            invisibleBuilder.Insert(colorStart, $"{colorIndicator}#00000000");
            invisibleBuilder.Insert(0, OpenTagChar);
            invisibleBuilder.Append(CloseTagChar);
            InvisibleTag = invisibleBuilder.ToString();
        }

        private int GetColorEnd(int colorStart) {
            int indicatorIndex = colorStart + colorIndicator.Length;
            if (indicatorIndex > originalTag.Length - 1 || (originalTag[indicatorIndex] != colorStringIndicator
                                                            && originalTag[indicatorIndex] != colorCodeIndicator))
                return -1;

            if (originalTag[indicatorIndex] == colorStringIndicator) {
                while (++indicatorIndex < originalTag.Length) {
                    if (originalTag[indicatorIndex] == colorStringIndicator) return indicatorIndex;
                }
            } else {
                while (++indicatorIndex < originalTag.Length) {
                    if (TagStopChars.Contains(originalTag[indicatorIndex])) return indicatorIndex - 1;
                }
                return originalTag.Length - 1;
            }
            return -1;
        }

        private void ProcessAlphaParameter(int alphaStart, int colorStart) {
            StringBuilder invisibleBuilder = new(colorStart < 0 ? originalTag : InvisibleTag);
            if (colorStart >= 0) alphaStart = InvisibleTag.IndexOf(alphaIndicator, StringComparison.Ordinal);
            int alphaEnd = GetAlphaEnd(alphaStart);
            if (alphaEnd < 0) alphaEnd = alphaStart + alphaIndicator.Length - 1;
            invisibleBuilder.Remove(alphaStart, alphaEnd - alphaStart + 1);
            invisibleBuilder.Insert(alphaStart, $"{alphaIndicator}#00");
            InvisibleTag = invisibleBuilder.ToString();
        }

        private int GetAlphaEnd(int alphaStart) {
            int indicatorIndex = alphaStart + alphaIndicator.Length;
            if (indicatorIndex > originalTag.Length - 1 || originalTag[indicatorIndex] != colorCodeIndicator) {
                return -1;
            }

            while (++indicatorIndex < originalTag.Length) {
                if (TagStopChars.Contains(originalTag[indicatorIndex])) return indicatorIndex - 1;
            }
            return originalTag.Length - 1;
        }

        public override string ClosingTag => $"{OpenTagChar}{ClosingTagIndicator}{tagName}{CloseTagChar}";
        public override int TypedLength => length;
        public override bool MustClose => false;
    }
}