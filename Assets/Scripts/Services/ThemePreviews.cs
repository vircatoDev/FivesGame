using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Scripts.Configs;
using UnityEngine;

namespace Scripts.Services
{
    /// <summary>Small theme pictures for the menus, loaded once on the loading screen and held for the whole session.</summary>
    public sealed class ThemePreviews
    {
        private readonly GlobalConfig _config;
        private readonly ISpriteLoader _sprites;
        private readonly Dictionary<ThemeConfig, Sprite> _previews = new Dictionary<ThemeConfig, Sprite>();

        public ThemePreviews(GlobalConfig config, ISpriteLoader sprites)
        {
            _config = config;
            _sprites = sprites;
        }

        public async UniTask Load()
        {
            var previews = await UniTask.WhenAll(_config.Themes.Select(theme => _sprites.Load(theme.Preview, this)));
            for (var i = 0; i < previews.Length; i++)
                _previews[_config.Themes[i]] = previews[i];
        }

        public Sprite Of(ThemeConfig theme) => _previews[theme];
    }
}
