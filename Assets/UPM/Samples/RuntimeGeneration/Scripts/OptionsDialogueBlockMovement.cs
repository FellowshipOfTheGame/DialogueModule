using UnityEngine;

namespace Fog.Dialogue.Samples.RuntimeGeneration {
    [CreateAssetMenu(fileName = "NewOptionsDialogue",
                     menuName = "FoG/DialogueModule/Sample/RuntimeGeneration/OptionsDialogueSample")]
    public class OptionsDialogueBlockMovement : OptionsDialogue {
        public override void BeforeDialogue() {
            SimpleMove.instance.BlockMovement();

            base.BeforeDialogue();
            DialogueHandler.instance.OnDialogueStart -= BeforeDialogue;
        }
    }
}