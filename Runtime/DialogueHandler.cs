using System.Text;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fog.Dialogue {
    public class DialogueHandler : MonoBehaviour {

        [Header("References")]
        [Tooltip("Reference to the TMPro text component of the main dialogue box.")]
        public TextMeshProUGUI dialogueText = null;
        [Tooltip("Whether or not the dialogue has a title or character name display.")]
        public bool useTitles = false;
        [Tooltip("Reference to the TMPro text component of the title/name display.")]
        [HideInInspectorIfNot(nameof(useTitles))] public TextMeshProUGUI titleText = null;
        [Tooltip("Whether or not the dialogue has a portrait.")]
        public bool usePortraits = false;
        [Tooltip("Reference to the Image component of the portrait to display.")]
        [HideInInspectorIfNot(nameof(usePortraits))] public Image portrait = null;
        [Tooltip("Current dialogue script to be displayed. To create a new dialogue, go to Assets->Create->Anathema->Dialogue.")]
        public Dialogue dialogue;
        [Tooltip("Game object that contains the chat box to be enabled/disabled")]
        public DialogueScrollPanel dialogueBox = null;
        [Tooltip("Game object that handles choosing dialogue options")]
        [SerializeField] private OptionHandler optionHandler = null;

        [Space(10)]

        [Header("Input")]
        [SerializeField] private InputActionReference directionsAction;
        [SerializeField] private InputActionReference submitAction;
        [SerializeField] private InputActionReference cancelAction;

        [Space(10)]

        [Header("Settings")]
        [Tooltip("Whether or not the characters are going to be displayed one at a time.")]
        public bool useTypingEffect = false;
        [HideInInspectorIfNot(nameof(useTypingEffect))][Range(1, 60)] public int framesBetweenCharacters = 0;
        [Tooltip("If true, trying to skip dialogue will first fill in the entire dialogue line and then skip if prompted again, if false it will skip right away.")]
        [HideInInspectorIfNot(nameof(useTypingEffect))] public bool fillInBeforeSkip = false;
        [Tooltip("Whether or not, after filling in the entire text, the dialogue skips to the next line automatically.")]
        public bool autoSkip = false;
        [HideInInspectorIfNot(nameof(autoSkip))] public float timeUntilSkip = 0;
        [Tooltip("Whether or not to pause game during dialogue")]
        public bool pauseDuringDialogue = false;
        [Tooltip("Advanced setting: If there is only 1 handler/dialogue box (A visual novel for example) you can make this a singleton and call it from DialogueHandler.instance. If unsure, leave it false.")]
        public bool isSingleton = false;
        private Queue<DialogueLine> dialogueLines = new Queue<DialogueLine>();
        private DialogueLine currentLine;
        private bool isLineDone;
        public bool IsActive { get; private set; } = false;
        private string currentTitle;
        private Color defaultPanelColor;
        private StringBuilder stringBuilder;
        public delegate void DialogueAction();
        public event DialogueAction OnDialogueStart;
        public event DialogueAction OnDialogueEnd;
        public static DialogueHandler instance;
        public static bool debugActivated = false;

        #region Singleton
        private void Awake() {
            if (!isSingleton)
                return;

            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Destroy(this);
            }
        }

        private void OnDestroy() {
            if (isSingleton && instance == this)
                instance = null;
        }
        #endregion

        private void Start() {
            Image panelImg = dialogueBox != null ? dialogueBox.GetComponent<Image>() : null;
            defaultPanelColor = panelImg ? panelImg.color : Color.white;
            stringBuilder = new StringBuilder();
        }

        private void Update() {
            if (!IsActive)
                return;

            if (isLineDone) {
                CheckScrollInput();
                CheckNextLineInput();
            } else {
                CheckSkipLineInput();
            }
            CheckSkipAllLinesIfDebug();
        }

        private void CheckScrollInput() {
            float axisValue = directionsAction.action.ReadValue<Vector2>().y;
            dialogueBox.Scroll(axisValue * Time.deltaTime);
        }

        private void CheckNextLineInput() {
            if (submitAction.action.triggered)
                StartCoroutine(NextLineCoroutine());
        }

        private void CheckSkipLineInput() {
            if (submitAction.action.triggered)
                Skip();
        }

        private void CheckSkipAllLinesIfDebug() {
            if (!debugActivated || !cancelAction.action.triggered)
                return;

            dialogueLines.Clear();
            EndDialogue();
        }

        public void StartDialogue(Dialogue dialogue) {
            OnDialogueStart += dialogue.BeforeDialogue;
            OnDialogueEnd += dialogue.AfterDialogue;
            this.dialogue = dialogue;
            StartDialogue();
        }

        public void StartDialogue() {
            EndActiveDialogue();
            OnDialogueStart?.Invoke();
            PauseGameIfNeeded();
            EnqueueDialogueLines();
            ShowDialogue();
        }

        private void EndActiveDialogue() {
            if (IsActive)
                EndDialogue();
        }

        private void PauseGameIfNeeded() {
            if (pauseDuringDialogue)
                Time.timeScale = 0f;
        }

        private void EnqueueDialogueLines() {
            foreach (DialogueLine line in dialogue.lines) {
                dialogueLines.Enqueue(line);
            }
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
            if (IsActive)
                EndDialogueWithoutCallback();
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
            if (useTypingEffect)
                yield return null;
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
            if (autoSkip) {
                yield return new WaitForSecondsRealtime(timeUntilSkip);
                StartCoroutine(NextLineCoroutine());
            }
        }

        private void UpdatePanelColor() {
            Image panelImg = dialogueBox.GetComponent<Image>();
            if (panelImg)
                panelImg.color = currentLine.Color;
        }

        private void UpdatePortrait() {
            portrait.sprite = null;
            Color transparent = Color.white;
            transparent.a = 0;

            if (!usePortraits || portrait == null)
                return;

            portrait.sprite = currentLine.Portrait;
            portrait.color = (portrait.sprite != null) ? Color.white : transparent;
            portrait.gameObject.SetActive(portrait.sprite != null);
        }

        private void UpdateTitle() {
            if (!useTitles || currentLine.Title == null)
                return;

            stringBuilder.Clear();
            if (titleText == dialogueText)
                stringBuilder.Append($"<size={dialogueText.fontSize + 3}>");

            stringBuilder.Append($"<b>{currentLine.Title}</b>");
            if (titleText == dialogueText) {
                stringBuilder.Append("</size>\n");
                titleText.text = stringBuilder.ToString();
                currentTitle = titleText.text;
            } else {
                titleText.text = stringBuilder.ToString();
            }
        }

        public void Skip() {
            if (!IsActive)
                return;

            StopAllCoroutines();
            if (fillInBeforeSkip && !isLineDone) {
                FillDialogueText();
                dialogueBox.JumpToEnd();
                isLineDone = true;
            } else {
                StartCoroutine(NextLineCoroutine());
            }
        }

        private IEnumerator FillInTextCoroutine() {
            if (useTypingEffect) {
                yield return TypeDialogueTextCoroutine();
            } else {
                FillDialogueText();
            }
        }

        private IEnumerator TypeDialogueTextCoroutine() {
            stringBuilder.Clear();
            stringBuilder.Append(dialogueText.text);
            foreach (char character in currentLine.Text) {
                stringBuilder.Append(character);
                dialogueText.text = stringBuilder.ToString();
                dialogueBox.ScrollToEnd();
                yield return WaitForFrames(framesBetweenCharacters);
            }
        }

        private void FillDialogueText() {
            stringBuilder.Clear();
            stringBuilder.Append((dialogueText == titleText) ? currentTitle : "");
            stringBuilder.Append(currentLine.Text);
            dialogueText.text = stringBuilder.ToString();
        }

        public void EndDialogue() {
            EndDialogueWithoutCallback();
            OnDialogueEnd?.Invoke();
        }

        public void EndDialogueWithoutCallback() {
            ResetAndDeactivateDialogueBox();
            UnpauseGameIfNeeded();
        }

        private void ResetAndDeactivateDialogueBox() {
            dialogueBox.gameObject.SetActive(false);
            dialogueText.text = "";
            titleText.text = "";
            if (portrait && portrait.sprite)
                portrait.sprite = null;
            Image panelImg = dialogueBox.GetComponent<Image>();
            if (panelImg)
                panelImg.color = defaultPanelColor;
            StopAllCoroutines();
            currentLine = null;
            IsActive = false;
        }

        private void UnpauseGameIfNeeded() {
            if (pauseDuringDialogue)
                Time.timeScale = 1f;
        }

        public static IEnumerator WaitForFrames(int frameCount) {
            while (frameCount > 0) {
                frameCount--;
                yield return null;
            }
        }

    }

}