
namespace EasyFishing
{
    /// <summary>The mod configuration.</summary>

    internal class ModConfig
    {
        /// <summary>Don't show fish probablities info of current fishing spot.</summary>
        public bool DisableFishingSpotInfo { get; set; }

        /// <summary>Don't show info on angling fish and treasure.</summary>
        public bool DisableCatchInfo { get; set; }

        /// <summary>Don't show extra info of fish collections in game menu.</summary>
        public bool DisableExtraInfo { get; set; }

        /// <summary>Show cheat info of fish you don't know yet.</summary>
        public bool UnknownFishCheat { get; set; }

        /// <summary>Display fish size in cm, not inch in English/Korean language.</summary>
        public bool MetricSize { get; set; }

        /// <summary>Key to practice fishing for training. Default is F9.</summary>
        public string FishingPracticeKey { get; set; } = "F11";

        public string[] PracticeFishes { get; set; } = {};

        /// <summary>Disable features below for easier fishing experience.</summary>
        public bool DisableFishingAdjust { get; set; }

        /// <summary>Relexed timing for Max casting power. 0 is normal, 100 is always max.</summary>
        public int RelaxCasting { get; set; } = 10;

        /// <summary>Shorter time until fish bites. 0 is normal, 100 is instant bite.</summary>
        public int QuickBite { get; set; } = 50;

        /// <summary>Pull out the fishing rod automatically when fish bites.</summary>
        public bool AutoHitRod { get; set; } = true;

        /// <summary>Catch fish instantly.</summary>
        public bool InstantCatch { get; set; }

        /// <summary>Force every fish catch to be perfectly executed, even if it wasn't.</summary>
        public bool AlwaysPerfect { get; set; }

        /// <summary>Reduce difficulty of fish. 0 is normal, 90 is the eaiest.</summary>
        public int EasyFish { get; set; } = 30;

        /// <summary>Slow down movement of angling reel: reduction by percent. 0 is normal, 90 is the slowest.</summary>
        public int SlowReel { get; set; } = 30;

        /// <summary>Expand size of green moving bar in percent. 0 is normal.</summary>
        public int ExpandBar { get; set; } = 20;

        /// <summary>Set initial catch progress by percent.</summary>
        public int ProgressStart { get; set; } = 20;

        /// <summary>Boost catch progress rate by percent.
        ///  0 is normal. Below 100, slow progress drain. At 100, no decay. Above 100, faster progress.
        /// </summary>
        public int ProgressBoost { get; set; } = 50;

        /// <summary>Always find treasure.</summary>
        public bool AlwaysTreasure { get; set; }

        /// <summary>On successful fish catch, catch treasure also without fail.</summary>
        public bool CatchTreasure { get; set; } = true;

        /// <summary>Fishing bait lasts multiple times. 2 means double the life.</summary>
        public int LastingBait { get; set; } = 2;

        /// <summary>Fishing tackle lasts longer times. 2 means double the life.</summary>
        public int LastingTackle { get; set; } = 2;

    }
}
