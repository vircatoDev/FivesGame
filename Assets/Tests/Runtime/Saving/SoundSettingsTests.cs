using System.Collections.Generic;
using Fives.Models;
using Fives.Services;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Saving
{
    /// <summary>Громкость эффектов из настроек игрока.</summary>
    public sealed class SoundSettingsTests
    {
        /// <summary>
        /// Громкость эффекта умножается на настройку игрока:
        /// 0,8 × 0,25 = 0,2;
        /// при нулевой настройке звука нет.
        /// </summary>
        [Test]
        public void EffectVolume_CombinesTheSavedSettingWithTheEffectVolume_AndMuteSilences()
        {
            using var game = new GameStand();
            game.Config.AudioClipsCollection = new List<GameSoundCollection>();
            game.Save.GetPlayerData().SoundSettings.SoundEffectsVolume = 0.25f;
            var sound = new SoundService(game.Config, game.Save);

            Assert.That(sound.EffectVolume(0.8f), Is.EqualTo(0.2f).Within(1e-5f));

            sound.SetSoundEffectVolume(0);
            Assert.That(sound.EffectVolume(1f), Is.Zero);
        }
    }
}
