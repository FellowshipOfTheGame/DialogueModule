using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fog.Dialogue {
    [RequireComponent(typeof(Collider2D))]
    public class Agent : MonoBehaviour {
        #region Singleton
        public static Agent Instance { get; private set; } = null;
        public void Awake() {
            if (!Instance) {
                Instance = this;
            } else {
                Destroy(this);
            }

            nFramesCooldown = Mathf.Max(nFramesCooldown, 1);
        }
        public void OnDestroy() {
            if (Instance == this) {
                Instance = null;
            }
        }
        #endregion

        [SerializeField] private int maxInteractions = 1;
        [SerializeField] private int nFramesCooldown = 5;
        private int wait;
        private bool IsCooldownTimerOver => wait <= 0;
        [SerializeField, HideInInspector] private bool canInteract;
        public bool CanInteract => canInteract;
        private bool isProcessingInput;
        int interactedCount;
        [SerializeField] private InputActionReference interactAction;

        public List<IInteractable> collidingInteractables = new List<IInteractable>();

        private void Reset() {
            maxInteractions = 1;
            nFramesCooldown = 5;
        }

        private void Start() {
            canInteract = true;
            isProcessingInput = false;
            ResetInputCooldownTimer();
        }

        private void Update() {
            // Esse botao precisa ser declarado nos inputs do projeto
            if (interactAction.action.triggered && IsCooldownTimerOver) {
                ResetInputCooldownTimer();
                InteractIfPossible();
            }
            UpdateInputCooldownTimer();
        }

        private void UpdateInputCooldownTimer() {
            wait = (wait <= 0) ? 0 : (wait - 1);
        }

        private void ResetInputCooldownTimer() {
            wait = nFramesCooldown;
        }

        private void InteractIfPossible() {
            if (isProcessingInput || !canInteract) {
                return;
            }
            isProcessingInput = true;
            InteractWithAvailableInteractables();
            isProcessingInput = false;
        }

        private void InteractWithAvailableInteractables() {
            interactedCount = 0;
            foreach (IInteractable interactable in collidingInteractables.ToArray()) {
                if (interactedCount >= maxInteractions || !canInteract) {
                    break;
                }
                AttemptInteraction(interactable);
            }
            interactedCount = 0;
        }

        private void AttemptInteraction(IInteractable interactable) {
            if (interactable != null) {
                interactable.OnInteractAttempt();
                interactedCount++;
            } else {
                collidingInteractables.Remove(interactable);
            }
        }

        public void BlockInteractions(){
            canInteract = false;
        }

        public void AllowInteractions(){
            ResetInputCooldownTimer();
            canInteract = true;
        }
    }
}
