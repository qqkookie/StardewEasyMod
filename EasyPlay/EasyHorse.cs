using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

using StardewValley;
using StardewValley.Characters;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyPlay
{
    using ModMain = EasyPlay;

    internal static class EasyHorse
    {
        internal static SButton HorseWhistleKey;

        private static SlimHorse MyHorse;

        internal static void Setup()
        {
            Enum.TryParse(ModMain.Config.HorseWhistleKey, true, out HorseWhistleKey);

            if ( ModMain.Config.DisableEasyHorse)
            {
                ModMain.Events.GameLoop.SaveLoaded += (s, e) => MyHorse = new SlimHorse();
                ModMain.Events.GameLoop.Saved += (s, e) => MyHorse = new SlimHorse();
                ModMain.Events.GameLoop.Saving += (s, e) => MyHorse.Restore();
                ModMain.Events.Input.ButtonPressed += OnButtonPressed;
            }
        }

        private static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Context.CanPlayerMove && e.Button == HorseWhistleKey)
                RecallHorse();
        }

        /// <summary>Recall player's horse here.</summary>
        internal static void RecallHorse()
        {
            Horse horse = FindHorse();
            if (horse != null)
            {
                Game1.warpCharacter(horse, Game1.currentLocation, Game1.player.getTileLocation());

                if (!ModMain.Config.DisableWhistleSound && Constants.TargetPlatform == GamePlatform.Windows)
                    PlayWhistle();
            }
        }

        internal static Horse FindHorse()
        {
            // Get all stables in the game
            var stables = from location in Game1.locations.OfType<StardewValley.Locations.BuildableGameLocation>()
                          from stable in location.buildings.OfType<StardewValley.Buildings.Stable>()
                          where stable.GetType().FullName?.Contains("TractorMod") != true
                          select stable;
            if (stables.Count() == 0)
                return null;

            Horse horse = (from stable in stables
                 where !Context.IsMultiplayer || stable.owner.Value == Game1.player.UniqueMultiplayerID
                 select Utility.findHorse(stable.HorseId)).FirstOrDefault(h => h != null && h.rider == null);

            return horse;
        }

        private static int LoadSound = 0;
        private static ISoundBank MySoundBank = null;
        private static WaveBank MyWaveBank = null;
        private static ISoundBank OrgSoundBank = Game1.soundBank;
        private static WaveBank OrgWaveBank = Game1.waveBank;

        internal static void PlayWhistle()
        {
            if (LoadSound < 0 )
                return;

            if (LoadSound == 0)
            {
                try {
                    MySoundBank = new SoundBankWrapper(new SoundBank(Game1.audioEngine,
                        Path.Combine(ModMain.ModHelper.DirectoryPath, "Assets", "WhistleSoundBank.xsb")));
                    MyWaveBank = new WaveBank(Game1.audioEngine,
                        Path.Combine(ModMain.ModHelper.DirectoryPath, "Assets", "WhistleWaveBank.xwb"));

                    OrgSoundBank = Game1.soundBank;
                    OrgWaveBank = Game1.waveBank;
                    LoadSound = 1;
                }
                catch (ArgumentException ex)
                {
                    LoadSound = -1;
                }
            }

            try
            {
                Game1.soundBank = MySoundBank;
                Game1.waveBank = MyWaveBank;
                Game1.audioEngine.Update();
                Game1.playSound("horseWhistle");
            }
            finally
            {
                Game1.soundBank = OrgSoundBank;
                Game1.waveBank = OrgWaveBank;
                Game1.audioEngine.Update();
            }
        }
    }

    internal class SlimHorse : Horse
    {
        internal static Horse BaseHorse;

        public SlimHorse()
        {
            BaseHorse = EasyHorse.FindHorse();

            if (BaseHorse == null)
                return;

            var npcs = this.currentLocation.characters;

            for (int index = 0; index < npcs.Count; ++index)
            {
                if (npcs[index] == BaseHorse)
                {
                    npcs[index] = this;
                    return;
                }
            }
        }

        internal void Restore()
        {
            if (BaseHorse == null)
                return;

            var npcs = this.currentLocation.characters;

            for (int index = 0; index < npcs.Count; ++index)
            {
                if (npcs[index] == this)
                {
                    npcs[index] = BaseHorse;
                    return;
                }
            }
        }

        public override Rectangle GetBoundingBox()
        {
            Rectangle boundingBox = base.GetBoundingBox();

            if (!this.mounting.Value)
                return boundingBox;

            boundingBox.Inflate(-14 - Game1.pixelZoom, 0);
            return boundingBox;
        }
    }
}
