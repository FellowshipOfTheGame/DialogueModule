using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Fog.Dialogue {
    public interface IDialogue {
        public List<DialogueLine> Lines { get; }

        public void BeforeDialogue();

        public void AfterDialogue();

        public void StartDialogue();

        public static readonly ReadOnlyDictionary<string, DialogueTextTag.Constructor> TMProTagFactory =
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
                { "sprite", SimpleColoredTag.CreateSpriteTag }, { "style", SimpleTextTag.CreateSimpleTag },
                { "sub", SimpleTextTag.CreateSimpleTag }, { "sup", SimpleTextTag.CreateSimpleTag },
                { "u", SimpleTextTag.CreateSimpleTag }, { "uppercase", SimpleTextTag.CreateSimpleTag },
                { "voffset", SimpleTextTag.CreateSimpleTag }, { "width", SimpleTextTag.CreateSimpleTag },
            };
            return dict;
        }
    }
}