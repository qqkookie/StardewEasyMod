using System.Collections.Generic;

namespace EasyPlay
{
    class ModConfig
    {
        /// <summary>Find Ladder: Force breaking next stone will generate ladder.</summary>
        public bool ForceLadder { get; set; }

        /// <summary>Easy Horse: Disable horse whistle and slim horse.</summary>
        public bool DisableEasyHorse { get; set; }

        /// <summary>Horse Whistle: Key to summon horse to player. Default is 'V' key.</summary>
        public string HorseWhistleKey { get; set; } = "V";

        /// <summary>Horse Whistle: Disable whistle sound.</summary>
        public bool DisableWhistleSound { get; set; }

        /// <summary>Don't Ask to Eat: Restore eating/drinking action and dialogue.</summary>
        public bool DisableDontAskToEat { get; set; }

        /// <summary>
        /// Don't Ask to Eat: vTo allow eating something not in the food list,
        /// press this key before eat it. Default is "F8" key.
        /// </summary>
        public string EatPrefixKey { get; set; } = "F8";

        /// <summary>
        /// Don't Ask to Eat: Eat/drink only these in the foods list. Don't ask to eat other edibles.
        /// Name list of food to eat, either localized display name or English name.
        /// </summary>
        public List<string> Foods { get; set; } = new List<string>();

    }
}
