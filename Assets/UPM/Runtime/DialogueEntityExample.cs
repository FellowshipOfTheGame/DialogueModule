using UnityEngine;

namespace Fog.Dialogue {
    [CreateAssetMenu(fileName = "NewDialogueEntity", menuName = "FoG/DialogueModule/DialogueEntityExample")]
    public class DialogueEntityExample : DialogueEntity {
        [SerializeField] private Color dialogueColor = Color.white;
        public override Color DialogueColor => dialogueColor;

        [SerializeField] private string dialogueName = "";
        public override string DialogueName => dialogueName;

        [SerializeField] private Sprite dialoguePortrait = null;
        public override Sprite DialoguePortrait => dialoguePortrait;
    }
}
