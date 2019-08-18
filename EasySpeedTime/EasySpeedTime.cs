using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Locations;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasySpeedTime
{

/// <summary>The mod entry point.</summary>
    public class EasySpeedTime : Mod
    {
        /*********
        ** Fields
        *********/

        /// <summary>The mod configuration.</summary>
        internal static IModHelper ModHelper;
        internal static ModConfig Config;
        internal static IReflectionHelper Reflection;

        private static SButton JumpKey;
        private static SButton PauseKey;

        /// <summary>time stopped by idling or pause key.</summary>
        private static bool TimeStopped = false;

        /// <summary>time stopped by location.</summary>
        private static bool FrozenPlace = false;

        /// <summary>Last time player did action in seconds</summary>
        private static int LastTime = 0;

        /// <summary>Old Game1.GameTimeInterval of last tick</summary>
        private static int OldInterval;

        /// <summary>Translated texts</summary>
        private static Dictionary<string, string> Trans;
        /// private static string tt_idle, tt_stop, tt_resume;

        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            ModHelper = helper;
            Reflection = helper.Reflection;

            Enum.TryParse(Config.JumpKey, true, out JumpKey);
            Enum.TryParse(Config.PauseKey, true, out PauseKey);

            if (Config.MoveSpeedUp < 0 || Config.MoveSpeedUp > 8)
                Config.MoveSpeedUp = 4;

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

            if (!Config.DisableAutoGate)
            {
                helper.Events.GameLoop.SaveLoaded += (s, e) => { AutoGate.UpdateGateList();};
                helper.Events.Player.Warped += (s, e) => { AutoGate.UpdateGateList(); };
                helper.Events.World.ObjectListChanged += (s, e) => { AutoGate.UpdateGateList(); };
            }

            if (!Config.DisableRunningClock || !Config.DisableTimeFreeze)
                helper.Events.Display.RenderedHud += OnRenderedHud;

            if (!Config.DisableTimeFreeze)
            {
                helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;

                // read translations 
                Dictionary<string, Dictionary<string, string>> dicts = helper.Data.ReadJsonFile
                    <Dictionary<string, Dictionary<string, string>>>("Translation.json");

                if (!dicts.TryGetValue( helper.Translation.Locale, out Trans))
                    Trans = dicts["default"];
            }
        }

        /*********
        ** Private methods
        *********/

        private static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            if (Context.CanPlayerMove &&e.Button == JumpKey)
                JumpSwim.StartJump();

            if (Config.DisableTimeFreeze)
                return;

            LastTime = Game1.ticks / 60;

            if (e.Button == PauseKey && !FrozenPlace)
            {
                TimeStopped = !TimeStopped; // toggle
                Message.OnScreen((TimeStopped ? Trans["pause"] : Trans["resume"]), 100, 100);
            }
            else
                TimeStopped = false;
        }

        /// <summary>Receives an update tick.</summary>
        private static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!Config.DisableTimeFreeze)
            {
                if (TimeStopped || FrozenPlace)
                    Game1.gameTimeInterval = OldInterval;
                else
                    OldInterval = Game1.gameTimeInterval;
            }

            if (!Context.IsPlayerFree)
                return;

            if (e.IsMultipleOf(6) && !Config.DisableAutoGate )
                AutoGate.CheckGates();

            if (JumpSwim.Jumping && JumpKey != SButton.None)
                JumpSwim.JumpUpdateTicked();

            Farmer player = Game1.player;
            // apply various buff conditions
            int speedAdd = CalcSpeedBuff();

            if (! Context.CanPlayerMove || !player.isMoving() || player.swimming.Value || player.controller != null)
            {
                player.addedSpeed = 0;
            }
            else if (Game1.player.mount != null)
            {   // TODO: need work
                Game1.player.mount.addedSpeed = speedAdd + Config.HorseSpeedUp;
            }
            else
            {
                speedAdd += Config.MoveSpeedUp;
                // Normal speed: walking = 2, running = 5.
                if (!player.running)
                    speedAdd = (speedAdd == 1) ? 0 : (speedAdd + 1) / 2;
                player.addedSpeed = speedAdd;
            }
        }

        private static void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            if (Game1.player.swimming.Value)
                JumpSwim.CheckForSwimSuit();
            if ( Context.IsMultiplayer)
                return;

            int now = Game1.ticks/60;
            FrozenPlace = FreezingLocation();

            if (!FrozenPlace && (Game1.player.isMoving() || Game1.player.UsingTool))
            {
                LastTime = now;
                TimeStopped = false;
            }
            else if (Config.IdleTime > 0 && now > (LastTime + Config.IdleTime))
                TimeStopped = true;

            if (Config.PauseTime > 0 && (now == (LastTime + Config.PauseTime)) && Context.IsPlayerFree)
                Game1.pauseThenMessage(200, Trans["idlelong"], false);

            if ((TimeStopped || FrozenPlace) && Context.IsPlayerFree)
            {
                // Rectangle canvas = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea;
                // Vector2 pos = new Vector2(canvas.Right -105, canvas.Bottom - 380);

                Message.Boxed(Trans["stop"], 50, 50);
            }
        }

        private static void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if(!Config.DisableRunningClock)
                RunningClock.Draw(e.SpriteBatch);

            Message.Draw(e.SpriteBatch);
        }

        /// <summary>Should time be frozen for player in this place?</summary>
        private static bool FreezingLocation()
        {
            GameLocation map = Game1.currentLocation;

            if (map is MineShaft || map.Name.StartsWith("UndergroundMine"))
                return Config.FreezeInMineCave;

            if (map.IsOutdoors)
                return (Game1.player.swimming.Value && Config.FreezeOnSwimming);

            if (!Config.FreezeInDoor)
                return false;

            // To freeze while swimming in bathhouse, both FreezeInDoor AND FreezeOnSwimming.
            return (Config.FreezeOnSwimming || !Game1.player.swimming.Value 
                || !(map is BathHousePool) );
        }

        private static int CalcSpeedBuff(int speedup = 0)
        {
            Farmer player = Game1.player;
            GameLocation map = Game1.currentLocation;

            if (map.IsOutdoors && !Game1.isFestival())
            {
                // Stormy bad weather
                if (Game1.isLightning || Game1.isDebrisWeather && Game1.IsFall)
                    speedup--;
                // Good weather and good stamina
                else if (!Game1.isRaining && !Game1.isSnowing && (player.Stamina / player.MaxStamina >= 0.5f))
                    speedup++;
            }
            // over-encumbered or late night
            if (player.isInventoryFull() || Game1.timeOfDay >= 2400)
                speedup--;

            if (map.terrainFeatures.TryGetValue(player.getTileLocation(),
                    out StardewValley.TerrainFeatures.TerrainFeature tile) && tile != null)
            {
                // { hoe_dirt = 0,  wood = 1, stone, ghost, iceTile, straw = 5, gravel, 
                //  boardwalk = 7, colored_cobblestone, cobblestone, steppingStone = 10, }
                //  See StardewValley.TerrainFeatures.Flooring class defintion
                //  flooring.whichFloor.Value + 1 is used as index. 0 is for hoe_dirt. 
                int pave = -1;
                if (tile is StardewValley.TerrainFeatures.Flooring flooring)
                    pave = flooring.whichFloor.Value + 1;
                else if (tile is StardewValley.TerrainFeatures.HoeDirt )
                    pave = 0;

                if (pave >= 0 && pave < Config.RoadBuff.Length)
                    speedup += Config.RoadBuff[pave];   // speed up on roads
            }

            // clamp buff or debuff limit
            if (speedup > 4) speedup = 4;
            if (speedup < -3) speedup = -3;
            return speedup;
        }

    }
}

