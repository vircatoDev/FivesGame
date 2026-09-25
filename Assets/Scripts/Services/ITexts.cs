namespace Scripts.Services
{
    /// <summary>Localized text by key; arguments fill {0}, {1}... placeholders.</summary>
    public interface ITexts
    {
        string Get(string key, params object[] args);
    }
}
