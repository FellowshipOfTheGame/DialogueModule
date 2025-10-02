using UnityEngine;

namespace Fog.Dialogue.Samples.RuntimeGeneration {
    public class RandomDialogueController : MonoBehaviour {
        [SerializeField] private DialogueEntity speaker;
        private RandomizedDialogue randomDialogue;

        private void Start() {
            randomDialogue = new RandomizedDialogue(speaker);
            randomDialogue.StartDialogue();
        }
    }
}