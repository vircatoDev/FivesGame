namespace Scripts.Components
{
    // A changed layout needs projection; reset/replay switches also snap instead of animating.
    public struct BoardChangedEvent
    {
        public bool Snap;
    }
}
