namespace EasySave
{
    /// <summary>The mod configuration.</summary>
    internal class ModConfig
    {
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
