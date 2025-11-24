using UnityEngine;

namespace Fog.Dialogue {
    [CreateAssetMenu(fileName = "NewDialogueEntity", menuName = "FoG/DialogueModule/SimpleDialogueEntity")]
    public class SimpleDialogueEntity : DialogueEntity {
        [SerializeField] private Color dialogueColor = Color.white;

        [SerializeField] private string dialogueName = "";

        [SerializeField] private Sprite dialoguePortrait;
        public override Color DialogueColor => dialogueColor;
        public override string DialogueName => dialogueName;
        public override Sprite DialoguePortrait => dialoguePortrait;
    }
}