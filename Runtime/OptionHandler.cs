using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fog.Dialogue {
    [RequireComponent(typeof(AudioSource))]
    public class OptionHandler : MonoBehaviour {
        [SerializeField] private DialogueScrollPanel scrollPanel = null;
        [SerializeField] private RectTransform container = null;
        [SerializeField] private RectTransform optionList = null;
        [SerializeField] private GameObject optionPrefab = null;
        [SerializeField] private float inputCooldown = 0.1f;
        [SerializeField] private float activationTime = 0.5f;
        [SerializeField] private InputActionReference submitAction;
        [SerializeField] private InputActionReference directionsAction;
        private float timer;
        private bool IsTimerOver => timer > inputCooldown;
        private AudioSource audioSource;
        [SerializeField] private AudioClip changeOption;
        [SerializeField] private AudioClip selectOption;

        private int currentOptionIndex = -1;
        private DialogueOption CurrentOption => options[currentOptionIndex];
        private List<DialogueOption> options = new List<DialogueOption>();

        public bool IsActive { get; private set; }
        private bool SubmitButtonIsPressed => submitAction.action.phase == InputActionPhase.Started || submitAction.action.phase == InputActionPhase.Performed;

        private void Awake() {
            audioSource = GetComponent<AudioSource>();
            Deactivate();
            ValidatePrefab();
        }

        private void ValidatePrefab() {
            if (!optionPrefab) {
                Debug.Log("No prefab detected", gameObject);
                Destroy(this);
            } else {
                if (!optionPrefab.GetComponent<DialogueOption>()) {
                    Debug.Log("Prefab must have a DialogueOption component", gameObject);
                    Destroy(this);
                }
            }
        }

        public void CreateOptions(DialogueOptionInfo[] infos) {
            if (infos.Length > 0) {
                container.gameObject.SetActive(true);
                foreach (DialogueOptionInfo info in infos) {
                    CreateNewOption(info);
                }
                // This can be called from animation instead of coroutine, for better visual effect
                StartCoroutine(DelayedActivate(0.5f));
            } else {
                Debug.Log("Passed empty option array to Dialogue Handler");
                SelectOption();
            }
        }

        private void CreateNewOption(DialogueOptionInfo info) {
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
            RectTransform optionRect = CurrentOption.GetComponent<RectTransform>();
            float normalizedTop = scrollPanel.NormalizedTopPosition(optionRect);
            float normalizedBottom = scrollPanel.NormalizedBottomPosition(optionRect);

            if (scrollPanel.IsVerticalPositionLowerThan(normalizedTop) || scrollPanel.ViewportHeight <= optionRect.rect.height) {
                scrollPanel.ScrollToPosition(normalizedTop);
            } else if (scrollPanel.IsVerticalPositionHigherThan(normalizedBottom)) {
                scrollPanel.ScrollToPosition(normalizedBottom);
            }
        }

        private void SelectOption() {
            audioSource.PlayOneShot(selectOption);
            Deactivate();
            ResetTimer();
            Dialogue selectedDialogue = (currentOptionIndex >= 0) ? CurrentOption.NextDialogue : null;
            ClearOptionList();
            StartOrEndDialogue(selectedDialogue);
        }

        private static void StartOrEndDialogue(Dialogue selectedDialogue) {
            if (selectedDialogue) {
                DialogueHandler.instance.StartDialogue(selectedDialogue);
            } else {
                DialogueHandler.instance.EndDialogueWithoutCallback();
            }
        }

        private void ClearOptionList() {
            foreach (RectTransform transform in optionList) {
                Destroy(transform.gameObject);
            }
            options.Clear();
            currentOptionIndex = -1;
        }

        private void Update() {
            if (IsActive) {
                if (IsTimerOver) {
                    if (SubmitButtonIsPressed) {
                        CurrentOption.OnSelect?.Invoke();
                    } else {
                        CheckSelectionInput();
                    }
                    ResetTimer();
                }
                UpdateTimer();
            }
        }

        private void CheckSelectionInput() {
            float axisValue = directionsAction.action.ReadValue<Vector2>().y;
            float input = axisValue * (-1f);
            if (input != 0) {
                int newOptionIndex = Mathf.Clamp(currentOptionIndex + ((input > 0) ? 1 : -1), 0, options.Count - 1);
                FocusNewOptionIfNecessary(newOptionIndex);
                ShowHeaderIfNecessary(input);
            }
        }

        private void FocusNewOptionIfNecessary(int newOptionIndex) {
            if (newOptionIndex != currentOptionIndex) {
                CurrentOption.OnExit?.Invoke();
                currentOptionIndex = newOptionIndex;
                CurrentOption.OnFocus?.Invoke();
            }
        }

        private void ShowHeaderIfNecessary(float input) {
            if (input > 0 && currentOptionIndex == 0) {
                scrollPanel.ScrollToStart();
            }
        }

        private void UpdateTimer() {
            timer += Time.deltaTime;
        }

        private void ResetTimer() {
            timer = 0f;
        }
    }
}