using UnityEngine;

namespace Fog.Dialogue.Samples.Example {
    [CreateAssetMenu(fileName = "NewOptionsDialogue",
                     menuName = "FoG/DialogueModule/Sample/Example/OptionsDialogueSample")]
    public class OptionsDialogueBlockMovement : OptionsDialogue {
        public override void BeforeDialogue() {
            SimpleMove.instance.BlockMovement();

            base.BeforeDialogue();
        }
    }
}