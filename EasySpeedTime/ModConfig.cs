namespace EasySpeedTime
{
    /// <summary>The mod configuration.</summary>
    public class ModConfig
    {
        /// <summary>Speed up players move (run/walk). 0 to 8. 0 for normal speed, 5 for 100% speed up.</summary>
        public int MoveSpeedUp { get; set; } = 2;

        /// <summary>Speed up player's horse riding. 0 for normal horse speed.</summary>
        public int HorseSpeedUp { get; set; } = 2;

        // Addtional move speed modifier for hoe dirt and each road types: 11 types
        //  { hoe_dirt = 0, wood = 1, stone, ghost, iceTile, straw = 5,
        //      gravel = 6, boardwalk, colored_cobblestone, cobblestone, steppingStone = 10 };
        public int[] RoadBuff { get; set; } = { -1 , 0, 1, -1, -1, -1,    0, 1, 2, 2, -1 };

        /// <summary>Key to jump over things or go into/out of water. Default is 'Space' key.</summary>
        public string JumpKey { get; set; } = "Space";

        /// <summary>Disable automatic fence gate open/close.</summary>
        public bool DisableAutoGate { get; set; }

        /// <summary>Update in-game daytime clock frequently, every 2 min instead of 10 mins.</summary>
        public bool DisableRunningClock { get; set; }

        /// <summary>Disable time freeze listed below</summary>
        public bool DisableTimeFreeze { get; set; }

        /// <summary>Key to toggle time pause/resume. Default is 'O' key.</summary>
        public string PauseKey { get; set; } = "O";

        /// <summary>Idle time in seconds before time freeze, 0 for no idle time freeze. Default 10 secs.</summary>
        public int IdleTime { get; set; } = 10;

        /// <summary>Idle time in seconds before game is paused, 0 for no game pause. Default 200 secs.</summary>
        public int PauseTime { get; set; } = 200;

        /// <summary>Freeze time while in-door like farm house or store.</summary>
        public bool FreezeInDoor { get; set; } = true;

        /// <summary>Freeze time in the Mine/SkullCave.</summary>
        public bool FreezeInMineCave { get; set; } = true;

        /// <summary>Freeze time when swimming.</summary>
        public bool FreezeOnSwimming { get; set; } = false;

    }
}
