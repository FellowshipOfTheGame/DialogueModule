using System;
using UnityEngine;

namespace Fog.Dialogue {
    [Serializable]
    public struct DialogueOptionInfo {
        [TextArea] public string text;
        public Dialogue nextDialogue;
    }
}