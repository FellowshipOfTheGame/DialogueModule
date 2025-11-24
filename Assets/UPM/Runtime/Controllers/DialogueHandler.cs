using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Fog.Dialogue {
    public class DialogueHandler : MonoBehaviour {
        public delegate void DialogueAction();

        public static DialogueHandler Instance { get; protected set; } = null;
        public static bool debugActivated = false;

        [SerializeField] protected DialogueBox box;

        [Tooltip("Game object that handles choosing dialogue options")]
        [SerializeField]
        protected OptionHandler optionHandler;

        [Space(10)]
        [Header("Input")]
        [SerializeField] protected InputActionReference submitAction;
        [Tooltip("Input to debug and skip all dialogue lines, this doesn't trigger tag callbacks")]
        [SerializeField] protected InputActionReference cancelAction;

        [Header("Settings")]
        [Tooltip(
            "Whether or not, after filling in the entire text, the dialogue skips to the next line automatically.")]
        public bool autoSkip;
        [HideInInspectorIfNot(nameof(autoSkip))]
        public float timeUntilSkip;

        [Tooltip("Whether or not to pause game during dialogue")]
        public bool pauseDuringDialogue;

        [Tooltip(
            "Advanced setting: If there is only 1 handler/dialogue box (A visual novel for example) you can make this a singleton and call it from DialogueHandler.instance. If unsure, leave it false.")]
        public bool isSingleton;

        public event DialogueAction OnDialogueStart;
        public event DialogueAction OnDialogueEnd;

        protected IDialogueOption[] currentOptions;
        protected IDialogue currentDialogue;
        protected readonly Queue<DialogueLine> linesQueue = new();
        protected WaitForSecondsRealtime skipTimeCoroutine = null;
        protected Coroutine autoSkipCoroutine = null;

        #region Singleton
        protected void Awake() {
            if (!isSingleton) {
                Init();
                return;
            }

            if (!Instance)
                Instance = this;
            else if (Instance != this) {
                Debug.LogWarning($"Singleton {Instance.name} is still active, destroying new object {name}");
                Destroy(this);
                return;
            }
            Init();
        }

        protected void Init() {
            if (debugActivated) cancelAction.action.performed += DebugSkipAllLines;
            if (autoSkip) skipTimeCoroutine = new WaitForSecondsRealtime(timeUntilSkip);
        }

        protected void OnDestroy() {
            if (isSingleton && Instance == this) Instance = null;
            if (debugActivated) cancelAction.action.performed -= DebugSkipAllLines;
        }
        #endregion

        protected void EnableNextLineInput() {
            submitAction.action.performed -= SkipToCurrentLineEnd;
            submitAction.action.performed += FinishCurrentLine;
        }

        protected void SkipToCurrentLineEnd(InputAction.CallbackContext obj) {
            FillCurrentLine();
        }

        protected void FinishCurrentLine(InputAction.CallbackContext obj) {
            DisableSubmitKeyInputs();
            ShowNextLine();
        }

        protected void DisableSubmitKeyInputs() {
            submitAction.action.performed -= FinishCurrentLine;
            submitAction.action.performed -= SkipToCurrentLineEnd;
        }

        protected void EnableSkipLineInput() {
            submitAction.action.performed -= FinishCurrentLine;
            submitAction.action.performed += SkipToCurrentLineEnd;
        }

        protected void DebugSkipAllLines(InputAction.CallbackContext obj) {
            linesQueue.Clear();
            FinishDialogue();
        }

        public void StartDialogue(IDialogue newDialogue) {
            InterruptDialogue();
            if (newDialogue == null) return;

            currentDialogue = newDialogue;
            OnDialogueStart?.Invoke();
            newDialogue.BeforeDialogue();
            PauseGameIfNeeded();
            EnqueueDialogueLines(newDialogue);
            ShowNextLine();
        }

        public void InterruptDialogue() {
            DisableSubmitKeyInputs();
            ResetDialogueInfo();
            UnpauseGameIfNeeded();
        }

        protected void ResetDialogueInfo() {
            if (optionHandler) optionHandler.Deactivate();
            box.Hide();
            currentDialogue = null;
            currentOptions = null;
            linesQueue.Clear();
            if (autoSkipCoroutine == null) return;

            StopCoroutine(autoSkipCoroutine);
            autoSkipCoroutine = null;
        }

        protected virtual void PauseGameIfNeeded() {
            if (pauseDuringDialogue) Time.timeScale = 0f;
        }

        protected virtual void UnpauseGameIfNeeded() {
            if (pauseDuringDialogue) Time.timeScale = 1f;
        }

        protected void EnqueueDialogueLines(IDialogue dialogue) {
            linesQueue.Clear();
            foreach (DialogueLine line in dialogue.Lines) linesQueue.Enqueue(line);
        }

        protected void ShowNextLine() {
            if (linesQueue.Count <= 0) {
                FinishDialogue();
                return;
            }

            if (autoSkip) {
                box.ListenToLineEnd(AutoSkip);
            } else {
                box.ListenToLineEnd(OnLineEnd);
            }
            EnableSkipLineInput();
            box.StartTypingLine(linesQueue.Dequeue());
        }

        public void FinishDialogue() {
            IDialogue endedDialogue = currentDialogue;
            InterruptDialogue();
            OnDialogueEnd?.Invoke();
            endedDialogue?.AfterDialogue();
        }

        protected void OnLineEnd() {
            box.RemoveLineEndCallback(OnLineEnd);
            EnableNextLineInput();
        }

        protected void AutoSkip() {
            box.RemoveLineEndCallback(AutoSkip);
            DisableSubmitKeyInputs();
            autoSkipCoroutine = StartCoroutine(AutoSkipCoroutine());
        }

        private IEnumerator AutoSkipCoroutine() {
            if (autoSkip) yield return skipTimeCoroutine;

            ShowNextLine();
        }

        public void DisplayOptions(DialogueLine questionLine, IDialogueOption[] options) {
            InterruptDialogue();
            OnDialogueStart?.Invoke();
            PauseGameIfNeeded();
            ShowQuestion(questionLine, options);
        }

        protected void ShowQuestion(DialogueLine questionLine, IDialogueOption[] options) {
            DisableSubmitKeyInputs();
            currentOptions = options;
            box.ListenToLineEnd(ShowQuestionOptions);
            box.StartTypingLine(questionLine);
        }

        protected void ShowQuestionOptions() {
            box.RemoveLineEndCallback(ShowQuestionOptions);
            optionHandler.CreateOptions(currentOptions);
            currentOptions = null;
        }

        public void FillCurrentLine() {
            box.SkipToTheEnd();
        }
    }
}