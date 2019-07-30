using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using xTile.Dimensions;

using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyFishing
{
    /// <summary>The main entry point.</summary>
    public class EasyFishing : Mod
    {
        /*********
        ** Fields
        *********/

        /// <summary>The mod configuration.</summary>
        internal static ModConfig Config;
        internal static IReflectionHelper Reflection;
        internal static ITranslationHelper Translation;
        internal static IMonitor Logger;

        private static SButton FishingPracticeKey;

        private static SBobberBar Bobber = null;
        private static bool FishingEnding = false;
        private static bool Practicing = false;
        private static int[] PracticeList;

        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            Reflection = helper.Reflection;
            Translation = helper.Translation;
            Logger = Monitor;

            Enum.TryParse(Config.FishingPracticeKey, true, out FishingPracticeKey);

            helper.Events.GameLoop.SaveLoaded +=
                (sender, e) => { Config= helper.ReadConfig<ModConfig>(); };

            if (!Config.DisableFishingSpotInfo)
                helper.Events.Display.RenderedHud += FishingSpotInfo.OnRenderedHud;

            if (!Config.DisableExtraInfo)
                ExtraInfo.SetupExtraInfo(helper);

            if (!Config.DisableCatchInfo || !Config.DisableExtraInfo)
                helper.Events.Display.RenderedActiveMenu += CatchInfo.OnRenderedActiveMenu;

            helper.Events.Display.RenderedWorld += OnRenderedWorld;

            // Build list of fish to practice fishing.
            string[] fishnames = Config.PracticeFishes.Select(s => s.ToLower()).ToArray();
            List<int> fishids = new List<int>();

            int[] exclude = { 152, 153, 157 }; // Algae, Seaweed
            foreach (var kv in Game1.content.Load<Dictionary<int, string>>("Data\\Fish"))
            {
                string[] data = kv.Value.ToLower().Split('/');
                string id = kv.Key.ToString();
                if (data[1] != "trap" && !exclude.Contains(kv.Key) && (fishnames.Length == 0 || fishnames.Contains(data[0]) 
                    || (data.Count() >= 14 && fishnames.Contains(data[13])) || fishnames.Contains(id)))
                {
                    // Monitor.Log($"{id} {data[0]}\t\t{data[13]}");
                    fishids.Add(kv.Key);
                }

            }
            PracticeList = fishids.ToArray();

            helper.Events.Input.ButtonPressed += OnButtonPressed;

            Config.EasyFish = Math.Min(Math.Max(Config.EasyFish, -90), 90);
            Config.SlowReel = Math.Min(Math.Max(Config.SlowReel, 0), 100);

            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;

        }

        /*********
        ** Event handlers
        *********/

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        private static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!(Game1.player.CurrentTool is FishingRod rod))
                return;

            if (rod.isTimingCast)
            {
                // Is there some water tile in my front?
                Farmer player = Game1.player;
                GameLocation map = Game1.currentLocation;                
                Point cp = player.getTileLocationPoint();
                Location cTile = new Location(cp.X, cp.Y);
                Location diruvec = FacingDirection();

                if ( map != null && map.waterTiles != null)
                {
                    foreach (int d in Enumerable.Range(5, 20))      // TODO: range adjust.
                    {
                        Location tile = cTile + diruvec * d;
                        if (!map.isTileOnMap(tile.X, tile.Y) || !map.isTilePassable(tile, Game1.viewport))
                            break;  // something blocking or out of map
                        if (map.waterTiles[tile.X, tile.Y])
                            return; // found water tile. OK to go.
                    }
                }

                // not water front, stop casting fishing rod and restore stamina.
                player.completelyStopAnimatingOrDoingAction();
                rod.isTimingCast = false;
                player.Stamina += 8.0f - player.FishingLevel * 0.1f;
                player.canReleaseTool = true;
                Game1.playSound("dwop");
                return;
            }

            if (Config.DisableFishingAdjust) 
                return;

            SBobberBar bobber = Bobber;

            if (rod.pullingOutOfWater
                || (bobber != null && bobber.fadeOut && bobber.scale <= 0.1))
            {
                FishingEnding = true;
            }

            else if (rod.isReeling)
            {
                // apply fishing minigame changes
                if (bobber == null)
                {
                    if (!(Game1.activeClickableMenu is BobberBar menu))
                        return;
                    Bobber = SBobberBar.ConstructFromBaseClass(menu);
                    bobber = Bobber;
                }

                // Begin fishing game
                if (bobber.Timer < 15)  // Delay 1/4 seconds
                    bobber.Timer++;

                else if (bobber.Timer < 20) // init
                {
                    // Do these things once per fishing minigame, 1/4 second after it updates
                    if (Config.InstantCatch)
                        bobber.distanceFromCatching = 1.0f;
                    else if (Config.ProgressStart / 100.0f > bobber.distanceFromCatching)
                        bobber.distanceFromCatching = Config.ProgressStart / 100.0f;

                    bobber.difficulty *= (100 - Config.EasyFish) / 100.0f;
                    bobber.perfect = Config.AlwaysPerfect;
                    bobber.treasure = Config.AlwaysTreasure;
                    bobber.treasureCaught = (bobber.treasure && Config.InstantCatch);

                    if (Config.CatchTreasure && bobber.treasure && bobber.distanceFromCatching > 0.9f)
                        bobber.treasureCaught = true;

                    bobber.PosMax = bobber.bobberBarPos; // initial positon

                    bobber.bobberBarHeight = (int)(bobber.bobberBarHeight * (1.0f + Config.ExpandBar / 100.0f));

                    bobber.OldReelSpeed = 0;
                    bobber.Timer = 20;
                }
                else  // real update
                {
                    const float PrograssRate = 0.003f;

                    if (Config.ProgressBoost > 100)
                    {
                        if (bobber.bobberInBar)
                            bobber.distanceFromCatching += PrograssRate * (Config.ProgressBoost - 100) / 100.0f;
                        else
                            bobber.distanceFromCatching += PrograssRate;
                    }
                    else if (!bobber.bobberInBar)
                        bobber.distanceFromCatching += PrograssRate * Config.ProgressBoost / 100.0f;

                    if (bobber.distanceFromCatching > 1.0f)
                        bobber.distanceFromCatching = 1.0f;

                    // Reduce bouncing speed at top and bottom.
                    if (Config.SlowReel > 0 && Math.Abs(bobber.bobberBarSpeed) > 3.0f)
                    {
                        if (bobber.bobberBarPos < 0.1f)
                            bobber.bobberBarSpeed *= 0.3f;
                        else if (bobber.bobberBarPos > bobber.PosMax - 0.1f)
                            bobber.bobberBarSpeed *= (float)(0.99 + Math.Log10(0.1 + Config.SlowReel / 100.0) / 10);
                    }

                    // Reduce acceleration and max speed of bobberbar
                    const float MAXSPEED = 14.0f;
                    float reel = (100 - Config.SlowReel) / 100.0f;
                    float speed = bobber.OldReelSpeed + (bobber.bobberBarSpeed - bobber.OldReelSpeed) * reel;

                    bobber.OldReelSpeed = bobber.bobberBarSpeed;
                    bobber.bobberBarSpeed = Math.Min(Math.Max(speed, -MAXSPEED * reel), MAXSPEED * reel);
                }
            }
            // When fish nibbles, pull out the fishing rod automatically.
            else if (rod.isNibbling && !rod.hit && e.IsMultipleOf(10) && Config.AutoHitRod
                && Context.IsPlayerFree && Game1.activeClickableMenu == null
                && Reflection.GetField<int>(rod, "whichFish").GetValue() == -1)
            {
                rod.DoFunction(Game1.player.currentLocation, 1, 1, 1, Game1.player);
            }
            // cut down waiting time until fish bites
            else if (rod.isFishing && !rod.isNibbling && Config.QuickBite > 0
                && (rod.fishingBiteAccumulator > rod.timeUntilFishingBite * (1.0f - Config.QuickBite / 100.0f)))
            {
                rod.fishingBiteAccumulator = rod.timeUntilFishingBite;
            }
        }

        private static void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            Farmer player = Game1.player;
            if (!FishingEnding || player.CurrentTool == null || !(player.CurrentTool is FishingRod rod))
                 return;

            if (Practicing)
            {
                FishingEnding = false;
                Practicing = false;

                Bobber.scale = 0.0f;
                Bobber.fadeOut = false;

                Game1.player.completelyStopAnimatingOrDoingAction();
                Game1.exitActiveMenu();
            }
            else if (rod.fishCaught) // fish, seaweed, trash
            {
                CatchInfo.RedrawFishQualitySize(e.SpriteBatch, rod);
            }
            else if (!player.UsingTool) // including missed fish
            {
                FishingEnding = false;
                if (Config.DisableFishingAdjust)
                    return;

                // apply lasting bait/tackle : Only once per catch.
                if (rod.attachments[0] != null && Config.LastingBait > 1 
                    && Game1.random.Next(Config.LastingBait) != 0)
                    rod.attachments[0].Stack = Math.Min(rod.attachments[0].Stack + 1,
                        rod.attachments[0].maximumStackSize());

                if (rod.attachments[1] != null && Config.LastingTackle > 1
                    && rod.attachments[1].uses.Value > 0 && Game1.random.Next(Config.LastingTackle) != 0)
                {
                    rod.attachments[1].uses.Value--;
                    rod.attachments[1].scale.Y = 0.05f * rod.attachments[1].uses.Value;
                }
            }

            Bobber = null;
        }

        private static void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            // Relax max casting power timing tolerance
            if (!Config.DisableFishingAdjust && Game1.player.CurrentTool is FishingRod rod &&  rod.isTimingCast
                && LeftButtonReleased(Reflection.GetField<bool>(rod, "usedGamePadToCast").GetValue())
                && (1.01f - rod.castingPower < Config.RelaxCasting / 100.0))
            {
                rod.castingPower = 1.01f; // maximum casting power
            }
        }

        private static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove || e.Button != FishingPracticeKey
                || !(Game1.player.CurrentTool is FishingRod rod) || rod == null || Game1.player.UsingTool )
                return;

            // Practice Fishing for training, not real fishing.

            Practicing = true;
            Bobber = null;
            rod.isReeling = true;   // simulate fishing hit.

            int fish = PracticeList[Game1.random.Next(PracticeList.Length)];
            if (fish == 0)
                return;
            float fishSize = Math.Max(0.0f, Math.Min(1f, 1 * (float)(1.0 + (double)Game1.random.Next(-10, 10) / 100.0)));
            Game1.activeClickableMenu = new BobberBar(fish, fishSize, false, -1);
        }

        private static bool LeftButtonReleased(bool gamepad)
        {
            return ((!gamepad && Mouse.GetState().LeftButton == ButtonState.Released)
                || (gamepad && Game1.options.gamepadControls && GamePad.GetState(Game1.playerOneIndex).IsButtonUp(Buttons.X))
                || Game1.areAllOfTheseKeysUp(Keyboard.GetState(), Game1.options.useToolButton));
        }

        private static Location FacingDirection()
        {
            int dir = Game1.player.facingDirection;
            int dx = 0, dy = 0;

            if (dir == 1)
                dx = 1;
            else if (dir == 2)
                dy = 1;
            else if (dir == 3)
                dx = -1;
            else
                dy = -1;

            return new Location(dx, dy);
        }
    }
}