using UnityEngine;

namespace Fog.Dialogue {
    [RequireComponent(typeof(Collider2D))]
    public class InteractableExample : MonoBehaviour, IInteractable {
        [SerializeField] private Dialogue dialogue = null;

        public void Reset() {
            Collider2D[] colliders = GetComponents<Collider2D>();
            int nColliders = colliders.Length;
            if (nColliders == 1) {
                colliders[0].isTrigger = true;
            } else if (nColliders > 0) {
                bool hasTrigger = HasAtLeastOneTrigger(colliders);
                if (!hasTrigger)
                    colliders[0].isTrigger = true;
            }
        }

        private bool HasAtLeastOneTrigger(Collider2D[] colliders) {
            foreach (Collider2D col in colliders) {
                if (col.isTrigger)
                    return true;
            }
            return false;
        }

        public void OnInteractAttempt() {
            if (dialogue != null)
                DialogueHandler.instance.StartDialogue(dialogue);
        }

        public void OnTriggerEnter2D(Collider2D col) {
            Agent agent = col.GetComponent<Agent>();
            if (agent)
                agent.collidingInteractables.Add(this);
        }

        public void OnTriggerExit2D(Collider2D col) {
            Agent agent = col.GetComponent<Agent>();
            if (agent)
                agent.collidingInteractables.Remove(this);
        }

    }
}
