using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyPlay
{
    using ModMain = EasyPlay;

    internal class EasyHorse : Horse
    {
        private static EasyHorse MyHorse = null;    // singleton

        private static Horse BaseHorse = null;

        internal static void Setup()
        {
            Enum.TryParse(ModMain.Config.HorseWhistleKey, true, out HorseWhistleKey);

            if (!ModMain.Config.DisableEasyHorse)
            {
                ModMain.Events.GameLoop.SaveLoaded += (s, e) => CopyAndReplace();
                ModMain.Events.GameLoop.Saved += (s, e) => CopyAndReplace();
                ModMain.Events.GameLoop.Saving += (s, e) => Restore();
                ModMain.Events.Input.ButtonPressed += OnButtonPressed;
            }
        }

        private static void CopyHorse(Horse src, Horse dest)
        {
            dest.HorseId = src.HorseId;
            dest.Name = src.Name;
            dest.currentLocation = src.currentLocation;
            dest.Position = src.Position;
            dest.rider = src.rider;
            dest.hat.Value = src.hat.Value;
        }

        private static void CopyAndReplace()
        {
            if (BaseHorse == null)
            {
                // Get my stable
                Stable stable = Game1.getFarm().buildings.OfType<Stable>()
                    .Where(bld => bld.GetType().FullName?.Contains("TractorMod") == false
                    && (!Context.IsMultiplayer || bld.owner.Value == Game1.player.UniqueMultiplayerID))
                    .FirstOrDefault();

                if (stable == null)
                    return;

                BaseHorse = Utility.findHorse(stable.HorseId);
                if (BaseHorse == null)
                    return;
                if (BaseHorse.Name != Game1.player.horseName.Value)
                    Game1.player.horseName.Value = BaseHorse.Name;
            }

            if (MyHorse == null)
                MyHorse = new EasyHorse();

            CopyHorse(BaseHorse, MyHorse);

            var npcs = MyHorse.currentLocation.characters;

            for (int index = 0; index < npcs.Count; ++index)
            {
                if (npcs[index] == BaseHorse)
                {
                    npcs[index] = MyHorse;
                    return;
                }
            }
        }

        private static void Restore()
        {
            if (BaseHorse == null || MyHorse == null)
                return;

            var npcs = MyHorse.currentLocation.characters;

            for (int index = 0; index < npcs.Count; ++index)
            {
                if (npcs[index] == MyHorse)
                {
                    CopyHorse(MyHorse, BaseHorse);
                    npcs[index] = BaseHorse;
                    return;
                }
            }
        }

        public override Rectangle GetBoundingBox()
        {
            Rectangle boundingBox = base.GetBoundingBox();

            if (this.rider != null)
                boundingBox.Width = Game1.tileSize * 2 / 3;

            return boundingBox;
        }


        private static SButton HorseWhistleKey;

        private static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            Farmer player = Game1.player;
            Horse horse = MyHorse;
            if (e.Button != HorseWhistleKey || !Context.CanPlayerMove|| horse == null
                || horse.rider != null || String.IsNullOrWhiteSpace(player.horseName.Value))
                return;

            float distance = (player.currentLocation ==  horse.currentLocation)
               ? Vector2.Distance(player.getTileLocation(), horse.getTileLocation()): 999;
            bool ctlDown = Keyboard.GetState().IsKeyDown(Keys.LeftControl);

            if (ctlDown && distance < 2.0 )
            {
                if (horse.hat.Value == null)
                    return;
                // Remove hat of horse
                if (player.addItemToInventory(horse.hat.Value) != null)
                    Game1.createItemDebris(horse.hat.Value, horse.position, horse.facingDirection, null, -1);
                horse.hat.Value = null;
                Game1.playSound("dirtyHit");
            }
            else if (!ctlDown && distance > 10)
            {
                //  Summon player's horse here.
                Vector2 tile = Utility.recursiveFindOpenTileForCharacter(
                    player, player.currentLocation, player.getTileLocation(), 8);
                if (tile == Vector2.Zero)
                    Game1.player.getTileLocation();

                Game1.warpCharacter(MyHorse, Game1.currentLocation, tile);

                if (!ModMain.Config.DisableWhistleSound && Constants.TargetPlatform == GamePlatform.Windows)
                    PlayWhistle();
            }
        }

        // for Horse Whistle sound

        private static int LoadSound = 0;
        private static ISoundBank MySoundBank = null;
        private static WaveBank MyWaveBank = null;
        private static ISoundBank OrgSoundBank = Game1.soundBank;
        private static WaveBank OrgWaveBank = Game1.waveBank;

        private static void PlayWhistle()
        {
            if (LoadSound < 0)
                return;

            if (LoadSound == 0)
            {
                try
                {
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
}
