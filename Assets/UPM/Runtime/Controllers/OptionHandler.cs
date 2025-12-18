using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fog.Dialogue {
    [RequireComponent(typeof(AudioSource))]
    public class OptionHandler : MonoBehaviour {
        [SerializeField] private DialogueScrollPanel scrollPanel;
        [SerializeField] private RectTransform container;
        [SerializeField] private RectTransform optionList;
        [SerializeField] private GameObject optionPrefab;
        [SerializeField] private float inputCooldown = 0.1f;
        [SerializeField] private float activationTime = 0.5f;
        [SerializeField] private InputActionReference submitAction;
        [SerializeField] private InputActionReference directionsAction;
        [SerializeField] private AudioClip changeOption;
        [SerializeField] private AudioClip selectOption;
        private AudioSource audioSource;

        private int currentOptionIndex = -1;
        private readonly List<DialogueOption> options = new();
        private readonly Queue<DialogueOption> disabledOptions = new();
        private float timer;
        private bool IsTimerOver => timer > inputCooldown;
        private DialogueOption CurrentOption =>
            currentOptionIndex >= 0 && currentOptionIndex < options.Count ?
                options[currentOptionIndex] :
                null;

        public bool IsActive { get; private set; }

        private void Awake() {
            audioSource = GetComponent<AudioSource>();
            Deactivate();
            ValidatePrefab();
        }

        public void Deactivate() {
            ResetTimer();
            RemoveInputCallbacks();
            IsActive = false;
            CurrentOption?.ResetFocus();
            ClearOptionList();
            inputCooldown = Mathf.Max(0f, inputCooldown);
            StopAllCoroutines();
            container.gameObject.SetActive(false);
        }

        private void ResetTimer() {
            timer = 0f;
        }

        private void RemoveInputCallbacks() {
            submitAction.action.performed -= OnSubmit;
            directionsAction.action.canceled -= OnSelectionChange;
            directionsAction.action.performed -= OnSelectionChange;
        }

        private void OnSubmit(InputAction.CallbackContext context) {
            if (!IsActive || !IsTimerOver) return;

            ResetTimer();
            CurrentOption.OnSelect?.Invoke();
        }

        private void OnSelectionChange(InputAction.CallbackContext context) {
            if (!IsActive || !IsTimerOver) return;

            float axisValue = context.ReadValue<Vector2>().y;
            if (axisValue == 0) return;

            float input = axisValue * -1f;
            int newOptionIndex = Mathf.Clamp(currentOptionIndex + (input > 0 ? 1 : -1), 0, options.Count - 1);
            if (newOptionIndex == currentOptionIndex) return;

            ResetTimer();
            ShowHeaderIfNecessary(input);
            FocusNewOption(newOptionIndex);
        }

        private void ShowHeaderIfNecessary(float input) {
            if (input > 0 && currentOptionIndex == 0) scrollPanel.ScrollToStart();
        }

        private void FocusNewOption(int newOptionIndex) {
            CurrentOption.OnExit?.Invoke();
            currentOptionIndex = newOptionIndex;
            if (changeOption) audioSource?.PlayOneShot(changeOption);
            CurrentOption.OnFocus?.Invoke();
        }

        private void ValidatePrefab() {
            if (!optionPrefab) {
                Debug.LogError("No prefab detected", gameObject);
            } else {
                if (optionPrefab.GetComponent<DialogueOption>()) return;

                Debug.LogError("Prefab must have a DialogueOption component", gameObject);
            }
            Destroy(this);
        }

        private void Update() {
            if (!IsActive) return;

            UpdateTimer();
        }

        private void UpdateTimer() {
            timer += Time.unscaledDeltaTime;
        }

        public void CreateOptions(IDialogueOption[] infos) {
            if (infos != null && infos.Length > 0) {
                container.gameObject.SetActive(true);
                UpdateOptionList(infos);
                TriggerHandlerActivation();
            } else {
                Debug.LogError("Passed empty option array to Dialogue Handler", this);
                Deactivate();
                ClearOptionList();
                DialogueHandler.Instance.InterruptDialogue();
            }
        }

        protected virtual void TriggerHandlerActivation() {
            // This can be called from animation instead of coroutine, for better visual effect
            if (activationTime >= 0)
                Activate();
            else
                StartCoroutine(DelayedActivate(activationTime));
        }

        private IEnumerator DelayedActivate(float delay) {
            yield return new WaitForSeconds(delay);

            Activate();
        }

        private void ClearOptionList() {
            for (int index = options.Count - 1; index >= 0; index--) {
                DisableOption(index);
            }
            currentOptionIndex = -1;
        }

        private void DisableOption(int index) {
            options[index].gameObject.SetActive(false);
            disabledOptions.Enqueue(options[index]);
            options.RemoveAt(index);
        }

        private void UpdateOptionList(IDialogueOption[] infos) {
            for (int index = 0; index < infos.Length; index++) {
                if (index < options.Count) {
                    options[index].Configure(infos[index]);
                    options[index].transform.SetAsLastSibling();
                    continue;
                }
                if (disabledOptions.Count > 0) {
                    ReuseOption(infos[index]);
                } else {
                    CreateNewOption(infos[index]);
                }
            }
            DisableUnusedOptions(infos);
        }

        private void ReuseOption(IDialogueOption info) {
            DialogueOption option = disabledOptions.Dequeue();
            option.Configure(info);
            option.gameObject.SetActive(true);
            option.transform.SetAsLastSibling();
            options.Add(option);
        }

        private void CreateNewOption(IDialogueOption info) {
            GameObject go = Instantiate(optionPrefab, optionList);
            DialogueOption newOption = go.GetComponentInChildren<DialogueOption>();
            newOption.Configure(info);
            newOption.OnSelect += SelectOption;
            newOption.OnFocus += FocusOption;
            newOption.transform.SetAsLastSibling();
            options.Add(newOption);
        }

        private void SelectOption() {
            if (selectOption) audioSource?.PlayOneShot(selectOption);
            IDialogueOption selectedOption = currentOptionIndex >= 0 ? CurrentOption.Option : null;
            Deactivate();
            if (selectedOption != null)
                selectedOption.Select();
            else
                DialogueHandler.Instance.InterruptDialogue();
        }

        private void FocusOption() {
            RectTransform optionRect = CurrentOption.RectTransform;
            float normalizedTop = scrollPanel.NormalizedTopPosition(optionRect);
            float normalizedBottom = scrollPanel.NormalizedBottomPosition(optionRect);

            if (scrollPanel.IsVerticalPositionLowerThan(normalizedTop) ||
                scrollPanel.ViewportHeight <= optionRect.rect.height)
                scrollPanel.ScrollToPosition(normalizedTop);
            else if (scrollPanel.IsVerticalPositionHigherThan(normalizedBottom))
                scrollPanel.ScrollToPosition(normalizedBottom);
        }

        private void DisableUnusedOptions(IDialogueOption[] infos) {
            for (int index = options.Count - 1; index >= infos.Length; index--) {
                DisableOption(index);
            }
        }

        public void Activate() {
            container.gameObject.SetActive(true);
            currentOptionIndex = 0;
            CurrentOption.OnFocus?.Invoke();
            if (!IsActive) {
                AddInputCallbacks();
                ResetTimer();
            }
            IsActive = true;
            inputCooldown = Mathf.Max(0f, inputCooldown);
        }

        private void AddInputCallbacks() {
            directionsAction.action.performed += OnSelectionChange;
            directionsAction.action.canceled += OnSelectionChange;
            submitAction.action.performed += OnSubmit;
        }
    }
}