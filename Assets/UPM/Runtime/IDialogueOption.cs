namespace Fog.Dialogue {
    public interface IDialogueOption {
        public string Text { get; }

        public void Select();
    }
}