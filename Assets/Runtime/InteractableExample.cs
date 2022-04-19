using UnityEngine;

namespace Fog.Dialogue {
    [RequireComponent(typeof(Collider2D))]
    public class InteractableExample : MonoBehaviour, IInteractable {
        [SerializeField] private Dialogue dialogue = null;

        public void Reset() {
            int nColliders = GetComponents<Collider2D>().Length;
            // Se so tem um collider, se certifica que ele seja trigger
            if (nColliders == 1) {
                GetComponent<Collider2D>().isTrigger = true;
            } else if (nColliders > 0) {
                bool hasTrigger = HasAtLeastOneTrigger();
                // Se nenhum deles for, se certifica de que o primeiro deles seja trigger
                if (!hasTrigger) {
                    GetComponent<Collider2D>().isTrigger = true;
                }
            }
        }

        private bool HasAtLeastOneTrigger() {
            foreach (Collider2D col in GetComponents<Collider2D>()) {
                if(col.isTrigger) {
                    return true;
                }
            }
            return false;
        }

        public void OnInteractAttempt() {
            if (dialogue != null) {
                DialogueHandler.instance.StartDialogue(dialogue);
            }
        }

        public void OnTriggerEnter2D(Collider2D col) {
            Agent agent = col.GetComponent<Agent>();
            if (agent) {
                agent.collidingInteractables.Add(this);
            }
        }

        public void OnTriggerExit2D(Collider2D col) {
            Agent agent = col.GetComponent<Agent>();
            if (agent) {
                agent.collidingInteractables.Remove(this);
            }
        }

    }
}
