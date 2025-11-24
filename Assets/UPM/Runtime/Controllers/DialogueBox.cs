using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Fog.Dialogue {
    public class DialogueBox : MonoBehaviour {
        [SerializeField] protected TextMeshProUGUI dialogueText;
        [SerializeField] protected GameObject rootObject;
        [SerializeField] protected DialogueScrollPanel scrollPanel;
        [SerializeField] protected bool showSpeaker;
        [HideInInspectorIfNot(nameof(showSpeaker))]
        [Tooltip("Can have same value as the dialogueText field, check ResetTextAndUpdateSpeaker method for details")]
        [SerializeField] protected TextMeshProUGUI speakerText;
        [SerializeField] protected bool showPortrait;
        [HideInInspectorIfNot(nameof(showPortrait))]
        [SerializeField] protected Image portrait;
        [SerializeField] protected bool useTypingEffect = true;
        [HideInInspectorIfNot(nameof(useTypingEffect))]
        [SerializeField, Range(0.1f, 30)] protected float charactersPerSecond;
        [SerializeField] protected UnityEvent onLineFinished = new();
        [SerializeField] protected UnityEvent<DialogueTextTag> onTagParsed = new();
        protected DialogueLineTypewriter typewriter;
        protected Color scrollPanelColor = Color.white;
        protected float elapsedTime = 0;
        protected bool paused = false;
        protected int typedCharCount = 0;

        protected virtual void Awake() {
            scrollPanelColor = scrollPanel.PanelColor;
            typewriter = new DialogueLineTypewriter(onTagParsed);
            onTagParsed.AddListener(TagProcessed);
        }

        protected virtual void TagProcessed(DialogueTextTag tag) { }

        public void ListenToParsedTags(UnityAction<DialogueTextTag> callback) {
            onTagParsed.AddListener(callback);
        }

        public void RemoveParsedTagsCallback(UnityAction<DialogueTextTag> callback) {
            onTagParsed.RemoveListener(callback);
        }

        public void ListenToLineEnd(UnityAction callback) {
            onLineFinished.AddListener(callback);
        }

        public void RemoveLineEndCallback(UnityAction callback) {
            onLineFinished.RemoveListener(callback);
        }

        public virtual void StartTypingLine(DialogueLine line) {
            rootObject.SetActive(true);
            ResetTextAndUpdateSpeaker(line.Speaker);
            typewriter.Reset(line, GetSpeakerPrefix());
            UpdateSpeakerPortrait(line.Speaker);
            UpdatePanelColor(line.Speaker);
            elapsedTime = 0;
            paused = false;
            scrollPanel.DeactivateScrollInput();
            if (!useTypingEffect) SkipToTheEnd();
        }

        protected virtual void ResetTextAndUpdateSpeaker(DialogueEntity speaker) {
            string title = speaker == null ? null : speaker.DialogueName;
            if (!showSpeaker || title == null) {
                if (speakerText) speakerText.text = string.Empty;
                dialogueText.text = string.Empty;
                typedCharCount = 0;
                return;
            }

            if (speakerText == dialogueText) {
                dialogueText.text = FormattedSpeakerTitle(title);
                typedCharCount = dialogueText.textInfo.characterCount;
            } else {
                speakerText.text = title;
                dialogueText.text = string.Empty;
                typedCharCount = 0;
            }
        }

        protected virtual string FormattedSpeakerTitle(string title) {
            return $"<size={dialogueText.fontSize + 3}><b>{title}</b></size>\n";
        }

        protected virtual string GetSpeakerPrefix() {
            return !showSpeaker || speakerText || dialogueText.textInfo.characterCount <= 0 ?
                null :
                dialogueText.text;
        }

        protected virtual void UpdateSpeakerPortrait(DialogueEntity speaker) {
            portrait.sprite = null;
            Color transparent = Color.white;
            transparent.a = 0;

            if (!showPortrait || !portrait) return;

            portrait.sprite = speaker == null ? null : speaker.DialoguePortrait;
            portrait.color = portrait.sprite ? Color.white : transparent;
            portrait.gameObject.SetActive(portrait.sprite);
        }

        protected virtual void UpdatePanelColor(DialogueEntity speaker) {
            scrollPanel.PanelColor = speaker ? speaker.DialogueColor : scrollPanelColor;
        }

        public virtual void SkipToTheEnd() {
            elapsedTime = 0;
            bool alreadyFinished = typewriter.ReachedTheEnd;
            typewriter.SkipToTheEnd();
            dialogueText.text = typewriter.GetOutputString();
            typedCharCount = dialogueText.textInfo.characterCount;
            ScrollToEndOfVisibleString();
            scrollPanel.ActivateScrollInput();
            if (!alreadyFinished) onLineFinished.Invoke();
        }

        protected virtual void ScrollToEndOfVisibleString() {
            int lineIndex = dialogueText.textInfo.characterInfo[typedCharCount - 1].lineNumber;
            TMP_LineInfo lineInfo = dialogueText.textInfo.lineInfo[lineIndex];
            scrollPanel.ScrollDownToFitBaseline(Mathf.Abs(lineInfo.descender - dialogueText.lineSpacing));
        }

        public virtual void RestartCurrentLine() {
            typewriter.ResetCurrentLine();
            dialogueText.text = typewriter.GetOutputString();
            typedCharCount = dialogueText.textInfo.characterCount;
            elapsedTime = 0;
            paused = false;
            scrollPanel.DeactivateScrollInput();
        }

        public virtual void PauseTyping() {
            paused = true;
        }

        public virtual void ResumeTyping() {
            paused = false;
        }

        public virtual void Hide() {
            rootObject.SetActive(false);
        }

        protected virtual void Update() {
            if (paused || typewriter.ReachedTheEnd) return;

            elapsedTime += Time.unscaledDeltaTime;
            int previousCount = typedCharCount;
            while (!typewriter.ReachedTheEnd && KeepTyping()) {
                TypeNextCharacter();
            }

            if (typedCharCount == previousCount) {
                if (!typewriter.ReachedTheEnd) return;

                scrollPanel.ActivateScrollInput();
                onLineFinished.Invoke();
                return;
            }

            ScrollToEndOfVisibleString();
            if (!typewriter.ReachedTheEnd) return;

            scrollPanel.ActivateScrollInput();
            onLineFinished.Invoke();
        }

        protected virtual bool KeepTyping() {
            return typedCharCount <= dialogueText.textInfo.characterCount && elapsedTime > 1.0f / charactersPerSecond;
        }

        protected virtual void TypeNextCharacter() {
            elapsedTime -= (1.0f / charactersPerSecond);
            typedCharCount += typewriter.AdvanceTypingIndex();
            dialogueText.text = typewriter.GetOutputString();
        }
    }
}