using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Tools;
using StardewValley.TerrainFeatures;
using StardewValley.Locations;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyToolbar
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/

        /// <summary>The mod configuration.</summary>
        private static ModConfig Config;

        /// <summary>The toolbar shifting key and other bindings.</summary>
        private static SButton KeyShiftDown, KeyShiftUp, KeyDeselectTool, KeyAutoTool;

        /// <summary>Current toolbar slot index changed by mouse wheel scroll.</summary>
        private static int NewToolIndex = -1;

        /// <summary>Number of slots in a toolbar row </summary>
        private const int TBSlots = 12;

        private static bool AutoToolActive = false;
        private static bool Found = false; 
        private static int wc_cache = -1;       // watercan index

        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides methods for interacting with the mod directory, such as read/writing a config file or custom JSON files.</param>
        public override void Entry(IModHelper helper)
        {
            // Read config
            Config = helper.ReadConfig<ModConfig>();

            Enum.TryParse(Config.ShiftDownKey, true, out KeyShiftDown);
            Enum.TryParse(Config.ShiftUpKey, true, out KeyShiftUp);
            Enum.TryParse(Config.DeselectToolKey, true, out KeyDeselectTool);
            Enum.TryParse(Config.AutoToolKey, true, out KeyAutoTool);
            if ( !(new List<string>() { "Axe", "Scythe", "Hoe" }).Contains(Config.WeedsTool))
                Config.WeedsTool = "Axe";

            // hook events
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Input.ButtonReleased += OnButtonReleased;
            if (Config.ScrollToEmpty)
            {
                helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
                helper.Events.Input.MouseWheelScrolled += OnMouseWheelScrolled;
            }
        }

        /*********
        ** Private methods
        *********/

        /// <summary>The method invoked when the player presses a button.</summary>
        private static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove)
                return;

            if (e.Button == KeyAutoTool)
            {
                TrySelectTool();
                AutoToolActive = true;
                return;
            }
            else if (e.Button == KeyDeselectTool)
            {
                if (Game1.player.CurrentToolIndex < 999)
                {
                    Game1.playSound("dwop");
                    Game1.player.CurrentToolIndex += 99 * TBSlots;
                }
                return;
            }

            // Rotate the displayed row in the toolbar.
            if (e.Button == KeyShiftDown)
                Game1.player.shiftToolbar(true);
            else if (e.Button == KeyShiftUp)
                Game1.player.shiftToolbar(false);
            else
                return;

            if ( Config.ResetOnShift)
                Game1.player.CurrentToolIndex = 0;

            if ( Config.DeselectOnShift)
                // Invalid tool index to deselect current tool 
                Game1.player.CurrentToolIndex += 99* TBSlots;
        }

        private static void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button == KeyAutoTool)
                AutoToolActive = false;
        }

        // Scroll to empty toolbar slot on mouse wheel scroll
        private static void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (!Context.CanPlayerMove || Game1.activeClickableMenu != null 
                || Game1.player.UsingTool || Game1.pickingTool)
                return;

            NewToolIndex = Game1.player.CurrentToolIndex;

            if (e.Delta < 0 && !Game1.options.invertScrollDirection)
                ++NewToolIndex;
            else
                --NewToolIndex;

            NewToolIndex = (NewToolIndex + TBSlots) % TBSlots;
        }

        private static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (AutoToolActive && e.IsMultipleOf(10))
                TrySelectTool();

            if (NewToolIndex < 0)
                return;

            if ( !Game1.player.UsingTool && !Game1.pickingTool )
                Game1.player.CurrentToolIndex = NewToolIndex;

            NewToolIndex = -1;
        }

        private static void TrySelectTool()
        {
            Farmer player = Game1.player;
            if (!Context.CanPlayerMove || player.UsingTool || Game1.pickingTool)
                return;

            GameLocation map = Game1.currentLocation;
            bool InHome = map is Farm || map.IsGreenhouse;
            Found = false;

            // choose cursor location or tool hit location.
            Vector2 tile = Game1.player.isRidingHorse() ? Game1.currentCursorTile 
                : player.GetToolLocation(ignoreClick: false) / Game1.tileSize;
            tile = new Vector2((float)Math.Floor(tile.X +0.000001f), (float)Math.Floor(tile.Y + 0.000001f));

            if (map.Objects.TryGetValue(tile, out StardewValley.Object obj))
            {
                if (obj.Name == "Artifact Spot")
                    SelectTool("Hoe");
                else if (obj.Name == "Stone")
                    SelectTool("Pickaxe");
                else if (obj.Name == "Twig")
                    SelectTool("Axe");
                else if (obj.Name == "Weeds")
                    SelectTool(Config.WeedsTool);
                else if (!InHome && obj.Name == "Barrel")
                    SelectTool("Weapon");
            }
            else if (map.terrainFeatures.TryGetValue(tile, out TerrainFeature terra))
            {
                if (terra is Tree)
                    SelectTool("Axe");
                else if (terra is Grass)
                    SelectTool("Scythe");
                else if (InHome && terra is GiantCrop)
                    SelectTool("Axe");
                else if (map.isTileHoeDirt(tile) && terra is HoeDirt dirt)
                {
                    if (!InHome)
                        SelectTool("Pickaxe");
                    else if (dirt.crop != null)
                    {
                        if (dirt.readyForHarvest() || dirt.crop.dead.Value)
                            SelectTool("Scythe");
                        else
                            SelectTool("Watering Can");
                    }
                    else if (SelectTool("Seeds") < 0)
                        SelectTool(Config.DefaultTool);
                }
            }
            else if ((wc_cache < 0 || (player.Items[wc_cache] is WateringCan wc && wc.WaterLeft < wc.waterCanMax - 4))
    && IsWaterSource(tile))
            {
                SelectTool("Watering Can");
                wc_cache = NewToolIndex;
            }
            
            //else if (map.doesTileHaveProperty((int) tile.X, (int)tile.Y, "Diggable", "Back") != null)
            //    SelectTool("Hoe");

            else if (map is Farm farm)
            {
                int ri = ResourceClumpIndex(farm.resourceClumps.ToList(), tile);
                if (ri == ResourceClump.stumpIndex || ri == ResourceClump.hollowLogIndex)
                    SelectTool("Axe");
                else if (ri == ResourceClump.meteoriteIndex || ri == ResourceClump.boulderIndex)
                    SelectTool("Pixkaxe");
            }
            else if (map is MineShaft mine)
            {
                if (ResourceClumpIndex(mine.resourceClumps.ToList(), tile) > 0)
                    SelectTool("Pixkaxe");
            }
            else if (map is Woods woods)
            {
                if (ResourceClumpIndex(woods.stumps.ToList(), tile) > 0)
                    SelectTool("Axe");
            }
            else if (map is Forest forest)
            {
                if (forest.log.tile.Value == tile)  // TODO: something strange..
                    SelectTool("Axe");
            }

            if (!Found)   // default selection
            {
                if (map is MineShaft || map.Name.StartsWith("UndergroundMine"))
                    SelectTool("Weapon");
                else
                    SelectTool(Config.DefaultTool);    
            }
        }

        public static int SelectTool(string tooltype, bool allrows = false)
        {
            if (String.IsNullOrEmpty(tooltype))
                return -1;

            // MeleeWeapon is subtype of Tool, Scythe is also MeleeWeapon.
            // MeleeWeapon can be used as makeshift scythe.
            bool weapontype = (tooltype == "Weapon" || tooltype == "Scythe");
            int iTool = -1; int alt = -1;
            var inven = Game1.player.Items.GetEnumerator();
            for (int ii = 0; inven.MoveNext() && (allrows || ii < TBSlots); ii++)
            {
                Item obj = inven.Current;
                if (obj != null && obj is Tool && 
                    (obj.Name.EndsWith(tooltype) || (weapontype && (obj is MeleeWeapon))))
                {
                    if (!((tooltype == "Weapon" && obj.Name == "Scythe")
                        || (tooltype == "Scythe" && obj.Name != "Scythe")))
                    {
                        iTool = ii;
                        break;
                    }
                    else if (alt < 0)
                        alt = ii;
                }
            }
            if (iTool < 0)
            {
                if (weapontype && alt >= 0)
                    iTool = alt;     // if no weapon, use scythe.
                else if (tooltype == "none" || tooltype == "0")
                    iTool = Game1.player.CurrentToolIndex + 99 * TBSlots;
                // numbered as 1-12, not zero based.
                else if (Int32.TryParse(tooltype, out int slot) && slot > 0 && slot <= TBSlots)
                    iTool = slot - 1;
                else
                    return -1;
            }

            if ((Game1.player.CurrentToolIndex < 999 || iTool < 999)  
                && (Game1.player.CurrentToolIndex != iTool))
            {
                NewToolIndex = iTool;
                Game1.playSound("toolSwap");
            }
            Found = true;
            return iTool;
        }

        private static bool IsWaterSource(Vector2 tile)
        {
            GameLocation map = Game1.currentLocation;
            if (map.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Water", "Back") != null
                || map.doesTileHaveProperty((int)tile.X, (int)tile.Y, "WaterSource", "Back") != null)
                return true;

            if (!(map is Farm || map.IsGreenhouse) || !(map is BuildableGameLocation site))
                return false;

            return (site.getBuildingAt(tile) != null
                    && site.getBuildingAt(tile).buildingType.Equals("Well")
                    && site.getBuildingAt(tile).daysOfConstructionLeft.Value <= 0);
        }

        private static int ResourceClumpIndex(List<ResourceClump> rclist, Vector2 tile)
        {
            var rcl = rclist.Where(r => r.occupiesTile((int)tile.X, (int)tile.Y));
            return (rcl.Count() > 0 ? rcl.First().parentSheetIndex.Value : -1);
        }

    }
}
