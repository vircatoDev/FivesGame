using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fives.Configs;

namespace Fives.Services
{
    /// <summary>Themes' pictures on the content server, downloaded to the device.</summary>
    public interface IThemeDownloads
    {
        /// <summary>Downloads what is not on the device yet. Throws when that fails, for example without a network.</summary>
        UniTask Download(IReadOnlyList<ThemeConfig> themes, IProgress<float> progress, CancellationToken cancellation);
    }
}
