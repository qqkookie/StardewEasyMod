using System;
using System.Collections.Generic;
using System.Linq;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;


namespace EasyPlay
{
    using ModMain = EasyPlay;

    public static class DontAskToEat
    {
        private static ModConfig Config => ModMain.Config;

        internal static  bool DisableDontAskToEat;   // disable eat blocking
        internal static List<string> Foods;          // Edible foods name list
        internal static SButton EatPrefix;           // key to allow to eat food on hand
        internal static bool AllowEat = false;

        internal static void Setup()
        { 
            Enum.TryParse(Config.EatPrefixKey, true, out EatPrefix);

            DisableDontAskToEat = Config.DisableDontAskToEat;

            Foods = Config.Foods.Select(s => s.ToLower()).ToList();

            ModMain.Events.Display.MenuChanged += OnMenuChanged;
            ModMain.Events.Input.ButtonPressed += OnButtonPressed;

            /*
            // To generate full list of ediable candiates
            List<string> edibles = new List<string>();
            foreach (var item in Game1.objectInformation)
            {
                string[] parts = item.Value.Split('/');
                if (Int32.Parse(parts[1]) > 0 && Int32.Parse(parts[2]) > -300)
                    edibles.Add(parts[4]);
            }
            */
        }

        private static void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!Context.IsWorldReady || DisableDontAskToEat || Game1.currentLocation.lastQuestionKey != "Eat")
                return;

            if (e.NewMenu is DialogueBox dlg)
            {
                StardewValley.Object activeobj = Game1.player.ActiveObject;
                if (activeobj != null && activeobj.Price > 0 && activeobj.Edibility > -300
                    && !Foods.Contains(activeobj.DisplayName.ToLower()) && !Foods.Contains(activeobj.Name.ToLower()))
                {
                    if (!AllowEat)
                    {
                        // close Eat? Y/N quesion dialogue box.
                        ModMain.Reflection.GetField<bool>(dlg, "transitioning", true).SetValue(false);
                        ModMain.Reflection.GetField<int>(dlg, "safetyTimer", true).SetValue(0);
                        ModMain.Reflection.GetField<int>(dlg, "selectedResponse", true).SetValue(1);
                        dlg.receiveLeftClick(0, 0, false);
                    }
                    else
                        AllowEat = false;
                }
            }
            else if (e.OldMenu is DialogueBox && Game1.player.isEating )
            {
                // If player chose Yes to eat, add it to the foods list.

                Item item = Game1.player.itemToEat;
                if ( item != null && !Foods.Contains(item.DisplayName.ToLower()) && !Foods.Contains(item.Name.ToLower()))
                {
                    Foods.Add(item.DisplayName.ToLower());

                    Config.Foods.Add(item.DisplayName);
                    ModMain.ModHelper.WriteConfig(Config);
                }
            }
        }

        //  To eat something not in the foods list, select the edible and press prefix key 
        //  followed by action button (click right). Then select yes on the eat confirm dialogue.
        //  Then the new edible will be added to the current foods list.
        internal static void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Context.IsPlayerFree && e.Button == EatPrefix
                && (AllowEat || Game1.player.ActiveObject != null))
            {
                AllowEat = !AllowEat;
                Game1.playSound("dwoop");
            }
        }
    }
}
