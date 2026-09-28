namespace Fives.Components
{
    public enum BoardControl { Undo, Hint }

    public struct BoardControlEvent
    {
        public BoardControl Control;
    }
}
