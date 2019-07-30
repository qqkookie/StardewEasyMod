using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace EasySave
{
    using ModMain = EasySave;

    /// <summary>Provides methods for saving and loading game data.</summary>
    public class SaveManager
    {
        /*********
        ** Fields
        *********/

        /// <summary>player data file name.</summary>
        private string CharFile => $"Char-{Constants.SaveFolderName}.json";

        /// <summary>The relative path to the player data file.</summary>
        private string CharRelativePath => Path.Combine("Char", CharFile);

        /// <summary>Full path to the player data file.</summary>
        private string CharFullPath => Path.Combine(ModMain.ModHelper.DirectoryPath, CharRelativePath);

        /// <summary>Copy of player data file in save folder</summary>
        private string CharSavePath => Path.Combine(Constants.CurrentSavePath, CharFile);

        /// <summary>Whether we should save at the next opportunity.</summary>
        private bool WaitingToSave;

        /// <summary> Currently displayed save menu (null if no menu is displayed) </summary>
        private NewSaveGameMenu currentSaveMenu;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        internal SaveManager()
        {
        }

        private void empty(object o, EventArgs args) { }

        /// <summary>Perform any required update logic.</summary>
        internal void Update()
        {
            // perform passive save
            if (this.WaitingToSave && Game1.activeClickableMenu == null)
            {
                this.currentSaveMenu = new NewSaveGameMenu();
                this.currentSaveMenu.SaveComplete += this.CurrentSaveMenu_SaveComplete;
                Game1.activeClickableMenu = this.currentSaveMenu;
                this.WaitingToSave = false;
            }
        }

        /// <summary>Event function for NewSaveGameMenu event SaveComplete</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void CurrentSaveMenu_SaveComplete(object sender, EventArgs e)
        {
            this.currentSaveMenu.SaveComplete -= this.CurrentSaveMenu_SaveComplete;
            this.currentSaveMenu = null;
            //AfterSave.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Clear saved data.</summary>
        internal void ClearData()
        {
            if (File.Exists(CharFullPath))
            {
                File.Delete(CharFullPath);
                File.Delete(CharSavePath);
                File.Delete(CharSavePath + "_old");
            }
            this.RemoveLegacyDataForThisPlayer();
        }

        /// <summary>Initiate a game save.</summary>
        internal void BeginSaveData()
        {
            // save game data
            Farm farm = Game1.getFarm();
            if (farm.shippingBin.Any())
            {

                Game1.activeClickableMenu = new NewShippingMenu(farm.shippingBin, ModMain.Reflection);
                farm.shippingBin.Clear();
                farm.lastItemShipped = null;
                this.WaitingToSave = true;
            }
            else
            {
                this.currentSaveMenu = new NewSaveGameMenu();
                this.currentSaveMenu.SaveComplete += this.CurrentSaveMenu_SaveComplete;
                Game1.activeClickableMenu = this.currentSaveMenu;
            }

            // By Cookie
            // Bookkeeping tag label
            string tag = $"{Constants.SaveFolderName} Day {SDate.Now().DaysSinceStart} {Game1.timeOfDay.ToString("0000")}"
                + $" @ {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";

            // save data to disk
            PlayerData data = new PlayerData
            {
                Tag = tag,
                Time = Game1.timeOfDay,
                Characters = this.GetPositions().ToArray(),
                IsCharacterSwimming = Game1.player.swimming.Value
            };
            ModMain.ModHelper.Data.WriteJsonFile(CharRelativePath, data);

            // By Cookie
            // copy saved char data to game save folder for backup
            if (File.Exists(CharSavePath))
            {
                File.Delete(CharSavePath + "_old");
                File.Move(CharSavePath, CharSavePath + "_old");
            }
            File.Copy(CharFullPath, CharSavePath);

            // clear any legacy data (no longer needed as backup)
            this.RemoveLegacyDataForThisPlayer();
        }

        /// <summary>Load all game data.</summary>
        internal void LoadData()
        {
            // By Cookie
            // copy char data from game save folder back to Char data folder.
            try
            {
                File.Delete(CharFullPath);
                if (File.Exists(CharSavePath))
                    File.Copy(CharSavePath, CharFullPath);
            }
            catch { }

            // get data
            PlayerData data = ModMain.ModHelper.Data.ReadJsonFile<PlayerData>(CharRelativePath);
            if (data == null)
                return;

            // apply
            Game1.timeOfDay = data.Time;
            this.ResumeSwimming(data);
            this.SetPositions(data.Characters);

            // TODO: make it call back
            SaveAnyTime.ShouldResetSchedules = true;

            // Notify other mods that load is complete
            // AfterLoad.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Checks to see if the player was swimming when the game was saved and if so, resumes the swimming animation.</summary>
        private void ResumeSwimming(PlayerData data)
        {
            try
            {
                if (data.IsCharacterSwimming)
                {
                    Game1.player.changeIntoSwimsuit();
                    Game1.player.swimming.Value = true;
                }
            }
            catch
            {
                //Here to allow compatability with old save files.
            }
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Get the current character positions.</summary>
        private IEnumerable<CharacterData> GetPositions()
        {
            // player
            {
                var player = Game1.player;
                string name = player.Name;
                string map = player.currentLocation.uniqueName.Value; //Try to get a unique name for the location and if we can't we are going to default to the actual name of the map.
                if (string.IsNullOrEmpty(map))
                    map = player.currentLocation.Name; //This is used to account for maps that share the same name but have a unique ID such as Coops, Barns and Sheds.
                Point tile = player.getTileLocationPoint();
                int facingDirection = player.facingDirection;

                yield return new CharacterData(CharacterType.Player, name, map, tile, facingDirection);
            }

            // NPCs (including horse and pets)
            foreach (NPC npc in Utility.getAllCharacters())
            {
                CharacterType? type = this.GetCharacterType(npc);
                if (type == null || npc?.currentLocation == null)
                    continue;
                string name = npc.Name;
                string map = npc.currentLocation.Name;
                Point tile = npc.getTileLocationPoint();
                int facingDirection = npc.facingDirection;

                yield return new CharacterData(type.Value, name, map, tile, facingDirection);
            }
        }

        /// <summary>Reset characters to their saved state.</summary>
        /// <param name="positions">The positions to set.</param>
        /// <returns>Returns whether any NPCs changed position.</returns>
        private void SetPositions(CharacterData[] positions)
        {
            // player
            {
                CharacterData data = positions.FirstOrDefault(p => p.Type == CharacterType.Player && p.Name == Game1.player.Name);
                if (data != null)
                {
                    Game1.player.previousLocationName = Game1.player.currentLocation.Name;
                    //Game1.player. locationAfterWarp = Game1.getLocationFromName(data.Map);
                    Game1.xLocationAfterWarp = data.X;
                    Game1.yLocationAfterWarp = data.Y;
                    Game1.facingDirectionAfterWarp = data.FacingDirection;
                    Game1.fadeScreenToBlack();
                    Game1.warpFarmer(data.Map, data.X, data.Y, false);
                    Game1.player.faceDirection(data.FacingDirection);
                }
            }

            // NPCs (including horse and pets)
            foreach (NPC npc in Utility.getAllCharacters())
            {
                // get NPC type
                CharacterType? type = this.GetCharacterType(npc);
                if (type == null)
                    continue;

                // get saved data
                CharacterData data = positions.FirstOrDefault(p => p.Type == type && p.Name == npc.Name);
                if (data == null)
                    continue;

                // update NPC
                Game1.warpCharacter(npc, data.Map, new Point(data.X, data.Y));
                npc.faceDirection(data.FacingDirection);
            }
        }

        /// <summary>Get the character type for an NPC.</summary>
        /// <param name="npc">The NPC to check.</param>
        private CharacterType? GetCharacterType(NPC npc)
        {
            if (npc is Monster)
                return null;
            if (npc is Horse)
                return CharacterType.Horse;
            if (npc is Pet)
                return CharacterType.Pet;
            return CharacterType.Villager;
        }

        /// <summary>Remove legacy save data for this player.</summary>
        private void RemoveLegacyDataForThisPlayer()
        {
            DirectoryInfo dataDir = new DirectoryInfo(Path.Combine(ModMain.ModHelper.DirectoryPath, "Save_Data"));
            DirectoryInfo playerDir = new DirectoryInfo(Path.Combine(dataDir.FullName, Game1.player.Name));
            if (playerDir.Exists)
                playerDir.Delete(recursive: true);
            if (dataDir.Exists && !dataDir.EnumerateDirectories().Any())
                dataDir.Delete(recursive: true);
        }
    }
}
