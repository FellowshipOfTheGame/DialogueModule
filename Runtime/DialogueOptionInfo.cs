using UnityEngine;

namespace Fog.Dialogue {
    [System.Serializable]
    public struct DialogueOptionInfo {
        [TextArea] public string text;
        public Dialogue nextDialogue;
    }
}