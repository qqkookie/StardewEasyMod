using System;

using StardewModdingAPI;

namespace EasyMine
{
    public class EasyMine : Mod
    {
        internal static ModConfig Config;
        internal static IReflectionHelper Reflection;

        //internal static IModHelper ModHelper;
        //internal static ITranslationHelper Translation;
        // internal static IMonitor Logger;

        public override void Entry(IModHelper helper)
        {
            Reflection = helper.Reflection;
            Config = helper.ReadConfig<ModConfig>();

            SkullElevator.SetupSkullElevator(helper.Events);

            LadderFinder.SetupLadderFinder(helper.Events);
        }

        /*
        {
            Translation = helper.Translation;
            string startingMessage = Translation.Get("template.start", new { mod = helper.ModRegistry.ModID, folder = helper.DirectoryPath });
            Monitor.Log(startingMessage, LogLevel.Trace);

            config = helper.ReadConfig<Config>();

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            e.Button.TryGetKeyboard(out Keys keyPressed);
                

            if (keyPressed.Equals(config.debugKey))
                Monitor.Log(Translation.Get("template.key"), LogLevel.Info);
        }
        */
    }
}
