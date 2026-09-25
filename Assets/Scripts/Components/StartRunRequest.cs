namespace Scripts.Components
{
    /// <summary>
    /// A run was paid for and its picture is loaded. Unlike events it stays in the world until BoardSetupSystem
    /// turns it into a board on the Playing state, or the run is abandoned with GameEndEvent.
    /// </summary>
    public struct StartRunRequest
    {
    }
}
