using UnityEngine;

namespace Fog.Dialogue.Samples.RuntimeGeneration {
    public class RandomDialogueController : MonoBehaviour {
        [SerializeField] private DialogueEntity speaker;
        [SerializeField] private RandomDialogueGenerator randomizer;
        private RandomizedDialogue randomDialogue;

        private void Start() {
            randomDialogue = new RandomizedDialogue(speaker, randomizer);
            randomDialogue.StartDialogue();
        }
    }
}