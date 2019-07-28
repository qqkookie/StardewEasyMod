using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;


namespace EasySave
{
    /// <summary>The mod entry point.</summary>
    public class EasySave : Mod
    {
        /*********
        ** Fields
        *********/

        /// <summary>The folder path containing backups</summary>
        internal static readonly string BackupsFolder = Path.Combine(Constants.SavesPath, "..", "BackUps");

        /// <summary>The mod configuration.</summary>
        internal static ModConfig Config;

        internal static IModHelper ModHelper;
        internal static IReflectionHelper Reflection;
        internal static IMonitor Logger;

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
            Logger = Monitor;

            helper.Events.Display.MenuChanged += OnMenuChanged;

            BackupSaves();

            if (!Config.DisableBackupOnSave)
                helper.Events.GameLoop.Saved += SaveAnyTime.OnSaved;

            if (!Config.DisableSaveAnyTime)
                SaveAnyTime.Setup(helper);
        }

        /*********
        ** Private methods
        *********/

        /// <summary>Back up saves</summary>
        internal static void BackupSaves()
        {
            // string folderPath = BackupsPath;
            string zipname, zippath;
            bool inGame = Context.IsWorldReady;

            if (inGame)
            {
                string tag = "Day" + SDate.Now().DaysSinceStart;
                if (Game1.timeOfDay != 600)  // On daily save, TOD is AM 6:00
                    tag += "_" + Game1.timeOfDay.ToString("0000");

                zipname = $"{Constants.SaveFolderName}-{tag}@{DateTime.Now:yyyyMMdd'-'HHmmss}.zip";
            }
            // else if (Config.BackupEveryRun)
            //    zipname = $"Backup@{DateTime.Now:yyyyMMdd'-'HHmmss}.zip";
            else
                zipname = $"Backup@{DateTime.Now:yyyyMMdd}.zip";

            zippath = Path.Combine(BackupsFolder, zipname);
            if (!inGame && new FileInfo(zippath).Exists)
                return;

            try
            {
                // back up saves
                Directory.CreateDirectory(BackupsFolder);
                if (inGame)
                    ZipFile.CreateFromDirectory(Constants.CurrentSavePath, zippath, CompressionLevel.Fastest, true);
                else
                    ZipFile.CreateFromDirectory(Constants.SavesPath, zippath);

                // delete old backups exceeding SaveCount
                var oldbackups = new DirectoryInfo(BackupsFolder)
                    .EnumerateFiles()
                    .OrderByDescending(f => f.CreationTime)
                    .Skip(Config.BackupCount);

                oldbackups.ToList().ForEach(file => file.Delete());
            }
            catch (Exception ex)
            {
                Logger.Log("Error while backing up save files (see log file for details).", LogLevel.Warn);
                Logger.Log(ex.ToString(), LogLevel.Trace);
            }
        }

        private static void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is TitleMenu)
                ;
        }
    }
}