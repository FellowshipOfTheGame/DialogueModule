using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fog.Dialogue {
    public class DialogueHandler : MonoBehaviour {
        public delegate void DialogueAction();

        public static DialogueHandler instance;
        public static bool debugActivated = false;

        [Header("References")]
        [Tooltip("Reference to the TMPro text component of the main dialogue box.")]
        public TextMeshProUGUI dialogueText;

        [Tooltip("Whether or not the dialogue has a title or character name display.")]
        public bool useTitles;
        [Tooltip("Reference to the TMPro text component of the title/name display.")]
        [HideInInspectorIfNot(nameof(useTitles))]
        public TextMeshProUGUI titleText;

        [Tooltip("Whether or not the dialogue has a portrait.")]
        public bool usePortraits;
        [Tooltip("Reference to the Image component of the portrait to display.")]
        [HideInInspectorIfNot(nameof(usePortraits))]
        public Image portrait;

        [Tooltip(
            "Current dialogue script to be displayed. To create a new dialogue, go to Assets->Create->FoG->DialogueModule->Dialogue.")]
        public Dialogue dialogue;

        [Tooltip("Game object that contains the chat box to be enabled/disabled")]
        public DialogueScrollPanel dialogueBox;

        [Tooltip("Game object that handles choosing dialogue options")] [SerializeField]
        private OptionHandler optionHandler;

        [Space(10)]
        [Header("Input")]
        [SerializeField]
        private InputActionReference directionsAction;
        [SerializeField] private InputActionReference submitAction;
        [SerializeField] private InputActionReference cancelAction;

        [Space(10)]
        [Header("Settings")]
        [Tooltip("Whether or not the characters are going to be displayed one at a time.")]
        public bool useTypingEffect;
        [HideInInspectorIfNot(nameof(useTypingEffect))] [Range(1, 60)]
        public int framesBetweenCharacters;
        [Tooltip(
            "If true, trying to skip dialogue will first fill in the entire dialogue line and then skip if prompted again, if false it will skip right away.")]
        [HideInInspectorIfNot(nameof(useTypingEffect))]
        public bool fillInBeforeSkip;

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

        protected DialogueLine currentLine;
        protected DialogueLineTypewriter typewriter;
        protected string currentTitle = string.Empty;
        protected int titleLength = -1;
        protected Color defaultPanelColor;
        protected readonly Queue<DialogueLine> dialogueLines = new();
        protected bool isLineDone = true;
        public bool IsActive { get; protected set; }

        private void Start() {
            defaultPanelColor = dialogueBox.PanelColor;
        }

        private void Update() {
            if (!IsActive) return;

            if (isLineDone) {
                CheckScrollInput();
                CheckNextLineInput();
            } else
                CheckSkipLineInput();

            CheckSkipAllLinesIfDebug();
        }

        public event DialogueAction OnDialogueStart;
        public event DialogueAction OnDialogueEnd;

        private void CheckScrollInput() {
            float axisValue = directionsAction.action.ReadValue<Vector2>().y;
            dialogueBox.Scroll(axisValue * Time.deltaTime);
        }

        private void CheckNextLineInput() {
            if (submitAction.action.triggered) StartCoroutine(NextLineCoroutine());
        }

        private void CheckSkipLineInput() {
            if (submitAction.action.triggered) Skip();
        }

        private void CheckSkipAllLinesIfDebug() {
            if (!debugActivated || !cancelAction.action.triggered) return;

            dialogueLines.Clear();
            EndDialogue();
        }

        public void StartDialogue(Dialogue newDialogue) {
            EndActiveDialogue();
            dialogue = newDialogue;
            StartCurrentDialogue();
        }

        public void StartCurrentDialogue() {
            if (!dialogue) return;

            OnDialogueStart?.Invoke();
            dialogue.BeforeDialogue();
            PauseGameIfNeeded();
            EnqueueDialogueLines();
            ShowDialogue();
        }

        private void EndActiveDialogue() {
            if (IsActive) EndDialogue();
        }

        protected virtual void PauseGameIfNeeded() {
            if (pauseDuringDialogue) Time.timeScale = 0f;
        }

        private void EnqueueDialogueLines() {
            foreach (DialogueLine line in dialogue.lines) dialogueLines.Enqueue(line);
        }

        private void ShowDialogue() {
            StartCoroutine(ActivateInputCheck());
            dialogueBox.gameObject.SetActive(true);
            StartCoroutine(NextLineCoroutine());
        }

        public void DisplayOptions(DialogueLine questionLine, DialogueOptionInfo[] options) {
            EndActiveDialogueWithoutCallback();
            PauseGameIfNeeded();
            ShowQuestion(questionLine, options);
        }

        private void EndActiveDialogueWithoutCallback() {
            if (IsActive) EndDialogueWithoutCallback();
        }

        private void ShowQuestion(DialogueLine questionLine, DialogueOptionInfo[] options) {
            currentLine = questionLine;
            IsActive = false;
            isLineDone = false;
            dialogueBox.gameObject.SetActive(true);
            StartCoroutine(ShowQuestionCoroutine(options));
        }

        private IEnumerator ShowQuestionCoroutine(DialogueOptionInfo[] options) {
            yield return ShowLineSpeakerAndTextCoroutine();

            optionHandler.CreateOptions(options);
        }

        private IEnumerator NextLineCoroutine() {
            isLineDone = false;
            if (dialogueLines.Count <= 0) {
                EndDialogue();
                yield break;
            }

            currentLine = dialogueLines.Dequeue();
            yield return ShowLineSpeakerAndTextCoroutine();

            isLineDone = true;
            yield return AutoSkipCoroutine();
        }

        private IEnumerator ActivateInputCheck() {
            if (useTypingEffect) yield return null;

            IsActive = true;
        }

        private IEnumerator ShowLineSpeakerAndTextCoroutine() {
            UpdatePanelColor();
            UpdatePortrait();
            dialogueText.text = "";
            titleText.text = "";
            UpdateTitle();
            yield return FillInTextCoroutine();
        }

        private IEnumerator AutoSkipCoroutine() {
            if (!autoSkip) yield break;

            yield return new WaitForSecondsRealtime(timeUntilSkip);

            StartCoroutine(NextLineCoroutine());
        }

        protected virtual void UpdatePanelColor() {
            dialogueBox.PanelColor = currentLine.Color;
        }

        protected virtual void UpdatePortrait() {
            portrait.sprite = null;
            Color transparent = Color.white;
            transparent.a = 0;

            if (!usePortraits || !portrait) return;

            portrait.sprite = currentLine.Portrait;
            portrait.color = portrait.sprite ? Color.white : transparent;
            portrait.gameObject.SetActive(portrait.sprite);
        }

        protected virtual void UpdateTitle() {
            if (!useTitles || currentLine.Title == null) {
                titleLength = -1;
                return;
            }

            if (titleText == dialogueText) {
                currentTitle = $"<size={dialogueText.fontSize + 3}><b>{currentLine.Title}</b></size>\n";
                dialogueText.text = currentTitle;
                titleLength = dialogueText.textInfo.characterCount;
            } else {
                titleText.text = $"<b>{currentLine.Title}</b>";
                currentTitle = titleText.text;
                titleLength = titleText.textInfo.characterCount;
            }
        }

        public void Skip() {
            if (!IsActive) return;

            StopAllCoroutines();
            if (fillInBeforeSkip && !isLineDone) {
                typewriter.SkipToTheEnd();
                FillDialogueText();
                dialogueBox.JumpToEnd();
                isLineDone = true;
            } else
                StartCoroutine(NextLineCoroutine());
        }

        private IEnumerator FillInTextCoroutine() {
            if (typewriter != null)
                yield return TypeDialogueTextCoroutine();
            else
                FillDialogueText();
        }

        protected virtual IEnumerator TypeDialogueTextCoroutine() {
            int characterCount = 0;
            if (titleText == dialogueText) {
                characterCount = titleLength;
                typewriter.Reset(currentLine, currentTitle);
            } else {
                typewriter.Reset(currentLine);
            }
            dialogueText.text = typewriter.GetOutputString();
            dialogueBox.ScrollToEnd();
            while (!typewriter.ReachedTheEnd) {
                yield return WaitForFrames(framesBetweenCharacters);

                characterCount += typewriter.AdvanceTypingIndex();
                dialogueText.text = typewriter.GetOutputString();
                int lineIndex = dialogueText.textInfo.characterInfo[characterCount - 1].lineNumber;
                TMP_LineInfo lineInfo = dialogueText.textInfo.lineInfo[lineIndex];
                dialogueBox.ScrollDownToFitBaseline(Mathf.Abs(lineInfo.descender - dialogueText.lineSpacing));
            }
        }

        protected virtual void FillDialogueText() {
            dialogueText.text = $"{(dialogueText == titleText ? currentTitle : "")}{currentLine.VisibleString}";
        }

        public void EndDialogue() {
            EndDialogueWithoutCallback();
            OnDialogueEnd?.Invoke();
            if (dialogue) dialogue.AfterDialogue();
            dialogue = null;
        }

        public void EndDialogueWithoutCallback() {
            ResetAndDeactivateDialogueBox();
            UnpauseGameIfNeeded();
        }

        private void ResetAndDeactivateDialogueBox() {
            dialogueBox.gameObject.SetActive(false);
            dialogueText.text = "";
            titleText.text = "";
            if (portrait && portrait.sprite) portrait.sprite = null;
            dialogueBox.PanelColor = defaultPanelColor;
            StopAllCoroutines();
            currentLine = null;
            IsActive = false;
        }

        protected virtual void UnpauseGameIfNeeded() {
            if (pauseDuringDialogue) Time.timeScale = 1f;
        }

        public static IEnumerator WaitForFrames(int frameCount) {
            while (frameCount > 0) {
                frameCount--;
                yield return null;
            }
        }

        #region Singleton
        protected void Awake() {
            if (!isSingleton) {
                Init();
                return;
            }

            if (!instance)
                instance = this;
            else if (instance != this) {
                Debug.LogWarning($"Singleton {instance.name} is still active, destroying new object {name}");
                Destroy(this);
                return;
            }
            Init();
        }

        protected void Init() {
            typewriter = useTypingEffect ? new DialogueLineTypewriter() : null;
        }

        protected void OnDestroy() {
            if (isSingleton && instance == this) instance = null;
        }
        #endregion
    }
}