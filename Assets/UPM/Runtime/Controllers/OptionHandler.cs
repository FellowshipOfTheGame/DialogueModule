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

        private bool SubmitButtonIsPressed =>
            submitAction.action.phase is InputActionPhase.Started or InputActionPhase.Performed;

        private void Awake() {
            audioSource = GetComponent<AudioSource>();
            Deactivate();
            ValidatePrefab();
        }

        private void Update() {
            if (!IsActive) return;

            if (IsTimerOver) {
                CheckInputs();
                ResetTimer();
            }

            UpdateTimer();
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

        public void CreateOptions(IDialogueOption[] infos) {
            if (infos.Length > 0) {
                container.gameObject.SetActive(true);
                UpdateOptionList(infos);
                // This can be called from animation instead of coroutine, for better visual effect
                StartCoroutine(DelayedActivate(activationTime));
            } else {
                Debug.LogError("Passed empty option array to Dialogue Handler", this);
                SelectOption();
            }
        }

        private void UpdateOptionList(IDialogueOption[] infos) {
            for (int index = 0; index < infos.Length; index++) {
                if (index < options.Count) {
                    options[index].Configure(infos[index]);
                    continue;
                }
                if (disabledOptions.Count > 0) {
                    DialogueOption option = disabledOptions.Dequeue();
                    option.Configure(infos[index]);
                    option.gameObject.SetActive(true);
                    options.Add(option);
                } else {
                    CreateNewOption(infos[index]);
                }
            }
            for (int index = options.Count - 1; index >= infos.Length; index--) {
                options[index].gameObject.SetActive(false);
                disabledOptions.Enqueue(options[index]);
                options.RemoveAt(index);
            }
        }

        private void CreateNewOption(IDialogueOption info) {
            GameObject go = Instantiate(optionPrefab, optionList);
            DialogueOption newOption = go.GetComponentInChildren<DialogueOption>();
            newOption.Configure(info);
            newOption.OnSelect += SelectOption;
            newOption.OnFocus += FocusOption;
            options.Add(newOption);
        }

        private IEnumerator DelayedActivate(float delay) {
            yield return new WaitForSeconds(delay);

            Activate();
        }

        public void Activate() {
            currentOptionIndex = 0;
            CurrentOption.OnFocus?.Invoke();
            IsActive = true;
            inputCooldown = Mathf.Max(0f, inputCooldown);
        }

        public void Deactivate() {
            IsActive = false;
            container.gameObject.SetActive(false);
            inputCooldown = Mathf.Max(0f, inputCooldown);
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

        private void SelectOption() {
            if (selectOption) audioSource.PlayOneShot(selectOption);
            Deactivate();
            ResetTimer();
            IDialogueOption selectedOption = currentOptionIndex >= 0 ? CurrentOption.Option : null;
            ClearOptionList();
            if (selectedOption != null)
                selectedOption.Select();
            else
                DialogueHandler.Instance.InterruptDialogue();
        }

        private void ClearOptionList() {
            foreach (RectTransform transform in optionList) Destroy(transform.gameObject);
            options.Clear();
            currentOptionIndex = -1;
        }

        private void CheckInputs() {
            if (SubmitButtonIsPressed)
                CurrentOption.OnSelect?.Invoke();
            else
                CheckSelectionInput();
        }

        private void CheckSelectionInput() {
            float axisValue = directionsAction.action.ReadValue<Vector2>().y;
            float input = axisValue * -1f;
            if (input == 0) return;

            int newOptionIndex = Mathf.Clamp(currentOptionIndex + (input > 0 ? 1 : -1), 0, options.Count - 1);
            FocusNewOptionIfNecessary(newOptionIndex);
            ShowHeaderIfNecessary(input);
        }

        private void FocusNewOptionIfNecessary(int newOptionIndex) {
            if (newOptionIndex == currentOptionIndex) return;

            CurrentOption.OnExit?.Invoke();
            currentOptionIndex = newOptionIndex;
            CurrentOption.OnFocus?.Invoke();
        }

        private void ShowHeaderIfNecessary(float input) {
            if (input > 0 && currentOptionIndex == 0) scrollPanel.ScrollToStart();
        }

        private void UpdateTimer() {
            timer += Time.deltaTime;
        }

        private void ResetTimer() {
            timer = 0f;
        }
    }
}