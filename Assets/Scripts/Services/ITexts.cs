using Cysharp.Threading.Tasks;

namespace Scripts.Services
{
    /// <summary>Localized text by key; arguments fill {0}, {1}... placeholders.</summary>
    public interface ITexts
    {
        /// <summary>Completes when <see cref="Get"/> can answer without loading: WebGL cannot load synchronously.</summary>
        UniTask Ready();

        string Get(string key, params object[] args);
    }
}
