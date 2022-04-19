using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Fog.Dialogue {
    [RequireComponent(typeof(Mask)), RequireComponent(typeof(Image)), RequireComponent(typeof(RectTransform))]
    public class DialogueScrollPanel : ScrollRect {
        public bool smoothScrolling;
        public float scrollSpeed;
        [SerializeField] private GameObject scrollUpIndicator = null;
        [SerializeField] private GameObject scrollDownIndicator = null;
        [SerializeField] private GameObject skipIndicator = null;
        private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        private float ContentHeight => content.rect.height;
        public float ViewportHeight => viewport.rect.height;

        // To do: Change this so it also works with horizontal scrolling

        protected new void Reset() {
            smoothScrolling = false;
            scrollSpeed = 10f;
            content = null;
            horizontal = false;
            vertical = true;
            movementType = MovementType.Clamped;
            inertia = false;
            scrollSensitivity = 1;
            viewport = null;
            horizontalScrollbar = null;
            horizontalScrollbarSpacing = 1f;
            horizontalScrollbarVisibility = ScrollbarVisibility.AutoHideAndExpandViewport;
            verticalScrollbar = null;
            verticalScrollbarSpacing = 1f;
            verticalScrollbarVisibility = ScrollbarVisibility.AutoHideAndExpandViewport;
            onValueChanged = null;
        }

        public float NormalizedTopPosition(RectTransform rect) {
            float contentBottom = GetRectBottom(content);
            float rectTop = GetRectTop(rect);
            float distance = rectTop - contentBottom;

            return Mathf.Clamp((distance - ViewportHeight) / (ContentHeight - ViewportHeight), 0f, 1f);
        }

        public float NormalizedBottomPosition(RectTransform rect) {
            float contentBottom = GetRectBottom(content);
            float rectBottom = GetRectBottom(rect);
            float distance = rectBottom - contentBottom;

            return Mathf.Clamp(distance / (ContentHeight - ViewportHeight), 0f, 1f);
        }

        private float GetRectBottom(RectTransform rect){
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return corners[0].y;
        }

        private float GetRectTop(RectTransform rect){
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return corners[1].y;
        }

        public void Scroll(float axisInputValue) {
            RectTransform rectTransform = transform as RectTransform;
            float incrementValue = axisInputValue * scrollSpeed * (rectTransform.rect.height / ContentHeight);
            verticalNormalizedPosition = Mathf.Clamp(verticalNormalizedPosition + incrementValue, 0f, 1f);
        }

        public void JumpToEnd() {
            JumpToPosition(0f);
        }

        public void JumpToStart() {
            JumpToPosition(1f);
        }

        public void JumpToPosition(float targetNormalPosition) {
            if (smoothScrolling) {
                StopAllCoroutines();
            }
            Canvas.ForceUpdateCanvases();
            verticalNormalizedPosition = Mathf.Clamp(targetNormalPosition, 0f, 1f);
        }

        public void ScrollToEnd() {
            ScrollToPosition(0f);
        }

        public void ScrollToStart() {
            ScrollToPosition(1f);
        }

        public void ScrollToPosition(float targetNormalPosition) {
            targetNormalPosition = Mathf.Clamp(targetNormalPosition, 0f, 1f);
            if (smoothScrolling) {
                StopAllCoroutines();
                StartCoroutine(ScrollingToPosition(targetNormalPosition));
            } else {
                JumpToPosition(targetNormalPosition);
            }
        }

        private IEnumerator ScrollingToPosition(float targetNormalPosition) {
            if (IsVerticalPositionHigherThan(targetNormalPosition)) {
                yield return ScrollingDown(targetNormalPosition);
            } else if (IsVerticalPositionLowerThan(targetNormalPosition)) {
                yield return ScrollingUp(targetNormalPosition);
            }
        }

        private IEnumerator ScrollingUp(float targetPosition) {
            yield return waitForEndOfFrame;
            while (IsVerticalPositionLowerThan(targetPosition)) {
                verticalNormalizedPosition += (Time.deltaTime * scrollSpeed * 10) / ContentHeight;
                yield return waitForEndOfFrame;
            }
            verticalNormalizedPosition = 1f;
            velocity = Vector2.zero;
        }

        private IEnumerator ScrollingDown(float targetPosition) {
            yield return waitForEndOfFrame;
            while (IsVerticalPositionHigherThan(targetPosition)) {
                verticalNormalizedPosition -= (Time.deltaTime * scrollSpeed * 10) / ContentHeight;
                yield return waitForEndOfFrame;
            }
            verticalNormalizedPosition = 0f;
            velocity = Vector2.zero;
        }

        public bool IsVerticalPositionLowerThan(float value) {
            return verticalNormalizedPosition < (value - Mathf.Epsilon);
        }

        public bool IsVerticalPositionHigherThan(float value) {
            return verticalNormalizedPosition > (value + Mathf.Epsilon);
        }

        protected new void Start() {
            base.Start();
        }

        protected override void OnEnable() {
            base.OnEnable();
            if (skipIndicator != null) {
                skipIndicator.SetActive(true);
            }
            if (scrollUpIndicator != null) {
                scrollUpIndicator.SetActive(false);
            }
            if (scrollDownIndicator != null) {
                scrollDownIndicator.SetActive(false);
            }
        }

        protected override void OnDisable() {
            if (skipIndicator != null) {
                skipIndicator.SetActive(false);
            }
            if (scrollUpIndicator != null) {
                scrollUpIndicator.SetActive(false);
            }
            if (scrollDownIndicator != null) {
                scrollDownIndicator.SetActive(false);
            }
            base.OnDisable();
        }

        protected override void LateUpdate() {
            base.LateUpdate();
            if (scrollUpIndicator != null) {
                scrollUpIndicator.SetActive(IsVerticalPositionLowerThan(1.0f) && (ContentHeight - ViewportHeight > Mathf.Epsilon));
            }
            if (scrollDownIndicator != null) {
                scrollDownIndicator.SetActive(IsVerticalPositionHigherThan(0f) && (ContentHeight - ViewportHeight > Mathf.Epsilon));
            }
        }
    }
}
