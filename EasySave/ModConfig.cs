namespace EasySave
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
        /// <summary>When skipping intro, which screen to go?</summary>
        /// <remarks>Can be one of "Title", "Load", "Host", "Join", "AutoLoad" and "AutoHost".</remarks>
        public string IntroSkipTo { get; set; } = "Title";

        /// <summary>Last loaded game. Set and updated automatically.</summary>
        public string LastLoadedSave { get; set; }

        /// <summary>When returned to title screen, foret last loaded game.</summary>
        public bool ForgetLastOnTitle { get; set; }

        /// <remarks>Preferred "Windowed" screen resolution and postion of title screen. 
        /// list of 4 numbers of width, hieght and optional X, Y postion.</remarks>
        public int[] TitleWindow { get; set; } = { 1920, 1080, -1, -1 };

        /// <summary>Share common game options among saves. It saves/loads option changes.</summary>
        public bool ShareOptions { get; set; }

        /// <summary>Total number of save backups to keep. Last 50 backups by default.</summary>
        public int BackupCount { get; set; } = 50;

        /// <summary>Don't backup on in-game nightly (new day) game save.</summary>
        public bool DisableBackupOnSave { get; set; }

        /// <summary>Backup every time before game runs. Default is once per each calender day.</summary>
        // public bool BackupEveryRun { get; set; }

        /// <summary>Disable saving anytime on key press.</summary>
        public bool DisableSaveAnyTime { get; set; }

        /// <summary>Disable backup of save file by saving anytime.</summary>
        public bool DisableBackupSaveAnyTime { get; set; }

        /// <summary>The key which initiates a save at anytime. Default is 'V' key.</summary>
        public string SaveAnytimeKey { get; set; } = "F9";

    }
}
