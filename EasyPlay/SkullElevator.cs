using Microsoft.Xna.Framework;
using xTile;
using xTile.Tiles;

using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyPlay
{
    using ModMain = EasyPlay;

    internal static class SkullElevator
    {
        //internal static ModConfig Config => Config;
        internal static IReflectionHelper Reflection => ModMain.Reflection;

        internal const int ElevatorStep = 5;
        internal const float DifficultyScale = 1.0f;
        internal const int ELEVATORSIZE = 121;

        internal static void Setup()
        {
            ModMain.Events.Player.Warped += MineEvents_MineLevelChanged;
            ModMain.Events.Display.MenuChanged += MenuChanged;
            ModMain.Events.GameLoop.SaveLoaded += SetUpSkullCave;
        }

        private static void SetUpSkullCave(object sender, SaveLoadedEventArgs e)
        {
            if (!Game1.hasLoadedGame || Game1.CurrentEvent != null)
                return;

            GameLocation skullcave = Game1.getLocationFromName("SkullCave");
            TileSheet tileSheet = Game1.getLocationFromName("Mine").map.GetTileSheet("untitled tile sheet");
            skullcave.map.AddTileSheet(new TileSheet("z_path_objects_custom_sheet", skullcave.map,
                tileSheet.ImageSource, tileSheet.SheetSize, tileSheet.TileSize));
            skullcave.map.DisposeTileSheets(Game1.mapDisplayDevice);
            skullcave.map.LoadTileSheets(Game1.mapDisplayDevice);

            skullcave.setMapTileIndex(4, 3, 112, "Buildings", 2);
            skullcave.setMapTileIndex(4, 2, 96, "Front", 2);
            skullcave.setMapTileIndex(4, 1, 80, "Front", 2);
            skullcave.setMapTile(4, 3, 112, "Buildings", "MineElevator", 2);
            skullcave.setMapTile(4, 2, 96, "Front", "MineElevator", 2);
            skullcave.setMapTile(4, 1, 80, "Front", "MineElevator", 2);
        }

        private static void MenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!(e.NewMenu is MineElevatorMenu) || Game1.currentLocation.Name == "Mine"
                || (e.NewMenu is SkullElevatorMenu || e.NewMenu is SkullElevatorMenuScroll))
                return;
            if (Game1.currentLocation is MineShaft)
            {
                MineShaft currentLocation = Game1.currentLocation as MineShaft;
                if ((currentLocation != null ? (currentLocation.mineLevel <= 120 ? 1 : 0) : 0) != 0)
                    return;
            }
            if (Game1.player.deepestMineLevel > 120 + ELEVATORSIZE * ElevatorStep)
                Game1.activeClickableMenu = new SkullElevatorMenuScroll(); //ElevatorStep, Config.MineDifficulty);
            else
                Game1.activeClickableMenu = new SkullElevatorMenu(); // ElevatorStep, Config.MineDifficulty);
        }

        private static void MineEvents_MineLevelChanged(object sender, WarpedEventArgs e)
        {
            if (!(e.NewLocation is MineShaft) || !e.IsLocalPlayer)
                return;

            // Monitor.Log("Current lowest minelevel of player " + Game1.player.deepestMineLevel, (LogLevel)1);
            // Monitor.Log("Value of MineShaft.lowestMineLevel " + MineShaft.lowestLevelReached, (LogLevel)1);
            // Monitor.Log("Value of current mineShaft level " + newLocation.mineLevel, (LogLevel)1);

            if (!Game1.hasLoadedGame || Game1.mine == null || !(Game1.currentLocation is MineShaft mine)
                || Game1.CurrentMineLevel <= 120 || (Game1.CurrentMineLevel - 120) % ElevatorStep != 0 )
                return;

            // MineShaft mine = Game1.currentLocation as MineShaft;
            TileSheet tileSheet = Game1.getLocationFromName("Mine").map.GetTileSheet("untitled tile sheet");
            mine.map.AddTileSheet(new TileSheet("z_path_objects_custom_sheet", mine.map, tileSheet.ImageSource, tileSheet.SheetSize, tileSheet.TileSize));
            mine.map.DisposeTileSheets(Game1.mapDisplayDevice);
            mine.map.LoadTileSheets(Game1.mapDisplayDevice);

            Vector2 ladder = FindUpLadder(mine);
            int elevX = (int)ladder.X + 1;
            int elevY = (int)ladder.Y - 3;
            typeof(MineShaft).GetMethods();
            mine.setMapTileIndex(elevX, elevY + 2, 112, "Buildings", 1);
            mine.setMapTileIndex(elevX, elevY + 1, 96, "Front", 1);
            mine.setMapTileIndex(elevX, elevY, 80, "Front", 1);
            mine.setMapTile(elevX, elevY, 80, "Front", "MineElevator", 1);
            mine.setMapTile(elevX, elevY + 1, 96, "Front", "MineElevator", 1);
            mine.setMapTile(elevX, elevY + 2, 112, "Buildings", "MineElevator", 1);
            Reflection.GetMethod(mine, "prepareElevator", true).Invoke(new object[0]);

            // Point tile = Utility.findTile((GameLocation)mine, 80, "Buildings");
            // Monitor.Log("x " + tile.X + " y " + tile.Y, (LogLevel)1);
        }

        private static Vector2 FindUpLadder(MineShaft ms)
        {
            Map map = ms.map;
            for (int index1 = 0; index1 < map.GetLayer("Buildings").LayerHeight; ++index1)
            {
                for (int index2 = 0; index2 < map.GetLayer("Buildings").LayerWidth; ++index2)
                {
                    if (map.GetLayer("Buildings").Tiles[index2, index1] != null && map.GetLayer("Buildings").Tiles[index2, index1].TileIndex == 115)
                        return new Vector2(index2, (index1 + 1));
                }
            }
            return Reflection.GetField<Vector2>(ms, "tileBeneathLadder", true).GetValue();
        }
    }
}
