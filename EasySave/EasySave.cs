using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;

using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace EasySave
{
    /// <summary>Save Anywhere + Backup</summary>
    public partial class EasySave : Mod
    {
        /*********
		** Fields
		*********/

        /// <summary>The folder path containing backups</summary>
        private static readonly string BackupsFolder = Path.Combine(Constants.SavesPath, "..", "BackUps");

        /// <summary>The mod configuration.</summary>
        internal static ModConfig Config;

        internal static IModHelper ModHelper;
        internal static IReflectionHelper Reflection;
        private static IMonitor Logger;

        /// <summary>Provides methods for saving and loading game data.</summary>
        private static SaveManager SaveManager;

        /// <summary>The parsed schedules by NPC name.</summary>
        private static readonly IDictionary<string, string> NpcSchedules = new Dictionary<string, string>();

        /// <summary>Whether villager schedules should be reset now.</summary>
        internal static bool ShouldResetSchedules;

        /// <summary>Whether we're performing a non-vanilla save (i.e. not by sleeping in bed).</summary>
        private static bool IsCustomSaving;

        private static List<Monster> monsters;

        private static bool customMenuOpen;

        private static SButton SaveAnytimeKey;

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

            Enum.TryParse(Config.IntroSkipTo, true, out SkipIntro.SkipTo);
            Enum.TryParse(Config.SaveAnytimeKey, true, out SaveAnytimeKey);

            if (SkipIntro.SkipTo != SkipIntro.Screen.Intro && Config.ForgetLastOnTitle)
                helper.Events.GameLoop.ReturnedToTitle +=
                    (s, e) => { SkipIntro.SetLastFile(""); };

            BackupSaves();

            helper.Events.GameLoop.Saved += OnSaved;

            if (!Config.DisableSaveAnyTime)
            {
                SaveManager = new SaveManager();

                helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
                helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
                helper.Events.GameLoop.DayStarted += OnDayStarted;
                helper.Events.Input.ButtonPressed += OnButtonPressed;

                customMenuOpen = false;
            }
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

        /// <summary>Raised after the game finishes writing data to the save file (except the initial save creation).</summary>
        internal static void OnSaved(object sender, SavedEventArgs e)
        {
            // clear custom data after a normal save (to avoid restoring old state)
            if (!Config.DisableSaveAnyTime && !IsCustomSaving)
                SaveManager.ClearData();
            else
                IsCustomSaving = false;

            if (!IsCustomSaving || !Config.DisableBackupSaveAnyTime)
            {
                if (!Config.DisableBackupOnSave)
                    BackupSaves();

                if (Config.ShareOptions)
                {
                    StartupPreferences options = new StartupPreferences();
                    options.loadPreferences(false, false);
                    options.clientOptions = Game1.options;
                    options.savePreferences(false);
                }
            }
        }

        /// <summary>Raised after the player loads a save slot and the world is initialised.</summary>
        private static void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // reset state
            IsCustomSaving = false;
            ShouldResetSchedules = false;

            // load positions
            SaveManager.LoadData();

            SkipIntro.SetLastFile(Constants.SaveFolderName);

            if (Config.ShareOptions)
            {
                StartupPreferences options = new StartupPreferences();
                options.loadPreferences(false, false);
                Game1.options = options.clientOptions;
            }
        }
    }
}
