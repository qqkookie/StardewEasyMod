using System.Collections.Generic;

namespace EasyPlay
{
    class ModConfig
    {
        // Force next stone will generate ladder
        public bool ForceLadder { get; set; }

        /// <summary>Disable horse whistle and slime horse.</summary>
        public bool DisableEasyHorse { get; set; }

        /// <summary>Key to recall horse to player. Default is 'V' key.</summary>
        public string HorseWhistleKey { get; set; } = "V";

        public bool DisableWhistleSound { get; set; }

        // Restore eating/drinking action and dialogue.
        public bool DisableDontAskToEat { get; set; }

        // To allow eating something not in the food list, press this key before eat it. Default is "F8" key.
        public string EatPrefixKey { get; set; } = "F8";

        // Eat/drink only these in the foods list. Don't ask to eat other edibles.
        // Name list of food to eat, either localized display name or English name.
        public List<string> Foods { get; set; } = new List<string>();

    }
}
