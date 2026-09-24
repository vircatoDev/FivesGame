namespace Scripts.Components
{
    public enum BoardControl { Undo, Redo }

    public struct BoardControlEvent
    {
        public BoardControl Control;
    }
}
