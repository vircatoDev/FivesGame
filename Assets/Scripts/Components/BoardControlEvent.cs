namespace Scripts.Components
{
    public enum BoardControl { Undo, Replay, StopReplay }

    public struct BoardControlEvent
    {
        public BoardControl Control;
    }
}
