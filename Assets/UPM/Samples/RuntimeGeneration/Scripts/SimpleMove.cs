using UnityEngine;
using UnityEngine.InputSystem;

namespace Fog.Dialogue.Samples.RuntimeGeneration {
    [RequireComponent(typeof(Rigidbody2D))]
    public class SimpleMove : MonoBehaviour {
        public static SimpleMove instance = null;
        [SerializeField] [Range(0, 100)] private float moveSpeed;
        public bool canMove = true;
        [SerializeField] private InputActionReference movementAction;
        private Rigidbody2D rigid;

        private void Awake() {
            if (instance) {
                Destroy(this);
                return;
            }

            instance = this;
            rigid = GetComponent<Rigidbody2D>();
        }

        private void Update() {
            if (canMove) {
                Vector2 speed = movementAction.action.ReadValue<Vector2>();
                speed *= moveSpeed;
                rigid.linearVelocity = speed;
            } else
                rigid.linearVelocity = Vector2.zero;
        }

        private void OnDestroy() {
            if (instance == this) instance = null;
        }

        public void BlockMovement() {
            canMove = false;
            rigid.linearVelocity = Vector2.zero;
        }

        public void AllowMovement() {
            canMove = true;
        }
    }
}